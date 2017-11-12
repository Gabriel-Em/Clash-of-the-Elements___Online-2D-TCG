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
        public int messageType;
        public Models.ClientMessage cMessageToSender { get; set; }
        public Models.ClientMessage cMessageToSockets { get; set; }
        public Models.GameMessage gMessageToSender { get; set; }
        public Models.GameMessage gMessageToOpponent { get; set; }
        public List<Models.Card> CardCollection { get; set; }
        public List<Socket> cSockets { get; set; }
        public Socket gSocket { get; set; }
        public List<List<string>> GameRooms { get; set; }
        public string AssignedCommand { get; set; }

        public Response(string assignedCommand, Models.ClientMessage cMessageToSender_, Models.ClientMessage cMessageToSockets_, List<Socket> cSockets_)
        {
            AssignedCommand = assignedCommand;
            cMessageToSender = cMessageToSender_;
            cMessageToSockets = cMessageToSockets_;
            cSockets = cSockets_;

            messageType = 0;
        }

        public Response(string assignedCommand, Models.GameMessage gMessageToSender_, Models.GameMessage gMessageToOpponent_, Socket gSocket_)
        {
            AssignedCommand = assignedCommand;
            gMessageToSender = gMessageToSender_;
            gMessageToOpponent = gMessageToOpponent_;
            gSocket = gSocket_;

            messageType = 1;
        }

        public Response(string assignedCommand, List<Models.Card> CardCollection_)
        {
            AssignedCommand = assignedCommand;
            CardCollection = CardCollection_;

            messageType = 2;
        }

        public Response(string assignedCommand, List<List<string>> GameRooms_, List<Socket> cSockets_)
        {
            AssignedCommand = assignedCommand;
            GameRooms = GameRooms_;
            cSockets = cSockets_;

            messageType = 3;
        }

        public Response(string assignedCommand, List<List<string>> GameRooms_)
        {
            AssignedCommand = assignedCommand;
            GameRooms = GameRooms_;

            messageType = 4;
        }
    }
}
