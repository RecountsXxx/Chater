using MessengerLiblary;
using Microsoft.VisualBasic.ApplicationServices;
using Newtonsoft.Json;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using Message = MessengerLiblary.Message;
using User = MessengerLiblary.User;

namespace MiniMessenger
{
    public partial class Form1 : Form
    {
        private string sqlString = "Data Source=DESKTOP-PGENPK7\\SQLEXPRESS01;Initial Catalog=MessengerUsers;Integrated Security=True;";
        public string ServerDiskPath = "C://MessengerServer/";
        public TcpListener listener = null;
        public Mutex mutex = new Mutex();
        public bool serverStopped = false;
        public Dictionary<int, DateTime> lastMessagesClient = new Dictionary<int, DateTime>();
        public System.Windows.Forms.Timer timerOnline = new System.Windows.Forms.Timer();

        public string pathDb = string.Empty;
        public string nameDb = string.Empty;
       

        public Form1()
        {     
            //исправить путь бд
            InitializeComponent();
            startBtn_Click(null, null);

            if (!File.Exists("DbPath.txt"))
            {
                using (StreamWriter writer = new StreamWriter("DbPath.txt", false))
                {
                    writer.WriteLine(sqlString);
                }
            }
            else
            {
                using(StreamReader reader = new StreamReader("DbPath.txt"))
                {
                    sqlString = reader.ReadLine();
                }
            }

            timerOnline.Interval = 1000;
            timerOnline.Tick += TimerOnline_Tick;
            timerOnline.Start();

          
        }

        private void TimerOnline_Tick(object? sender, EventArgs e)
        {
            foreach (var item in lastMessagesClient)
            {
                if ((DateTime.Now - item.Value).TotalSeconds > 30)
                {
                    SetNotActive(item.Key);
                }
            }
        }

        private void createDBBtn_Click(object sender, EventArgs e)
        {
            if(nameDbTextBox.Text.Length > 2)
            {
                nameDb = nameDbTextBox.Text;
                pathDb = pathDb + nameDb + ".mdf";

                using (SqlConnection connection = new SqlConnection($"Data Source=(LocalDB)\\MSSQLLocalDB;Integrated Security=True"))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand($"CREATE DATABASE {nameDb} ON PRIMARY (NAME={nameDb}_Data, FILENAME='{pathDb}')", connection);
                    command.ExecuteNonQuery();
                }
                sqlString = $"Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename={pathDb};Integrated Security=True";
                using (StreamWriter writer = new StreamWriter("DbPath.txt", false))
                {
                    writer.WriteLine(sqlString);
                }

                using (SqlConnection connection = new SqlConnection(sqlString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand($"create table UserTable(\r\n[Id] int not null primary key identity(1,1),\r\n[Name] nvarchar(100) null,\r\n[Password] nvarchar(50) null,\r\n[Avatar] nvarchar(100) null,\r\n[IsOnline] bit null,\r\n[LastReceived] datetime null,\r\n[IpAddress] nvarchar(20) NULL,\r\n[Port] int NULL,\r\n);\r\n\r\ncreate table MessageTable(\r\n[Id] int not null primary key identity(1,1),\r\n[Text] nvarchar(max) null,\r\n[TimeStamp] datetime not null,\r\n[SenderId] int not null foreign key references UserTable([Id]),\r\n[ReceivedId] int not null foreign key references UserTable([Id]),\r\n[VoiceMessage] nvarchar(100) null,\r\n[ImageMessage] nvarchar(100) null,\r\n[VideoMessage] nvarchar(100) null,\r\n[ZipMessage] nvarchar(100) null,\r\n);\r\n\r\ncreate table FriendTable(\r\n[Id] int not null,\r\n[UserId] int null,\r\n);\r\n\r\ncreate table BlockedUsersTable(\r\n[UserId] int not null,\r\n[BlockedId] int null,\r\n);\r\n\r\ncreate table RequestedUsersTable(\r\n[UserId] int not null,\r\n[ToRequestsId] int null,\r\n);\r\n\r\ncreate table BusyIpAndPorts(\r\n[Id] int not null identity(1,1) primary key,\r\n[IpAdress] nvarchar(20) null,\r\n[Port] int null,\r\n);\r\n\r\ncreate table NotificationQueue(\r\n[Id] int not null primary key identity(1,1),\r\n[MessageId] int foreign key references MessageTable([Id]),\r\n);\r\n", connection);
                    command.ExecuteNonQuery();

                    command = new SqlCommand($"CREATE TRIGGER trg_MessageTable_NotificationQueue\r\nON MessageTable\r\nAFTER INSERT\r\nAS\r\nBEGIN\r\n    INSERT INTO NotificationQueue (MessageId)\r\n    SELECT id\r\n    FROM inserted;\r\nEND;",connection);
                    command.ExecuteNonQuery();
                }


            }
        }
        private void pathDBbutton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folder = new FolderBrowserDialog();
            if(folder.ShowDialog() == DialogResult.OK)
            {
                if(folder.SelectedPath.Length > 2)
                {
                    pathDb = folder.SelectedPath;
                }
            }
        }
        private async void startBtn_Click(object sender, EventArgs e)
        {
            serverStopped = false;
            closeBtn.Enabled = true;
            startBtn.Enabled = false;

            listener = new TcpListener(System.Net.IPAddress.Parse(ipAdressTextBox.Text), (int)portNumeric.Value);
            listener.Start();
           
            consoleListBox.Items.Add("Server started!");
            await Task.Run(() =>
            {

                while (true)
                {
                    if (!serverStopped)
                    {
                        TcpClient client = listener.AcceptTcpClient();
                        Thread thread = new Thread(() => HandleClient(client));
                        thread.Start();
                    }
                    else
                        break;
                    
                }
            });
        }
        private void closeBtn_Click(object sender, EventArgs e)
        {
            serverStopped = true;
            startBtn.Enabled = true;
            closeBtn.Enabled = false;
            consoleListBox.Items.Add("Server is stopped!");
            listener.Stop();
        }
        private void HandleClient(TcpClient client)
        {
            timerOnline.Start();
            NetworkStream stream = client.GetStream();
            while (true)
            {
                try
                {
                    StringBuilder builder = new StringBuilder();
                    int lengthBuffer = 0;
                    byte[] buffer = new byte[1024];
                    do
                    {
                        lengthBuffer = stream.Read(buffer, 0, buffer.Length);
                        builder.Append(Encoding.UTF8.GetString(buffer, 0, lengthBuffer));

                    } while (stream.DataAvailable);

                    string response = builder.ToString();
          
                  
                    if (response.Contains("Login"))
                    {
                        string username = response.Split(" ")[2];
                        string password = response.Split(" ")[4];
                        User user = LoginUser(username, password).Result;
                        stream.Write(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(user)));
                    }
                    if (response.Contains("Register"))
                    {
                        string username = response.Split(" ")[2];
                        string password = response.Split(" ")[4];
                        string result = RegisterUser(username, password).Result;
                        stream.Write(Encoding.UTF8.GetBytes(result));
                    }
                    if (response.Contains("GetFriends"))
                    {
                        int id = Convert.ToInt32(response.Split(" ")[2]);
                        if (lastMessagesClient.ContainsKey(id))
                        {
                            lastMessagesClient.Remove(id);
                            lastMessagesClient.Add(id, DateTime.Now);
                        }
                        else
                        {
                            lastMessagesClient.Add(id, DateTime.Now);
                        }
                        
                        SetActive(id);
                        List<Friend> friends = GetFriends(id).Result;
                        stream.Write(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(friends)));
                    }
                    if (response.Contains("Add Friend"))
                    {
                        int userId = Convert.ToInt32(response.Split(" ")[3]);
                        int friendId = Convert.ToInt32(response.Split(" ")[5]);
                        stream.Write(Encoding.UTF8.GetBytes(AddFriend(userId, friendId).Result));
                    }
                    if (response.Contains("Get user per name"))
                    {
                        string name = response.Split(" ")[5];
                        User user = GetUserPerName(name).Result;
                        stream.Write(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(user)));
                    }
                    if (response.Contains("Get user per id"))
                    {
                        int userId = Convert.ToInt32(response.Split(" ")[5]);
                        User user = GetUserPerId(userId).Result;
                        stream.Write(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(user)));
                    }
                    if (response.Contains("Set not active"))
                    {
                        int userId = Convert.ToInt32(response.Split(" ")[4]);
                        SetNotActive(userId);
                    }
                    if (response.Contains("Get message"))
                    {
                        int userId = Convert.ToInt32(response.Split(" ")[3]);
                        int receivedId = Convert.ToInt32(response.Split(" ")[5]);
                        stream.Write(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(GetMessages(userId, receivedId).Result)));
                    }
                    if (response.Contains("Get next messages"))
                    {
                        int userId = Convert.ToInt32(response.Split(" ")[4]);
                        int receivedId = Convert.ToInt32(response.Split(" ")[6]);
                        int messageFetch = Convert.ToInt32(response.Split(" ")[8]);
                        stream.Write(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(GetMessages(userId, receivedId,messageFetch).Result)));
                    }
                    if (response.Contains("Send message - "))
                    {
                        builder = new StringBuilder();
                        byte[] messageBuffer = new byte[1024];
                        string format = response.Split(" ")[3];

                        int lenBuffer = stream.Read(messageBuffer, 0, messageBuffer.Length);
                        string json = Encoding.UTF8.GetString(messageBuffer, 0, lenBuffer);
                        if (json != response)
                        {
                            Invoke(new Action(() => { consoleListBox.Items.Add("OMG - " + json); }));
                            stream.Write(Encoding.UTF8.GetBytes(SendMessage(stream, json, format).Result.ToString()));
                        }
                    }
                    if (response.Contains("Check chat message time"))
                    {
                        int receivedId = Convert.ToInt32(response.Split(" ")[5]);
                        int senderId = Convert.ToInt32(response.Split(" ")[7]);
                        stream.Write(Encoding.UTF8.GetBytes(CheckMessagesTime(receivedId,senderId).Result));
                    }
                    if (response.Contains("Check time messages"))
                    {
                        int receivedId = Convert.ToInt32(response.Split(" ")[4]);
                        stream.Write(Encoding.UTF8.GetBytes(CheckMessagesTime(receivedId).Result));
                    }
                    if (response.Contains("Check chat personal message"))
                    {
                        int receivedId = Convert.ToInt32(response.Split(" ")[5]);
                        int senderId = Convert.ToInt32(response.Split(" ")[7]);
                        stream.Write(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(CheckMessages(receivedId,senderId).Result)));
                    }
                    if (response.Contains("Check messages"))
                    {
                        int receivedId = Convert.ToInt32(response.Split(" ")[3]);
                        stream.Write(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(CheckMessages(receivedId).Result)));
                    }

                    if (response.Contains("Set avatar"))
                    {
                        MemoryStream ms = new MemoryStream();
                        do
                        {
                            stream.Read(buffer, 0, buffer.Length);
                            ms.Write(buffer);

                        } while (stream.DataAvailable);
                        string username = response.Split(" ")[3];
                        SetAvatar(username, ms.ToArray());
                    }
                    if (response.Contains("Change username"))
                    {
                        int id = Convert.ToInt32(response.Split(" ")[3]);
                        string username = response.Split(" ")[5];
                        string oldUsername = response.Split(" ")[7];
                        ChangeUsername(id, username, oldUsername);
                    }
                    if (response.Contains("Change password"))
                    {
                        int id = Convert.ToInt32(response.Split(" ")[3]);
                        string password = response.Split(" ")[5];
                        ChangePassword(id, password);
                    }
                    if (response.Contains("Get file"))
                    {
                        string path = response.Split(" ")[3];
                        byte[] bytes = File.ReadAllBytes(path);
                        stream.Write(bytes);
                    }
                    if (response.Contains("Delete friend"))
                    {
                        int userId = Convert.ToInt32(response.Split(" ")[3]);
                        int friendId = Convert.ToInt32(response.Split(" ")[5]);
                        DeleteFriend(userId, friendId);
                    }
                    if (response.Contains("Delete all messages"))
                    {
                        int userId = Convert.ToInt32(response.Split(" ")[4]);
                        int friendId = Convert.ToInt32(response.Split(" ")[6]);
                        DeleteAllMessages(userId, friendId);
                    }
                    if (response.Contains("Delete message"))
                    {
                        int userId = Convert.ToInt32(response.Split(" ")[3]);
                        int friendId = Convert.ToInt32(response.Split(" ")[5]);
                        int id = Convert.ToInt32(response.Split(" ")[7]);
                        string format = response.Split(" ")[9];
                        DeleteMessage(userId, friendId, id, format);
                    }


                    if (response.Contains("Get blocked users"))
                    {
                        int userId = Convert.ToInt32(response.Split(" ")[4]);
                        stream.Write(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(GetBlockedUsers(userId).Result)));
                    }
                    if (response.Contains("Check blocked"))
                    {
                        int userId = Convert.ToInt32(response.Split(" ")[3]);
                        int blockedId = Convert.ToInt32(response.Split(" ")[5]);
                        stream.Write(Encoding.UTF8.GetBytes(CheckBlockedUser(userId, blockedId).Result));
                    }
                    if (response.Contains("Block"))
                    {
                        int userId = Convert.ToInt32(response.Split(" ")[2]);
                        int blockedId = Convert.ToInt32(response.Split(" ")[4]);
                        BlockUser(userId, blockedId);
                    }
                    if (response.Contains("Unlocked"))
                    {
                        int userId = Convert.ToInt32(response.Split(" ")[2]);
                        int blockedId = Convert.ToInt32(response.Split(" ")[4]);
                        UnlockedUser(userId, blockedId);
                    }

                    if (response.Contains("Add to request friend table"))
                    {
                        int userId = Convert.ToInt32(response.Split(" ")[6]);
                        int friendId = Convert.ToInt32(response.Split(" ")[8]);
                        stream.Write(Encoding.UTF8.GetBytes(AddToRequestFriendTable(userId, friendId).Result));
                    }
                    if (response.Contains("Get requested users"))
                    {
                        int userId = Convert.ToInt32(response.Split(" ")[4]);
                        List<Friend> requestedFriend = GetRequestedFriendUsers(userId).Result;
                        stream.Write(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestedFriend)));
                    }
                    if (response.Contains("Delete from requested table"))
                    {
                        int userId = Convert.ToInt32(response.Split(" ")[5]);
                        int friendId = Convert.ToInt32(response.Split(" ")[7]);
                        DeleteFromRequestedTable(userId, friendId);
                    }
                    if (response.Contains("Disconnect"))
                    {
                        Invoke(new Action(() => { consoleListBox.Items.Add("Disconnected"); }));
                        client.Close();
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Invoke(new Action(() => { consoleListBox.Items.Add(ex.Message); }));
                    client.Close();
                    Invoke(new Action(() => { consoleListBox.Items.Add("Client disconnected"); }));
                    break;
                }
            }
        }

        #region Chat funcs
        public async Task<List<string>> GetMessages(int sentNameId, int nameId, int messageFetch)
        {
            List<string> messagesJson = new List<string>();
            await Task.Factory.StartNew(() =>
            {
                using (SqlConnection sql = new SqlConnection(sqlString))
                {
                    sql.Open();
                    SqlCommand command = new SqlCommand($"Select *  from MessageTable where SenderId = {sentNameId} and ReceivedId = {nameId} or SenderId = {nameId} and ReceivedId = {sentNameId} order by id desc offset {messageFetch} rows fetch next 15 rows only", sql);
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        Message message = null;
                        while (reader.Read())
                        {

                            if (reader.IsDBNull(5) && reader.IsDBNull(6) && reader.IsDBNull(7) && reader.IsDBNull(8))
                            {
                                message = new Message(reader.GetInt32(0), reader.GetDateTime(2), reader.GetInt32(3), reader.GetInt32(4)) { Text = reader.GetString(1) };
                            }
                            else if (!reader.IsDBNull(5))
                            {
                                message = new Message(reader.GetInt32(0), reader.GetDateTime(2), reader.GetInt32(3), reader.GetInt32(4)) { VoiceMessage = reader.GetString(5) };
                            }
                            else if (!reader.IsDBNull(6))
                            {
                                message = new Message(reader.GetInt32(0), reader.GetDateTime(2), reader.GetInt32(3), reader.GetInt32(4)) { ImageMessage = reader.GetString(6) };
                            }
                            else if (!reader.IsDBNull(7))
                            {
                                message = new Message(reader.GetInt32(0), reader.GetDateTime(2), reader.GetInt32(3), reader.GetInt32(4)) { VideoMessage = reader.GetString(7) };
                            }
                            else if (!reader.IsDBNull(8))
                            {
                                message = new Message(reader.GetInt32(0), reader.GetDateTime(2), reader.GetInt32(3), reader.GetInt32(4)) { ZipMessage = reader.GetString(8) };

                            }
                            if (nameId == message.RecipientId && sentNameId == message.SenderId || sentNameId == message.RecipientId && nameId == message.SenderId)
                            {
                                messagesJson.Add(JsonConvert.SerializeObject(message));
                            }
                        }
                    }
                }
            });
            return messagesJson;
        }
        public async Task<List<string>> GetMessages(int sentNameId, int nameId)
        {
            List<string> messagesJson = new List<string>();
            await Task.Factory.StartNew(() =>
            {
                using (SqlConnection sql = new SqlConnection(sqlString))
                {
                    sql.Open();
                    SqlCommand command = new SqlCommand($"Select top(15) *  from MessageTable where SenderId = {sentNameId} and ReceivedId = {nameId} or SenderId = {nameId} and ReceivedId = {sentNameId} order by id desc", sql);
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        Message message = null;
                        while (reader.Read())
                        {

                            if (reader.IsDBNull(5) && reader.IsDBNull(6) && reader.IsDBNull(7) && reader.IsDBNull(8))
                            {
                                message = new Message(reader.GetInt32(0), reader.GetDateTime(2), reader.GetInt32(3), reader.GetInt32(4)) { Text = reader.GetString(1) };
                            }
                            else if (!reader.IsDBNull(5))
                            {
                                message = new Message(reader.GetInt32(0), reader.GetDateTime(2), reader.GetInt32(3), reader.GetInt32(4)) { VoiceMessage = reader.GetString(5) };
                            }
                            else if (!reader.IsDBNull(6))
                            {
                                message = new Message(reader.GetInt32(0), reader.GetDateTime(2), reader.GetInt32(3), reader.GetInt32(4)) { ImageMessage = reader.GetString(6) };
                            }
                            else if (!reader.IsDBNull(7))
                            {
                                message = new Message(reader.GetInt32(0), reader.GetDateTime(2), reader.GetInt32(3), reader.GetInt32(4)) { VideoMessage = reader.GetString(7) };
                            }
                            else if (!reader.IsDBNull(8))
                            {
                                message = new Message(reader.GetInt32(0), reader.GetDateTime(2), reader.GetInt32(3), reader.GetInt32(4)) { ZipMessage = reader.GetString(8) };

                            }
                            if (nameId == message.RecipientId && sentNameId == message.SenderId || sentNameId == message.RecipientId && nameId == message.SenderId)
                            {
                                messagesJson.Add(JsonConvert.SerializeObject(message));
                            }
                        }
                    }
                }
            });
            return messagesJson;
        }
        public async Task<int> SendMessage(NetworkStream stream, string json, string format)
        {
            json = json.Replace("�", "");
            Message msg = null;
            try
            {
                await Task.Factory.StartNew(() =>
                {
   
                    MemoryStream ms = new MemoryStream();
                    byte[] buffer = new byte[1024];

                     msg = JsonConvert.DeserializeObject<Message>(json);
                    string path = string.Empty;

                    if (format != "Text")
                    {
                        do
                        {
                            stream.Read(buffer, 0, buffer.Length);
                            ms.Write(buffer);

                        } while (stream.DataAvailable);
                        ms.Position = 0;
                    }

                    using (SqlConnection sql = new SqlConnection(sqlString))
                    {
                        sql.Open();
                        SqlCommand command = new SqlCommand();
                        if (msg.Text != null)
                        {
                            command = new SqlCommand($"Insert into MessageTable (Text,TimeStamp,SenderId,ReceivedId) values ('{msg.Text.Replace("\'", "\"")}','{msg.TimeStamp}', {msg.SenderId},{msg.RecipientId}); SELECT SCOPE_IDENTITY();", sql);
                        }
                            if (msg.ImageMessage != null)
                        {
                            command = new SqlCommand($"Insert into MessageTable (ImageMessage,TimeStamp,SenderId,ReceivedId) values ('null','{msg.TimeStamp}', {msg.SenderId},{msg.RecipientId}); SELECT SCOPE_IDENTITY();", sql);
                        }
                        if (msg.VoiceMessage != null)
                        {
                            command = new SqlCommand($"Insert into MessageTable (VoiceMessage,TimeStamp,SenderId,ReceivedId) values ('null','{msg.TimeStamp}', {msg.SenderId},{msg.RecipientId}); SELECT SCOPE_IDENTITY();", sql);
                        }
                        if (msg.VideoMessage != null)
                        {
                            command = new SqlCommand($"Insert into MessageTable (VideoMessage,TimeStamp,SenderId,ReceivedId) values ('null','{msg.TimeStamp}', {msg.SenderId},{msg.RecipientId}); SELECT SCOPE_IDENTITY();", sql);
                        }
                        if (msg.ZipMessage != null)
                        {
                            command = new SqlCommand($"Insert into MessageTable (ZipMessage,TimeStamp,SenderId,ReceivedId) values ('null','{msg.TimeStamp}', {msg.SenderId},{msg.RecipientId}); SELECT SCOPE_IDENTITY();", sql);
                        }
                        msg.Id = Convert.ToInt32(command.ExecuteScalar());
                        if (format == "Video")
                            path = ServerDiskPath + "Messages/" + msg.Id + ".mp4";
                        if (format == "Image")
                            path = ServerDiskPath + "Messages/" + msg.Id + ".png";
                        if (format == "Voice")
                            path = ServerDiskPath + "Messages/" + msg.Id + ".wav";
                        if (format == "Zip")
                            path = ServerDiskPath + "Messages/" + msg.Id + ".zip";
                    }
                    if (msg.Text == null)
                    {
                        using (SqlConnection sql = new SqlConnection(sqlString))
                        {
                            SqlCommand command = null;
                            sql.Open();
                            if (msg.ImageMessage != null)
                            {
                                command = new SqlCommand($"update messagetable set ImageMessage = '{path}' where id = {msg.Id}", sql);
                            }
                            if (msg.VideoMessage != null)
                            {
                                command = new SqlCommand($"update messagetable set VideoMessage = '{path}' where id = {msg.Id}", sql);
                            }
                            if (msg.VoiceMessage != null)
                            {
                                command = new SqlCommand($"update messagetable set VoiceMessage = '{path}' where id = {msg.Id}", sql);
                            }
                            if (msg.ZipMessage != null)
                            {
                                command = new SqlCommand($"update messagetable set ZipMessage = '{path}' where id = {msg.Id}", sql);
                            }
                            command.ExecuteNonQuery();
                        }
                        File.WriteAllBytes(path, ms.ToArray());
                    }
                });
            }
            catch
            {

            }
            return msg.Id;

        }
        public async Task<string> CheckMessagesTime(int receivedId)
        {
            DateTime time = DateTime.Now;
            await Task.Factory.StartNew(() =>
            {
                using (SqlConnection sql = new SqlConnection(sqlString))
                {
                    sql.Open();
                    SqlCommand command = new SqlCommand($"select tOP(1)* from NotificationQueue  join MessageTable on NotificationQueue.MessageId = MessageTable.Id where MessageTable.ReceivedId = {receivedId} ORDER BY TimeStamp DESC", sql);
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            time = reader.GetDateTime(4);
                        }
                    }
                }
            });
            return time.ToString();
        }
        public async Task<string> CheckMessagesTime(int receivedId,int senderId)
        {
            DateTime time = DateTime.Now;
            await Task.Factory.StartNew(() =>
            {
                using (SqlConnection sql = new SqlConnection(sqlString))
                {
                    sql.Open();
                    SqlCommand command = new SqlCommand($"select tOP(1)* from NotificationQueue  join MessageTable on NotificationQueue.MessageId = MessageTable.Id where MessageTable.ReceivedId = {receivedId} and MessageTable.SenderId = {senderId} ORDER BY TimeStamp DESC", sql);
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            time = reader.GetDateTime(4);
                        }
                    }
                }
            });
            return time.ToString();
        }
        public async Task<Message> CheckMessages(int receivedId,int senderId)
        {
            Message message = null;
            await Task.Factory.StartNew(() =>
            {

                using (SqlConnection sql = new SqlConnection(sqlString))
                {
                    sql.Open();
                    SqlCommand command = new SqlCommand($"select tOP(1)* from NotificationQueue  join MessageTable on NotificationQueue.MessageId = MessageTable.Id where MessageTable.ReceivedId = {receivedId} and MessageTable.SenderId = {senderId} ORDER BY TimeStamp DESC", sql);
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {

                            if (reader.IsDBNull(7) && reader.IsDBNull(8) && reader.IsDBNull(9) && reader.IsDBNull(10))
                            {
                                message = new Message(reader.GetInt32(2), reader.GetDateTime(4), reader.GetInt32(5), reader.GetInt32(6)) { Text = reader.GetString(3) };

                            }
                            else if (!reader.IsDBNull(7))
                            {
                                message = new Message(reader.GetInt32(2), reader.GetDateTime(4), reader.GetInt32(5), reader.GetInt32(6)) { VoiceMessage = reader.GetString(7) };

                            }
                            else if (!reader.IsDBNull(8))
                            {
                                message = new Message(reader.GetInt32(2), reader.GetDateTime(4), reader.GetInt32(5), reader.GetInt32(6)) { ImageMessage = reader.GetString(8) };

                            }
                            else if (!reader.IsDBNull(9))
                            {
                                message = new Message(reader.GetInt32(2), reader.GetDateTime(4), reader.GetInt32(5), reader.GetInt32(6)) { VideoMessage = reader.GetString(9) };

                            }
                            else if (!reader.IsDBNull(10))
                            {
                                message = new Message(reader.GetInt32(2), reader.GetDateTime(4), reader.GetInt32(5), reader.GetInt32(6)) { ZipMessage = reader.GetString(10) };

                            }
                        }
                    }
                }
            });
            return message;
        }
        public async Task<Message> CheckMessages(int receivedId)
        {
            Message message = null;
            await Task.Factory.StartNew(() =>
            {

                using (SqlConnection sql = new SqlConnection(sqlString))
                {
                    sql.Open();
                    SqlCommand command = new SqlCommand($"select tOP(1)* from NotificationQueue  join MessageTable on NotificationQueue.MessageId = MessageTable.Id where MessageTable.ReceivedId = {receivedId} ORDER BY TimeStamp DESC", sql);
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {

                            if (reader.IsDBNull(7) && reader.IsDBNull(8) && reader.IsDBNull(9) && reader.IsDBNull(10))
                            {
                                message = new Message(reader.GetInt32(2), reader.GetDateTime(4), reader.GetInt32(5), reader.GetInt32(6)) { Text = reader.GetString(3) };

                            }
                            else if (!reader.IsDBNull(7))
                            {
                                message = new Message(reader.GetInt32(2), reader.GetDateTime(4), reader.GetInt32(5), reader.GetInt32(6)) { VoiceMessage = reader.GetString(7) };

                            }
                            else if (!reader.IsDBNull(8))
                            {
                                message = new Message(reader.GetInt32(2), reader.GetDateTime(4), reader.GetInt32(5), reader.GetInt32(6)) { ImageMessage = reader.GetString(8) };

                            }
                            else if (!reader.IsDBNull(9))
                            {
                                message = new Message(reader.GetInt32(2), reader.GetDateTime(4), reader.GetInt32(5), reader.GetInt32(6)) { VideoMessage = reader.GetString(9) };

                            }
                            else if (!reader.IsDBNull(10))
                            {
                                message = new Message(reader.GetInt32(2), reader.GetDateTime(4), reader.GetInt32(5), reader.GetInt32(6)) { ZipMessage = reader.GetString(10) };

                            }
                        }
                    }
                }
            });
            return message;
        }
        public async Task DeleteAllMessages(int userId, int friendId)
        {
            List<int> messageId = new List<int>();
            await Task.Factory.StartNew(() =>
            {
                using (SqlConnection sql = new SqlConnection(sqlString))
                {
                    sql.Open();
                    SqlCommand command = new SqlCommand($"DELETE notificationqueue FROM notificationqueue iNNER JOIN MessageTable ON MessageTable.Id = notificationqueue.messageid WHERE MessageTable.senderId = {userId} and MessageTable.receivedId = {friendId}", sql);
                    command.ExecuteNonQuery();

                    command = new SqlCommand($"delete from messageTable where SenderId = {userId} and ReceivedId = {friendId} ", sql);
                    command.ExecuteNonQuery();

                    command = new SqlCommand($"DELETE notificationqueue FROM notificationqueue iNNER JOIN MessageTable ON MessageTable.Id = notificationqueue.messageid WHERE MessageTable.senderId = {friendId} and MessageTable.receivedId = {userId}", sql);
                    command.ExecuteNonQuery();

                    command = new SqlCommand($"delete from messageTable where SenderId = {friendId} and ReceivedId = {userId} ", sql);
                    command.ExecuteNonQuery();
                }
            });
        }
        public async Task DeleteMessage(int userId, int friendId, int id, string format)
        {
            await Task.Factory.StartNew(() =>
            {
                using (SqlConnection sql = new SqlConnection(sqlString))
                {
                    sql.Open();
                    SqlCommand command = new SqlCommand($"DELETE notificationqueue FROM notificationqueue where messageid = {id}", sql);
                    command.ExecuteNonQuery();

                    command = new SqlCommand($"delete from messageTable where id={id} and SenderId = {userId} and ReceivedId = {friendId}", sql);
                    int a = command.ExecuteNonQuery();

                    File.Delete(ServerDiskPath + "Messages/" + id + format);
                }
            });
        }
        #endregion

        #region User funcs
        public async Task<User> LoginUser(string username, string password)
        {
            User user = null;
            await Task.Factory.StartNew(() =>
            {

                using (SqlConnection sql = new SqlConnection(sqlString))
                {
                    sql.Open();
                    SqlCommand command = new SqlCommand($"Select * from UserTable where Name = '{username}' and Password = '{password}'", sql);
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            int id = reader.GetInt32(0);
                            string nameTemp = reader.GetString(1);
                            string passwordTemp = reader.GetString(2);
                            bool isOnline = reader.GetBoolean(4);
                            string avatar = reader.GetString(3);
                            string ipAdress = reader.GetString(6);
                            int port = reader.GetInt32(7);
                            user = new User(id, nameTemp, passwordTemp) { IsOnline = isOnline, Avatar = avatar, IpAddress = ipAdress, Port = port };
                        }
                    }
                    sql.Close();
                    sql.Open();
                    command = new SqlCommand($"update usertable set IsOnline = 1 ,LastReceived = null where name = '{username}'", sql);
                    command.ExecuteNonQuery();
                    sql.Close();
                }
            });
            return user;
        }
        public async Task<string> RegisterUser(string username, string password)
        {
            string result = string.Empty;
            await Task.Factory.StartNew(() =>
            {
                using (SqlConnection sql = new SqlConnection(sqlString))
                {

                    byte[] avatarBytes = File.ReadAllBytes("C:\\MessengerServer\\userAcc.png");
                    sql.Open();
                    SqlCommand command = new SqlCommand($"IF NOT EXISTS (SELECT * FROM usertable WHERE Name = '{username}') insert into usertable (Name,Password,IsOnline) values ('{username}','{password}',0)", sql);
                    if (command.ExecuteNonQuery() > 0)
                    {
                        result = "Registration succesfull!";
                        Directory.CreateDirectory(ServerDiskPath + username);
                        SetAvatar(username, avatarBytes);

                        while (command.ExecuteNonQuery() < 0)
                        {
                            Random rd = new Random();
                            int generatedPort = rd.Next(81, 10000);
                            string ip = "127.0.0." + rd.Next(1, 255);
                            command = new SqlCommand($"IF NOT EXISTS (SELECT * FROM BusyIpAndPorts WHERE Port = {generatedPort} or IpAdress = '{ip}') insert into BusyIpAndPorts values ('{ip}',{generatedPort})", sql);
                            if (command.ExecuteNonQuery() > 0)
                            {
                                command = new SqlCommand($"update usertable set IpAddress = '{ip}', Port = {generatedPort} where name = '{username}'", sql);
                                command.ExecuteNonQuery();
                                break;
                            }
                        }

                    }
                    else
                        result = "User is Contains!";
                }
            });
            return result;
        }
        public async Task SetActive(int userId)
        {
            await Task.Factory.StartNew(() =>
            {

                using (SqlConnection sql = new SqlConnection(sqlString))
                {
                    SqlCommand command = new SqlCommand();
                    sql.Open();
                    command = new SqlCommand($"update usertable set IsOnline = 1 where id = '{userId}'", sql);
                    command.ExecuteNonQuery();
                    sql.Close();
                    sql.Open();
                    command = new SqlCommand($"update usertable set LastReceived = null where id = '{userId}'", sql);
                    command.ExecuteNonQuery();
                }
            });
        }
        public async Task<User> GetUserPerId(int userId)
        {
            User user = null;
            await Task.Factory.StartNew(() =>
            {
                using (SqlConnection sql = new SqlConnection(sqlString))
                {
                    sql.Open();
                    SqlCommand command = new SqlCommand($"Select * from UserTable where Id = {userId}", sql);
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            int id = reader.GetInt32(0);
                            string nameTemp = reader.GetString(1);
                            string avatar = reader.GetString(3);
                            bool isOnline = reader.GetBoolean(4);
                            DateTime lastReceived = DateTime.MinValue;
                            if (!reader.IsDBNull(5))
                                lastReceived = reader.GetDateTime(5); 
                            string ipAdress = reader.GetString(6);
                            int port = reader.GetInt32(7);
                            user = new User(id, nameTemp) { Avatar = avatar, IsOnline = isOnline, LastReceived = lastReceived, IpAddress = ipAdress, Port = port };
                        }
                    }

                }
            });
            return user;
        }
        public async Task<User> GetUserPerName(string username)
        {
            User user = null;
            await Task.Factory.StartNew(() =>
            {
                using (SqlConnection sql = new SqlConnection(sqlString))
                {
                    sql.Open();
                    SqlCommand command = new SqlCommand($"Select * from UserTable where Name = '{username}'", sql);
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            int id = reader.GetInt32(0);
                            string nameTemp = reader.GetString(1);
                            string avatar = reader.GetString(3);
                            bool isOnline = reader.GetBoolean(4);
                            DateTime lastReceived = DateTime.MinValue;
                            if (!reader.IsDBNull(5))
                                lastReceived = reader.GetDateTime(5);

                            string ipAdress = reader.GetString(6);
                            int port = reader.GetInt32(7);
                            user = new User(id, nameTemp) { Avatar = avatar, IsOnline = isOnline, LastReceived = lastReceived, IpAddress = ipAdress, Port = port };
                        }
                    }
                }
            });
            return user;
        }
        public async Task SetAvatar(string username, byte[] image_bytes)
        {
            await Task.Run(() =>
            {
                using (SqlConnection connection = new SqlConnection(sqlString))
                {
                    connection.Open();
                    string pathAvatar = ServerDiskPath + username + "/avatar.png";
                    SqlCommand command = new SqlCommand($"update userTable set Avatar = '{pathAvatar}' where Name = '{username}'", connection);
                    command.ExecuteNonQuery();
                    File.WriteAllBytes(pathAvatar, image_bytes);


                }
            });
        }
        public async Task ChangeUsername(int id, string username, string oldUsername)
        {
            {
                await Task.Run(() =>
                {
                    using (SqlConnection connection = new SqlConnection(sqlString))
                    {
                        connection.Open();

                        SqlCommand command = new SqlCommand($"update userTable set name = '{username}' where Id = {id}", connection);
                        command.ExecuteNonQuery();
                        command = new SqlCommand($"update userTable set avatar = 'C://MessengerServer/{username}/avatar.png' where Id = {id}", connection);
                        command.ExecuteNonQuery();

                        Directory.Move(ServerDiskPath + oldUsername, ServerDiskPath + username);

                    }
                });
            }
        }
        public async Task ChangePassword(int id, string password)
        {
            await Task.Run(() =>
            {
                using (SqlConnection connection = new SqlConnection(sqlString))
                {
                    connection.Open();

                    SqlCommand command = new SqlCommand($"update userTable set password = '{password}' where Id = {id}", connection);
                    command.ExecuteNonQuery();
                }
            });
        }
        public async Task<string> AddFriend(int userId, int friendId)
        {
            string result = string.Empty;
            await Task.Factory.StartNew(() =>
            {
                using (SqlConnection sql = new SqlConnection(sqlString))
                {
                    sql.Open();
                    SqlCommand command = new SqlCommand($"delete from RequestedUsersTable where userId = {friendId} and ToRequestsId={userId}", sql);
                    if (command.ExecuteNonQuery() > 0)
                        result = "True";
                    else
                        result = "False";
                    command = new SqlCommand($"delete from RequestedUsersTable where userId = {userId} and ToRequestsId={friendId}", sql);
                    if (command.ExecuteNonQuery() > 0)
                        result = "True";
                    else
                        result = "False";

                    command = new SqlCommand($"IF NOT EXISTS (SELECT * FROM FriendTable WHERE Id = {friendId} and UserId ={userId}) insert into FriendTable values ({friendId},{userId})", sql);
                    command.ExecuteNonQuery();

                    command = new SqlCommand($"IF NOT EXISTS (SELECT * FROM FriendTable WHERE Id = {userId} and UserId ={friendId}) insert into FriendTable values ({userId},{friendId})", sql);
                    command.ExecuteNonQuery();
                }
            });
            return result;

        }
        public async Task DeleteFriend(int userId, int friendId)
        {
            await Task.Factory.StartNew(() =>
            {
                using (SqlConnection sql = new SqlConnection(sqlString))
                {
                    sql.Open();
                    SqlCommand command = new SqlCommand($"delete from friendtable where id={friendId} and UserId = {userId}", sql);
                    command.ExecuteNonQuery();

                    command = new SqlCommand($"delete from friendtable where id={userId} and UserId = {friendId}", sql);
                    command.ExecuteNonQuery();
                }
            });
        }
        public async Task<List<Friend>> GetFriends(int userId)
        {
            List<Friend> friendList = new List<Friend>();
            await Task.Factory.StartNew(() =>
            {
                using (SqlConnection sql = new SqlConnection(sqlString))
                {
                    sql.Open();
                    SqlCommand sqlCommand = new SqlCommand($"select UT.Name, F.UserId, UT.Id from UserTable as UT Inner Join FriendTable f on UT.Id = f.Id where f.UserId = {userId}", sql);
                    SqlDataReader reader = sqlCommand.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            string name = reader.GetString(0);
                            int userIdTemp = reader.GetInt32(1);
                            int id = reader.GetInt32(2);

                            friendList.Add(new Friend(name, userIdTemp, id));
                        }
                    }
                }
            });
            return friendList;
        }
        public async Task SetNotActive(int userId)
        {
            await Task.Factory.StartNew(() =>
            {
                using (SqlConnection sql = new SqlConnection(sqlString))
                {
                    sql.Open();
                    SqlCommand command = new SqlCommand($"update usertable set IsOnline = 0 where id = {userId}", sql);
                    command.ExecuteNonQuery();
                    sql.Close();
                    sql.Open();
                    command = new SqlCommand($"update usertable set LastReceived = '{DateTime.Now.ToString()}' where id = {userId}", sql);
                    command.ExecuteNonQuery();

                }
            });
        }
        public async Task BlockUser(int userId, int blockedId)
        {
            await Task.Factory.StartNew(() =>
            {
                using (SqlConnection sql = new SqlConnection(sqlString))
                {
                    sql.Open();
                    SqlCommand command = new SqlCommand($"insert into BlockedUsersTable values({userId},{blockedId})", sql);
                    command.ExecuteNonQuery();

                    command = new SqlCommand($"delete from friendtable where id = {userId} and useriD = {blockedId}", sql);
                    command.ExecuteNonQuery();

                    command = new SqlCommand($"delete from friendtable where id = {blockedId} and useriD = {userId}", sql);
                    command.ExecuteNonQuery();

                }
            });
        }
        public async Task UnlockedUser(int userId, int blockedId)
        {
            await Task.Factory.StartNew(() =>
            {
                using (SqlConnection sql = new SqlConnection(sqlString))
                {
                    sql.Open();
                    SqlCommand command = new SqlCommand($"delete from BlockedUsersTable where UserId={userId} and BlockedId={blockedId}", sql);
                    command.ExecuteNonQuery();

                }
            });
        }
        public async Task<List<Friend>> GetBlockedUsers(int userId)
        {
            List<Friend> friends = new List<Friend>();
            await Task.Factory.StartNew(() =>
            {
                using (SqlConnection sql = new SqlConnection(sqlString))
                {
                    sql.Open();
                    SqlCommand command = new SqlCommand($"select UT.Name, F.UserId, F.BlockedId from UserTable as UT Inner Join BlockedUsersTable f on UT.Id = f.BlockedId where f.UserId ={userId}",sql);
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            string name = reader.GetString(0);
                            int userIdTemp = reader.GetInt32(1);
                            int id = reader.GetInt32(2);

                            friends.Add(new Friend(name, userIdTemp, id));
                        }
                    }
                }
            });
            return friends;
        }
        public async Task<string> CheckBlockedUser(int userId, int blockedId)
        {
            string result = "False";
            await Task.Factory.StartNew(() =>
            {
                using (SqlConnection sql = new SqlConnection(sqlString))
                {
                    sql.Open();
                    SqlCommand command = new SqlCommand($"select * from blockeduserstable where userId = {userId} and blockedId={blockedId}", sql);
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        result = "True";
                    }
                }
            });
            return result;
        }
        public async Task<bool> CheckYouHaveThisFriend(int userId,int friendId)
        {
            bool result = false;
            await Task.Factory.StartNew(() =>
            {
                using (SqlConnection sql = new SqlConnection(sqlString))
                {
                    sql.Open();
                    SqlCommand command = new SqlCommand($"select * from friendtable where Id = {userId} and userId ={friendId}", sql);
                    SqlDataReader reader = command.ExecuteReader();
                    if (!reader.HasRows)
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                }
            });
            return result;
        }
        public async Task<string> AddToRequestFriendTable(int userId, int friendId)
        {
            string result = "False";
            await Task.Factory.StartNew(() =>
            {
                using (SqlConnection sql = new SqlConnection(sqlString))
                {
                    sql.Open();
                    if (CheckYouHaveThisFriend(userId, friendId).Result) 
                    {
                        SqlCommand command = new SqlCommand($"IF NOT EXISTS (SELECT * FROM RequestedUsersTable WHERE UserId = {userId} and ToRequestsId ={friendId}) IF NOT EXISTS (SELECT * FROM RequestedUsersTable WHERE UserId = {friendId} and ToRequestsId ={userId}) insert into RequestedUsersTable values ({userId},{friendId})", sql);
                        if (command.ExecuteNonQuery() > 0)
                            result = "True";
                        else
                            result = "You have already sent a request to the user";
                    }
                    else
                    {
                        result = "You have this friend";
                    }
                }
            });
            return result;
        }
        public async Task<List<Friend>> GetRequestedFriendUsers(int userId)
        {
            List<Friend> friends = new List<Friend>();
            await Task.Factory.StartNew(() =>
            {
                using (SqlConnection sql = new SqlConnection(sqlString))
                {
                    sql.Open();
                    SqlCommand sqlCommand = new SqlCommand($"select UT.Name, F.ToRequestsId, UT.Id from UserTable as UT Inner Join RequestedUsersTable f on UT.Id = f.UserId where f.ToRequestsId ={userId}", sql);
                    SqlDataReader reader = sqlCommand.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            string name = reader.GetString(0);
                            int userIdTemp = reader.GetInt32(1);
                            int id = reader.GetInt32(2);

                            friends.Add(new Friend(name, userIdTemp, id));
                        }
                    }
                }
            });
            return friends;
        }
        public async Task DeleteFromRequestedTable(int userId, int friendId)
        {
            await Task.Factory.StartNew(() =>
            {
                using (SqlConnection sql = new SqlConnection(sqlString))
                {
                    sql.Open();
                    SqlCommand command = new SqlCommand($"delete from RequestedUsersTable where userId = {friendId} and ToRequestsId={userId}", sql);
                    command.ExecuteNonQuery();
       
                }
            });
        
        }
        #endregion
    }
}