using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using DM___Server.Models;

namespace DM___Server.Controllers
{
    public class ControllerCollection
    {
        private Database db;
        private Data.LobbyRoomData lobbyRoomData;

        public ControllerCollection(Database db, Data.LobbyRoomData lobbyRoomData)
        {
            this.db = db;
            this.lobbyRoomData = lobbyRoomData;
        }

        public Models.Response processGetDecks(Socket sender)
        {
            Models.Response response = new Models.Response(sender);

            response.commandStringArgumentsToSender = db.fetchUserDecks(lobbyRoomData.getUsernameBySocket(sender));
            if (response.commandStringArgumentsToSender.Count == 0)
            {
                response.responseCommandToSender = "NODECKSFOUND";
                response.commandStringArgumentsToSender.Clear();
            }
            else
                response.responseCommandToSender = "DECKSDELIVERED";

            return response;
        }

        public Models.Response processDeleteDeck(Models.ClientMessage message)
        {
            db.removeDeck(Int32.Parse(message.stringArguments[0]));

            return null;
        }

        public Response processCreateDeck(ClientMessage message, Socket sender)
        {
            Models.Response response = new Models.Response(sender);
            int deckID;

            deckID = db.createDeck(lobbyRoomData.getUsernameBySocket(sender), message.stringArguments[0]);

            if (deckID != -1)
            {
                response.responseCommandToSender = "DECKCREATED";
                response.commandStringArgumentsToSender.Add(message.stringArguments[0]);
                response.commandIntArgumentsToSender.Add(deckID);
            }

            return response;
        }

        public Response processUpdateDeck(ClientMessage message)
        {
            db.updateDeck(message.intArguments[0], message.stringArguments[0]);

            return null;
        }
    }
}
