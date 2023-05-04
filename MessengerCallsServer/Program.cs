using NAudio.Wave;
using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace MessengerCallsServer
{
    public class User
    {
        public int id;
        public string typeCall;
        public TcpClient client;

        public User(int id, string typeCall,TcpClient client)
        {
            this.id = id;
            this.typeCall = typeCall;
            this.client = client;
        }
        public User(int id, TcpClient client)
        {
            this.id = id;
            this.client = client;
        }
    }
    internal class Program
    {
        private static TcpListener listener = new TcpListener(System.Net.IPAddress.Parse("127.0.0.1"), 8001);
        private static List<User> allUsers = new List<User>();
        private static Dictionary<int, int> callsDictionary = new Dictionary<int, int>();
        private static Dictionary<int, int> realCallsDictionary = new Dictionary<int, int>();
        static async Task Main(string[] args)
        {
            listener.Start();
            Console.WriteLine("Server started!");
            await Task.Run(() =>
            {
                while (true)
                {
                    byte[] buffer = new byte[1024];
                    TcpClient client = listener.AcceptTcpClient();
                    Console.WriteLine("Client connected");
                    NetworkStream stream = client.GetStream();
                    int len = stream.Read(buffer, 0, buffer.Length);
                    string response = Encoding.UTF8.GetString(buffer, 0, len);
                    if (response.Contains("Connect"))
                    {
                        User user = new User(Convert.ToInt32(response.Split(" ")[2]), client);
                        allUsers.Add(user);

                        Thread thread = new Thread(() => HandleClient(user));
                        thread.Start();
                    }
  

                }
            });
        }
        private static void HandleClient(User user)
        {
            bool hungup = false;
            NetworkStream stream = user.client.GetStream();
            while (true)
            {
                try
                {
                    byte[] buffer = new byte[1024];
                    int len = stream.Read(buffer, 0, buffer.Length);
                    string response = Encoding.UTF8.GetString(buffer, 0, len);

                    if (response.Contains("Check status audio call"))
                    {
                        if (hungup == true)
                        {
                            int userId = Convert.ToInt32(response.Split(" ")[5]);
                            int friendId = Convert.ToInt32(response.Split(" ")[7]);
                            User userOne = allUsers.Where(x => x.id == userId).FirstOrDefault();
                            User userTwo = allUsers.Where(x => x.id == friendId).FirstOrDefault();
                            if (userOne != null && userTwo != null)
                            {
                                NetworkStream streamOne = userOne.client.GetStream();
                                NetworkStream streamTwo = userTwo.client.GetStream();
                                if (realCallsDictionary.ContainsKey(userOne.id) && realCallsDictionary.ContainsKey(userTwo.id))
                                {
                                    streamOne.Write(Encoding.UTF8.GetBytes("NICE"));
                                    streamTwo.Write(Encoding.UTF8.GetBytes("NICE"));
                                    Console.WriteLine(":ice");
                                }
                                Console.Write("omg");
                                if (!realCallsDictionary.ContainsKey(userOne.id))
                                {
                                    User userTemp = allUsers.Where(x => x.id == userOne.id).FirstOrDefault();
                                    NetworkStream streamTemp = userTemp.client.GetStream();
                                    streamTemp.Write(Encoding.UTF8.GetBytes("User is cancelled call"));
                                    Console.WriteLine("userOne");
                                }
                                if (!realCallsDictionary.ContainsKey(userTwo.id))
                                {
                                    User userTemp = allUsers.Where(x => x.id == userTwo.id).FirstOrDefault();
                                    NetworkStream streamTemp = userTemp.client.GetStream();
                                    streamTemp.Write(Encoding.UTF8.GetBytes("User is cancelled call"));
                                    Console.WriteLine("userTwo");
                                }
                            }
                        }
                       

                    }
                    if (response.Contains("Check calls"))
                    {
                        if (callsDictionary.ContainsKey(user.id))
                        {
                            int callerUserId = callsDictionary[user.id];
                            Console.WriteLine("Call on " + user.id + " on " + callerUserId);
                            stream.Write(Encoding.UTF8.GetBytes(Convert.ToString(callerUserId)));
                            callsDictionary.Remove(user.id);
                        }
                    }
                    if (response.Contains("Audio call"))
                    {
                        int userId = Convert.ToInt32(response.Split(" ")[3]);
                        int friendId = Convert.ToInt32(response.Split(" ")[5]);
                        if (!realCallsDictionary.ContainsKey(userId))
                        {
                            realCallsDictionary.Add(userId, friendId);
                        }
 
                        if (!callsDictionary.ContainsKey(userId))
                            callsDictionary.Add(userId, friendId);

                        Console.WriteLine("audio call");
                    }
                    if (response.Contains("Up audio call"))
                    {
                        int userId = Convert.ToInt32(response.Split(" ")[6]);
                        int friendId = Convert.ToInt32(response.Split(" ")[4]);
                        realCallsDictionary.Add(userId, friendId);
                        hungup = true;

                    }
                    if (response.Contains("Close audio call"))
                    {
                        int userId = Convert.ToInt32(response.Split(" ")[6]);
                        int friendId = Convert.ToInt32(response.Split(" ")[4]);

                        User userTemp = allUsers.Where(x => x.id == friendId).FirstOrDefault();
                        if (userTemp != null)
                        {
                            NetworkStream streamTemp = user.client.GetStream();
                            streamTemp.Write(Encoding.UTF8.GetBytes("User is cancelled call"));
                        }
                        User userTempTwo = allUsers.Where(x => x.id == userId).FirstOrDefault();
                        if (userTempTwo != null)
                        {
                            NetworkStream streamTempTwo = userTempTwo.client.GetStream();
                            streamTempTwo.Write(Encoding.UTF8.GetBytes("User is cancelled call"));
                        }

                        realCallsDictionary.Remove(userId);
                        //realCallsDictionary.Remove(friendId);
                        Console.WriteLine(userId + " " + friendId + " disconnect");
                    }
                    if (response.Contains("Close only user"))
                    {
                        int userId = Convert.ToInt32(response.Split(" ")[4]);
                        User temp = allUsers.Where(x => x.id == user.id).FirstOrDefault();
                        allUsers.Remove(temp);
                        realCallsDictionary.Remove(userId);
                    }
                    if (response.Contains("Disconnect"))
                    {
                        realCallsDictionary.Remove(user.id);
                        allUsers.Remove(user);
                        Console.WriteLine("discoonectttt");
                        user.client.Close();
                        break;
                    }
                }
                catch (Exception ex)
                {
                    realCallsDictionary.Remove(user.id);
                    allUsers.Remove(user);
                    user.client.Close();
                    Console.WriteLine(ex.Message);
                    break;
                }
            }
        }
    }
}