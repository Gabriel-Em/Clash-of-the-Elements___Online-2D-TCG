using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DM___Client.Models
{
    [Serializable]
    public class ClientMessage
    {
        public string Command { get; set; }
        public List<string> Arguments { get; set; }
        public Card Card { get; set; }

        public ClientMessage()
        {
            Command = string.Empty;
            Arguments = null;
            Card = null;
        }

        public ClientMessage(string command, List<string> arguments)
        {
            Command = command;
            Arguments = arguments;
            Card = null;
        }

        public ClientMessage(string command, Card card)
        {
            Command = command;
            Arguments = null;
            Card = card;
        }

        public ClientMessage(string command)
        {
            Command = command;
            Arguments = null;
            Card = null;
        }
    }
}
