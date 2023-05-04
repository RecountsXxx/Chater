using System.Net.Sockets;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;

namespace MessengerLiblary
{
    public class MessengerCallsLiblary
    {
        public TcpClient client = new TcpClient();
        public NetworkStream stream = null;
        public static Mutex mutex = new Mutex();
        public MessengerCallsLiblary()
        {


        }

        #region Calls
        public void ConnectCallsServer(string ip, int port, int id)
        {
            client = new TcpClient();
            client.Connect(IPAddress.Parse(ip), port);
            stream = client.GetStream();
            stream.Write(Encoding.UTF8.GetBytes($"Connect - {id}"));
        }
        public void DisconnectCallsServer()
        {
            stream.Write(Encoding.UTF8.GetBytes($"Disconnect"));
        }
        public int CheckOutCalls(int id)
        {
            mutex.WaitOne();
            int result = 0;
            byte[] buffer = new byte[1024];
            stream.Write(Encoding.UTF8.GetBytes($"Check calls - {id}"));
            if (stream.DataAvailable)
            {
                int len = stream.Read(buffer, 0, buffer.Length);
                if (!Encoding.UTF8.GetString(buffer, 0, len).Contains("User is cancelled call"))
                {
                    result = Convert.ToInt32(Encoding.UTF8.GetString(buffer, 0, len));
                }
            }
            mutex.ReleaseMutex();
            return result;
        }
        public void AudioCall(int userId,int friendId)
        {
            mutex.WaitOne();
            stream.Write(Encoding.UTF8.GetBytes($"Audio call - {userId} - {friendId}"));
            mutex.ReleaseMutex();
        }
        public void CloseAudioCall(int userId, int friendId)
        {
            mutex.WaitOne();
            stream.Write(Encoding.UTF8.GetBytes($"Close audio call - {userId} - {friendId}"));
            mutex.ReleaseMutex();
        }
        public void UpAudioCall(int userId, int friendId)
        {
            mutex.WaitOne();
            stream.Write(Encoding.UTF8.GetBytes($"Up audio call - {userId} - {friendId}"));
            mutex.ReleaseMutex();
        }
        public void CloseOnlyOneUserAudioCall(int userId)
        {
            mutex.WaitOne();
            stream.Write(Encoding.UTF8.GetBytes($"Close only user - {userId}"));
            mutex.ReleaseMutex();
        }
        public string CheckStatusAudioCall(int userId,int friendId)
        {
            mutex.WaitOne();
            string result = "";
            byte[] buffer = new byte[1024];
            stream.Write(Encoding.UTF8.GetBytes($"Check status audio call - {userId} - {friendId}"));
            if (stream.DataAvailable)
            {
                int len = stream.Read(buffer, 0, buffer.Length);
                result = Encoding.UTF8.GetString(buffer, 0, len);
            }
            mutex.ReleaseMutex();
            return result;
        }
        public async Task<byte[]> GetVoice(int userId,int friendId) {
            byte[] buffer= new byte[10240000];
            MemoryStream ms = new MemoryStream();
            int len = 0;
            do
            {
                len = await stream.ReadAsync(buffer,0,buffer.Length);
                ms.Write(buffer,0,len); 
            } while (stream.DataAvailable);
            return ms.ToArray();
        }
        public async void SendVoice(int userId,int friendId,byte[] bytes) {
            await stream.WriteAsync(bytes);
        }
        #endregion
    }
}
