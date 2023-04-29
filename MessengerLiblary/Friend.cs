using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessengerLiblary
{
    public class Friend
    {
        public string Name { get; set; }
        public int UserId { get; set; }
        public int id { get; set; }
        public Friend(string idName, int UserId, int id)
        {
            this.id = id;
            this.UserId = UserId;
            this.Name = idName;
        }
    }
}
