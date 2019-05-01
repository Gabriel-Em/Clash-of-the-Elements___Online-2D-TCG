using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DM___Server.Models
{
    public class GameMessage
    {
        public string Command { get; set; }
        public List<string> stringArguments { get; set; }
        public List<int> intArguments { get; set; }
        public int GameID { get; set; }

        public GameMessage(string command)
        {
            Command = command;
        }

        public GameMessage()
        {
            Command = string.Empty;
            stringArguments = new List<string>();
            intArguments = new List<int>();
        }

        public GameMessage(string command, List<string> arguments, List<int> cardIDS)
        {
            Command = command;
            stringArguments = arguments;
            intArguments = cardIDS;
        }
    }
}
