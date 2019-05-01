using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DM___Client.Models
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
            GameID = -1;
        }

        public GameMessage(string command, int gameID)
        {
            Command = command;
            stringArguments = null;
            intArguments = null;
            GameID = gameID;
        }

        public GameMessage(string command, int gameID, List<int> cardIDs)
        {
            Command = command;
            stringArguments = null;
            intArguments = cardIDs;
            GameID = gameID;
        }

        public GameMessage(string command, int gameID, List<string> arguments)
        {
            Command = command;
            stringArguments = arguments;
            intArguments = null;
            GameID = gameID;
        }
    }
}
