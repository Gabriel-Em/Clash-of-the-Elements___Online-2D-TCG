using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using DM___Server.Models;

namespace DM___Server.Controllers
{
    public class ControllerPreGameRoom
    {
        private Database _db;
        private Data.LobbyRoomData _lobbyRoomData;

        public ControllerPreGameRoom(Database db, Data.LobbyRoomData lobbyRoomData)
        {
            _db = db;
            _lobbyRoomData = lobbyRoomData;
        }

        public Models.Response processGetDeckList(Socket sender)
        {
            Models.Response response = new Models.Response(sender);

            response.commandStringArgumentsToSender = _db.fetchUserDeckList(_lobbyRoomData.getUsernameBySocket(sender));
            response.responseCommandToSender = "DECKLISTDELIVERED";

            return response;
        }
    }
}
