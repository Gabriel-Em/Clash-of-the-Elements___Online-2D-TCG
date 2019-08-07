using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using DM___Server.Models;

namespace DM___Server.Controllers
{
    public class ControllerLobby
    {
        private Database db;
        private Data.LobbyRoomData lobbyRoomData;
        private Data.GameRoomsData gameRoomData;
        private Data.OnGoingGamesData onGoingGamesData;

        public ControllerLobby(Database db, Data.LobbyRoomData lobbyRoomData, Data.GameRoomsData gameRoomData, Data.OnGoingGamesData onGoingGamesData)
        {
            this.db = db;
            this.lobbyRoomData = lobbyRoomData;
            this.gameRoomData = gameRoomData;
            this.onGoingGamesData = onGoingGamesData;
        }

        public Models.Response processGetLobbyRoomUsers(Socket sender)
        {
            Models.Response response = new Models.Response(sender);

            response.commandStringArgumentsToSender = lobbyRoomData.lobbyRoomUsersToNickNameList(sender);
            if (response.commandStringArgumentsToSender.Count == 0)
                response.responseCommandToSender = "NOLOBBYROOMUSERS";
            else
                response.responseCommandToSender = "POPULATELOBBYROOM";


            return response;
        }

        public Models.Response processFetchUserData(Socket sender)
        {
            Models.Response response = new Models.Response(sender);

            response.commandStringArgumentsToSender = db.fetchUserData(lobbyRoomData.getUsernameBySocket(sender));
            if (response.commandStringArgumentsToSender.Count != 0)
                response.responseCommandToSender = "USERDATAFETCHED";
            else
                response.responseCommandToSender = "ERRORFETCHDATA";

            return response;
        }

        public Models.Response processNewChatMessage(Models.ClientMessage message, Socket sender)
        {
            Models.Response response = new Models.Response(sender);

            response.socketsToNotify = lobbyRoomData.lobbyRoomUsersToSocketList();
            response.responseCommandToSockets = message.Command;
            response.commandStringArgumentsToSockets = message.stringArguments;

            return response;
        }

        public Models.Response processGetCardCollection(Socket sender)
        {
            Models.Response response = new Models.Response(sender);

            response.responseCommandToSender = "YOUGOTCARDS";
            response.CardCollection = lobbyRoomData.CardCollection;

            return response;
        }

        public Models.Response processLeaveLobby(Models.ClientMessage message, Socket sender)
        {
            Models.Response response = new Models.Response(sender);

            // we change player's location to wherever he is now that he left the lobby
            lobbyRoomData.changeLocation(sender, message.stringArguments[0]);

            // we remove any game rooms created by the player and remove the player from any rooms he joined

            response.socketsToNotify = lobbyRoomData.lobbyRoomUsersToSocketList();
            response.socketsToNotify.Remove(sender);
            response.responseCommandToSockets = "REMOVELOBBYROOMUSER";
            response.commandStringArgumentsToSockets.Add(lobbyRoomData.getNickNameBySocket(sender));

            if (message.stringArguments[0] != "PreGameRoom")
            {
                gameRoomData.removeClient(sender);
            }
            else
                response.commandStringArgumentsToSockets.Add(string.Empty);

            return response;
        }

        public Models.Response processJoinLobby(Socket sender)
        {
            Models.Response response = new Models.Response(sender);

            response.responseCommandToSockets = "ADDLOBBYROOMUSER";
            response.commandStringArgumentsToSockets.Add(lobbyRoomData.getNickNameBySocket(sender));
            response.socketsToNotify = lobbyRoomData.lobbyRoomUsersToSocketList();
            response.socketsToNotify.Remove(sender);

            lobbyRoomData.changeLocation(sender, "Lobby");

            return response;
        }

        public Response processGetGameRooms(Socket sender)
        {
            Models.Response response = new Models.Response(sender);

            response.responseCommandToSender = "YOUGOTROOMS";
            foreach (Models.GameRoom room in gameRoomData.GameRooms)
            {
                response.commandStringArgumentsToSender.Add(room.ToString());
            }

            return response;
        }

        public Models.Response processCreateGameRoom(Socket sender)
        {
            Models.Response response = new Models.Response(sender);

            // if the user that requested to create a game room has any available decks that it may play with
            if (db.userHasPlayableDecks(lobbyRoomData.getUsernameBySocket(sender)))
            {
                User user = lobbyRoomData.getUserBySocket(sender);

                if (gameRoomData.checkUserAlreadyInRoom(user.Username))
                {
                    response.responseCommandToSender = "ALREADYINAROOM";
                }
                else
                {
                    Models.GameRoom room = gameRoomData.createNewGameRoom(user);

                    response.socketsToNotify = lobbyRoomData.lobbyRoomUsersToSocketList();
                    response.commandStringArgumentsToSockets = new List<string>() { room.ToString() };
                    response.responseCommandToSockets = "ADDGAMEROOM";
                }
            }
            else
                response.responseCommandToSender = "DECKSREQUIREDTOJOIN";

            return response;
        }

        public Response processCloseGameRoom(ClientMessage message, Socket sender)
        {
            Response response = new Response(sender);
            int roomID;

            roomID = Int32.Parse(message.stringArguments[0]);

            gameRoomData.closeGameRoom(roomID);

            response.responseCommandToSockets = "CLOSEROOM";
            response.commandStringArgumentsToSockets = message.stringArguments;
            response.socketsToNotify = lobbyRoomData.lobbyRoomUsersToSocketList();

            return response;
        }

        public Response processJoinGameRoom(ClientMessage message, Socket sender)
        {
            Response response = new Response(sender);
            User user;

            user = lobbyRoomData.getUserBySocket(sender);

            // if the user that requested to join has available decks
            if (db.userHasPlayableDecks(user.Username))
            {
                if (gameRoomData.checkUserAlreadyInRoom(user.Username))
                {
                    response.responseCommandToSender = "ALREADYINAROOM";
                }
                else
                {
                    // if the user was able to join
                    if (gameRoomData.linkUserToRoom(user, Int32.Parse(message.stringArguments[0])))
                    {
                        response.responseCommandToSockets = "LINKUSERTOROOM";
                        response.commandStringArgumentsToSockets.Add(user.NickName);
                        response.commandStringArgumentsToSockets.Add(message.stringArguments[0]);
                        response.socketsToNotify = lobbyRoomData.lobbyRoomUsersToSocketList();
                    }
                }
            }
            else
                response.responseCommandToSender = "DECKSREQUIREDTOJOIN";

            return response;
        }

        public Response processLeaveGameRoom(ClientMessage message, Socket sender)
        {
            Response response = new Response(sender);
            User user;

            user = lobbyRoomData.getUserBySocket(sender);

            if (gameRoomData.removeUserFromGameRoom(user, Int32.Parse(message.stringArguments[0])))
            {
                response.responseCommandToSockets = "REMOVEUSERFROMROOM";
                response.commandStringArgumentsToSockets.Add(user.NickName);
                response.commandStringArgumentsToSockets.Add(message.stringArguments[0]);
                response.socketsToNotify = lobbyRoomData.lobbyRoomUsersToSocketList();
            }

            return response;
        }

        public Response processSetReady(ClientMessage message, Socket sender)
        {
            Response response = new Response(sender);
            User user;
            int roomID;

            user = lobbyRoomData.getUserBySocket(sender);
            roomID = Int32.Parse(message.stringArguments[0]);

            if (gameRoomData.setUserReady(user, roomID))
            {
                Models.GameRoom gr = gameRoomData.getRoomByID(roomID);

                if (gr != null)
                {
                    // if both players are set to ready they receive permission to start the match
                    if (gr.OwnerIsReady && gr.GuestIsReady)
                    {
                        response.responseCommandToSender = "JOINPREGAMEROOM";
                        response.responseCommandToSockets = "JOINPREGAMEROOM";

                        response.commandIntArgumentsToSender.Add(gr.roomID);
                        response.commandIntArgumentsToSockets.Add(gr.roomID);

                        gr.State = "Battle in progress...";

                        onGoingGamesData.createNewGame(gr.roomID, gr.Owner.Socket, gr.Guest.Socket);

                        if (sender == gr.Owner.Socket)
                        {
                            response.commandStringArgumentsToSender.Add(lobbyRoomData.getNickNameBySocket(gr.Guest.Socket));
                            response.commandStringArgumentsToSockets.Add(lobbyRoomData.getNickNameBySocket(gr.Owner.Socket));
                            response.socketsToNotify.Add(gr.Guest.Socket);
                        }
                        else
                        {
                            response.commandStringArgumentsToSender.Add(lobbyRoomData.getNickNameBySocket(gr.Owner.Socket));
                            response.commandStringArgumentsToSockets.Add(lobbyRoomData.getNickNameBySocket(gr.Guest.Socket));
                            response.socketsToNotify.Add(gr.Owner.Socket);
                        }
                    }
                    else
                    {
                        // otherwise just notify both players about the one that hit ready
                        response.responseCommandToSockets = "SETREADY";
                        response.commandIntArgumentsToSockets.Add(roomID);
                        response.commandStringArgumentsToSockets.Add(user.NickName);
                        response.socketsToNotify.Add(gr.Owner.Socket);
                        response.socketsToNotify.Add(gr.Guest.Socket);
                    }
                }
                
            }

            return response;
        }

        public Response processSetRoomState(ClientMessage message)
        {
            Response response = new Response();
            int gameID;
            Models.Game game;

            gameID = Int32.Parse(message.stringArguments[0]);
            game = onGoingGamesData.getGameByID(gameID);

            response.responseCommandToSockets = "SETROOMSTATE";
            response.socketsToNotify = lobbyRoomData.lobbyRoomUsersToSocketList();
            response.socketsToNotify.Remove(game.Player1Socket);
            response.socketsToNotify.Remove(game.Player2Socket);
            response.commandStringArgumentsToSockets.Add(gameID.ToString());
            response.commandStringArgumentsToSockets.Add(message.stringArguments[1]);

            return response;
        }
    }
}
