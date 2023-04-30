using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessengerLiblary
{
    [Serializable]
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string Avatar { get; set; }
        public bool IsOnline { get; set; }
        public DateTime LastReceived { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public User(int id, string name, string Password)
        {
            this.Id = id;
            this.Name = name;
            this.Password = Password;
        }

        public User(int id, string name)
        {
            this.Id = id;
            this.Name = name;
        }
        public User()
        {
        }
    }
}
