using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DM___Client.Models
{
    public class UserData
    {
        public string Username { get; set; }
        public string NickName { get; set; }
        public string Email { get; set; }
        public int GamesWon { get; set; }
        public int GamesLost { get; set; }

        public UserData()
        {
            Username = "UNDEFINED";
            NickName = "UNDEFINED";
            Email = "UNDEFINED";
            GamesWon = -1;
            GamesLost = -1;
        }
    }
}
