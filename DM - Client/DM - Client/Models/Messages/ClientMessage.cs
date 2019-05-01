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
        public List<string> stringArguments { get; set; }
        public List<int> intArguments { get; set; }
        public List<Card> CardCollection { get; set; }

        public ClientMessage()
        {
            Command = string.Empty;
            stringArguments = null;
            CardCollection = null;
            intArguments = null;
        }

        public ClientMessage(string Command, List<string> stringArguments, List<int> intArguments, List<Card> CardCollection)
        {
            this.Command = Command;
            this.stringArguments = stringArguments;
            this.CardCollection = CardCollection;
            this.intArguments = intArguments;
        }

        public ClientMessage(string Command, List<string> stringArguments)
        {
            this.Command = Command;
            this.stringArguments = stringArguments;
            CardCollection = null;
            intArguments = null;
        }

        public ClientMessage(string Command)
        {
            this.Command = Command;
            stringArguments = null;
            CardCollection = null;
            intArguments = null;
        }
    }
}
