using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace MessengerLiblary
{
    public class MessengerLiblary
    {
        public TcpClient client = new TcpClient();
        public NetworkStream stream = null;
        public Mutex mutex = new Mutex();

        public void Connect(string ip,int port)
        {
            client = new TcpClient();
            client.Connect(IPAddress.Parse(ip), port);
            stream = client.GetStream();
        }
        public void Disconnect()
        {
            stream.Write(Encoding.UTF8.GetBytes($"Disconnect"));
        }

        public MessengerLiblary() {
           
           
        }

        #region Chat funcs
        public List<Message> GetMessages(int userId, int receivedId)
        {
            List<Message> deserializedMsg = new List<Message>();
            stream.Write(Encoding.UTF8.GetBytes($"Get messages - {userId} - {receivedId}"));

            string json;

            int bufferLength = 0;
            StringBuilder builder = new StringBuilder();
            byte[] buffer = new byte[1024];
            do
            {
                bufferLength = stream.Read(buffer, 0, buffer.Length);
                builder.Append(Encoding.UTF8.GetString(buffer, 0, bufferLength));
            } while (stream.DataAvailable);

            List<string> deserializedString = JsonConvert.DeserializeObject<List<string>>(builder.ToString());
            foreach (var item in deserializedString)
                deserializedMsg.Add(JsonConvert.DeserializeObject<Message>(item));

            return deserializedMsg;
        }
        public List<Message> GetNextMessages(int userId, int receivedId,int messageFetch)
        {
            List<Message> deserializedMsg = new List<Message>();
            stream.Write(Encoding.UTF8.GetBytes($"Get next messages - {userId} - {receivedId} - {messageFetch}"));

            string json;

            int bufferLength = 0;
            StringBuilder builder = new StringBuilder();
            byte[] buffer = new byte[1024];
            do
            {
                bufferLength = stream.Read(buffer, 0, buffer.Length);
                builder.Append(Encoding.UTF8.GetString(buffer, 0, bufferLength));
            } while (stream.DataAvailable);

            List<string> deserializedString = JsonConvert.DeserializeObject<List<string>>(builder.ToString());
            foreach (var item in deserializedString)
                deserializedMsg.Add(JsonConvert.DeserializeObject<Message>(item));

            return deserializedMsg;
        }
        public async Task<int> SendMessage(Message msg, byte[] bytes, string format)
        {
            int msgId = 0;
            byte[] buffer = new byte[1024];
            await Task.Run(() => { 
            stream.Write(Encoding.UTF8.GetBytes($"Send message - " + format));
                if (format == "Text")
                {
                    stream.Write(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(msg)));
                }
                if (format == "Image")
                {
                    stream.Write(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(msg)));
                    Thread.Sleep(150);
                    stream.Write(bytes);
                }
                if (format == "Video")
                {
                    stream.Write(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(msg)));
                    Thread.Sleep(150);
                    stream.Write(bytes);
                }
                if (format == "Voice")
                {
                    stream.Write(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(msg)));
                    Thread.Sleep(150);
                    stream.Write(bytes);
                }
                if (format == "Zip")
                {
                    stream.Write(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(msg)));
                    Thread.Sleep(150);
                    stream.Write(bytes);
                }
            });
            int len = stream.Read(buffer, 0, buffer.Length);
                msgId = Convert.ToInt32(Encoding.UTF8.GetString(buffer, 0, len));
            return msgId;
        }
        public string CheckMessagesTime(int receivedId)
        {
            byte[] buffer = new byte[1024];
                StringBuilder builder = new StringBuilder();
                stream.Write(Encoding.UTF8.GetBytes($"Check time messages - {receivedId}"));
                int bufferLength = 0;
                do
                {
                    bufferLength = stream.Read(buffer, 0, buffer.Length);
                    builder.Append(Encoding.UTF8.GetString(buffer, 0, bufferLength));
                } while (stream.DataAvailable);
            return builder.ToString();
        }
        public string CheckMessagesTime(int receivedId,int senderId)
        {
            mutex.WaitOne();
            byte[] buffer = new byte[1024];
            StringBuilder builder = new StringBuilder();
                stream.Write(Encoding.UTF8.GetBytes($"Check chat message time - {receivedId} - {senderId}"));
                Thread.Sleep(150);
                int bufferLength = 0;
                do
                {
                    bufferLength = stream.Read(buffer, 0, buffer.Length);
                    builder.Append(Encoding.UTF8.GetString(buffer, 0, bufferLength));
                } while (stream.DataAvailable);
            mutex.ReleaseMutex();
            return builder.ToString();
        }
        public Message CheckMessagesChat(int receivedId, int senderId)
        {
            mutex.WaitOne();
            byte[] buffer = new byte[1024];
            Message message = null;
                StringBuilder builder = new StringBuilder();
                stream.Write(Encoding.UTF8.GetBytes($"Check chat personal message - {receivedId} - {senderId}"));
                Thread.Sleep(150);
                int bufferLength = 0;
                do
                {
                    bufferLength = stream.Read(buffer, 0, buffer.Length);
                    builder.Append(Encoding.UTF8.GetString(buffer, 0, bufferLength));
                } while (stream.DataAvailable);
               message = JsonConvert.DeserializeObject<Message>(builder.ToString());
            mutex.ReleaseMutex();
            return message;
        }
        public Message CheckMessages(int receivedId)
        {
            byte[] buffer = new byte[1024];
            Message message = null;
                StringBuilder builder = new StringBuilder();
                stream.Write(Encoding.UTF8.GetBytes($"Check messages - {receivedId}"));
                int bufferLength = 0;
                do
                {
                    bufferLength = stream.Read(buffer, 0, buffer.Length);
                    builder.Append(Encoding.UTF8.GetString(buffer, 0, bufferLength));
                } while (stream.DataAvailable);
                message = JsonConvert.DeserializeObject<Message>(builder.ToString());
            return message;
        }
        public byte[] GetFile(string path)
        {
            byte[] buffer = new byte[1024];
            MemoryStream ms = new MemoryStream();

                stream.Write(Encoding.UTF8.GetBytes($"Get file - {path}"));
                int bufferLength = 0;
                do
                {
                    bufferLength = stream.Read(buffer, 0, buffer.Length);
                    ms.Write(buffer, 0, bufferLength);
                } while (stream.DataAvailable);
                ms.Position = 0;
            return ms.ToArray();
        }
        public void DeleteAllMessages(int userId, int friendId)
        {
            stream.Write(Encoding.UTF8.GetBytes($"Delete all messages - {userId} - {friendId}"));
        }
        public void DeleteMessage(int userId, int friendId, int id,string format)
        {
            stream.Write(Encoding.UTF8.GetBytes($"Delete message - {userId} - {friendId} - {id} - {format}"));
        }
        #endregion

        #region User funcs
        public bool Register(string username, string password) {
            bool result = false;
            StringBuilder builder = new StringBuilder();
            stream.Write(Encoding.UTF8.GetBytes("Register - " + username + " - " + password));

            int lengthBuffer = 0;
            byte[] buffer = new byte[1024];
            do
            {
                lengthBuffer = stream.Read(buffer, 0, buffer.Length);
                builder.Append(Encoding.UTF8.GetString(buffer, 0, lengthBuffer));
            } while (stream.DataAvailable);
            if (builder.ToString() == "Registration succesfull!")
                result = true;
            return result;
        
        }
        public User Login(string username, string password) {
            User user = null;
                StringBuilder builder = new StringBuilder();
                stream.Write(Encoding.UTF8.GetBytes("Login - " + username + " - " + password));
                int lengthBuffer = 0;
                byte[] buffer = new byte[1024];
                do
                {
                    lengthBuffer = stream.Read(buffer, 0, buffer.Length);
                    builder.Append(Encoding.UTF8.GetString(buffer, 0, lengthBuffer));
                } while (stream.DataAvailable);
                user = JsonConvert.DeserializeObject<User>(builder.ToString());
            return user;
        }
        public void SetAvatar(string username, byte[] bytes)
        {
            stream.Write(Encoding.UTF8.GetBytes($"Set avatar - {username}"));
            Thread.Sleep(100);
            stream.Write(bytes, 0, bytes.Length);
        }
        public void ChangeUsername(int id, string username,string oldUsername)
        {
            stream.Write(Encoding.UTF8.GetBytes($"Change username - {id} - {username} - {oldUsername}"));
        }
        public void ChangePassword(int id, string password)
        {
            stream.Write(Encoding.UTF8.GetBytes($"Change password - {id} - {password}"));
        }
        public void DeleteFriend(int userId, int friendId)
        {
            stream.Write(Encoding.UTF8.GetBytes($"Delete friend - {userId} - {friendId}"));
        }
        public List<Friend> GetFriends(int userId)
        {
            byte[] buffer = new byte[1024];
            List<Friend> friends = null;
            try
            {
                stream.Write(Encoding.UTF8.GetBytes($"GetFriends - {userId}"));

                int bufferLength = stream.Read(buffer, 0, buffer.Length);
                string json = Encoding.UTF8.GetString(buffer, 0, bufferLength);
                friends = JsonConvert.DeserializeObject<List<Friend>>(json);
            }
            catch
            {

            }

            return friends;
        }
        public bool AddFriend(int userId, int friendId)
        {
            byte[] buffer = new byte[1024];
            bool result = false;

                stream.Write(Encoding.UTF8.GetBytes($"Add Friend - {userId} - {friendId}"));

                int bufferLength = stream.Read(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, bufferLength);
                if (response == "True")
                    result = true;
            return result;
        }
        public User GetUserPerName(string username)
        {
            byte[] buffer = new byte[1024];
            User user = null;
            try
            {
                stream.Write(Encoding.UTF8.GetBytes($"Get user per name - {username}"));

                StringBuilder builder = new StringBuilder();
                int bufferLength = 0;
                do
                {
                    bufferLength = stream.Read(buffer, 0, buffer.Length);
                    builder.Append(Encoding.UTF8.GetString(buffer, 0, bufferLength));
                } while (stream.DataAvailable);
                user = JsonConvert.DeserializeObject<User>(builder.ToString());
            }
            catch
            {

            }
            return user;
        }
        public User GetUserPerId(int userId)
        {
            byte[] buffer = new byte[1024];
            User? user = null;
                stream.Write(Encoding.UTF8.GetBytes($"Get user per id - {userId}"));

                StringBuilder builder = new StringBuilder();
                int bufferLength = 0;
                do
                {
                    bufferLength = stream.Read(buffer, 0, buffer.Length);
                    builder.Append(Encoding.UTF8.GetString(buffer, 0, bufferLength));
                } while (stream.DataAvailable);
                user = JsonConvert.DeserializeObject<User>(builder.ToString());
            return user;
        }
        public void SetNotActive(int userId)
        {
            stream.Write(Encoding.UTF8.GetBytes($"Set not active - {userId}"));
        }
        public void BlockUser(int userId,int blockedId)
        {
            stream.Write(Encoding.UTF8.GetBytes($"Block - {userId} - {blockedId}"));
        }
        public void UnlockUser(int userId, int blockedId)
        {
            stream.Write(Encoding.UTF8.GetBytes($"Unlocked - {userId} - {blockedId}"));
        }
        public List<Friend> GetBlockedUsers(int userId)
        {
            List<Friend> blocked = new List<Friend>();
                stream.Write(Encoding.UTF8.GetBytes($"Get blocked users - {userId}"));

                StringBuilder builder = new StringBuilder();
                byte[] buffer = new byte[1024];
                int bufferLength = 0;
                do
                {
                    bufferLength = stream.Read(buffer, 0, buffer.Length);
                    builder.Append(Encoding.UTF8.GetString(buffer, 0, bufferLength));
                } while (stream.DataAvailable);
                blocked = JsonConvert.DeserializeObject<List<Friend>>(builder.ToString());
            return blocked;
        }
        public bool CheckBlockedUser(int userId, int blockedId)
        {
            bool result = false;
                stream.Write(Encoding.UTF8.GetBytes($"Check blocked - {userId} - {blockedId}"));
                byte[] buffer = new byte[1024];
                int len = stream.Read(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, len);
                if (response == "True")
                    result = true;
            return result;
        }
        public string AddToRequestFriendsTable(int userId, int friendId)
        {
            string result = "";
                stream.Write(Encoding.UTF8.GetBytes($"Add to request friend table - {userId} - {friendId}"));
                byte[] buffer = new byte[1024];
                int len = stream.Read(buffer, 0, buffer.Length);

               result= Encoding.UTF8.GetString(buffer, 0, len);
            return result;
        }
        public List<Friend> GetRequestedFriend(int userId)
        {
            List<Friend> requestedFriend = new List<Friend>();
                stream.Write(Encoding.UTF8.GetBytes($"Get requested users - {userId}"));
                    MemoryStream ms = new MemoryStream();
                    byte[] buffer = new byte[1024];
                    int len = stream.Read(buffer, 0, buffer.Length);
                    string json = string.Empty;
                    json = Encoding.UTF8.GetString(buffer, 0, len);
                    if (json != null)
                        requestedFriend = JsonConvert.DeserializeObject<List<Friend>>(json);
            return requestedFriend;
        }
        public void RemoveFromRequestedFriendTabe(int userId,int friendId)
        {
            stream.Write(Encoding.UTF8.GetBytes($"Delete from requested table - {userId} - {friendId}"));
        }
        #endregion
    }
}
