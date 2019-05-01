using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DM___Server
{
    public class User
    {
        public string Username { get; set; }
        public string NickName { get; set; }
        public Socket Socket { get; set; }
        public string Location { get; set; }

        public User(string _username, string _nickname, Socket _socket, string location)
        {
            Username = _username;
            NickName = _nickname;
            Socket = _socket;
            Location = location;
        }
    }
}
