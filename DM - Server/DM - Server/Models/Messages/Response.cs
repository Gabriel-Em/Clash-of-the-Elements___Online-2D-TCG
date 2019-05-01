using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DM___Server.Models
{
    public class Response
    {
        // response commands to users
        public string responseCommandToSender { get; set; }
        public string responseCommandToSockets { get; set; }

        // response arguments to users
        public List<string> commandStringArgumentsToSender { get; set; }
        public List<string> commandStringArgumentsToSockets { get; set; }

        public List<int> commandIntArgumentsToSender { get; set; }
        public List<int> commandIntArgumentsToSockets { get; set; }

        // sockets that need to receive the response
        public List<Socket> socketsToNotify { get; set; }
        public Socket sender { get; set; }

        public List<Card> CardCollection;

        public int Type { get; set; }

        public Response()
        {
            responseCommandToSender = null;
            responseCommandToSockets = null;
            commandStringArgumentsToSender = new List<string>();
            commandStringArgumentsToSockets = new List<string>();
            sender = null;
            socketsToNotify = new List<Socket>();
            CardCollection = null;
            commandIntArgumentsToSender = new List<int>();
            commandIntArgumentsToSockets = new List<int>();
        }

        public Response(Socket socket)
        {
            responseCommandToSender = null;
            responseCommandToSockets = null;
            commandStringArgumentsToSender = new List<string>();
            commandStringArgumentsToSockets = new List<string>();
            sender = socket;
            socketsToNotify = new List<Socket>();
            CardCollection = null;
            commandIntArgumentsToSender = new List<int>();
            commandIntArgumentsToSockets = new List<int>();
        }
    }
}
