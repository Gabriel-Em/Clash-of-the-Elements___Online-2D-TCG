using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DM___Server
{
    public class Controller
    {
        private List<User> usersInLobby;
        private List<User> usersOutsideLobby;
        private Database db;
        private List<Models.Card> CardCollection;
        private List<Models.GameRoom> GameRooms;
        private List<Models.Game> OnGoingGames;
        private object lobbyLock = new object();
        private object readyLock = new object();

        public Controller()
        {
            usersInLobby = new List<User>();
            usersOutsideLobby = new List<User>();
            OnGoingGames = new List<Models.Game>();
            GameRooms = new List<Models.GameRoom>();
            db = new DM___Server.Database();
            CardCollection = db.cardCollectionToList();
        }

        public void removeClient(Socket socket)
        {
            int index;

            index = getIndexOfSocketInLobby(socket);
            if (index != -1)
                usersInLobby.RemoveAt(index);
            index = getIndexOfSocketOutsideLobby(socket);
            if (index != -1)
                usersOutsideLobby.RemoveAt(index);

            removeSocketFromGameRooms(socket);
            removeSocketFromOnGoingGames(socket);
        }

        public string getUsernameOfClientSocket(Socket socket)
        {
            int index = getIndexOfSocketInLobby(socket);

            if (index != -1)
                return usersInLobby[index].Username;

            index = getIndexOfSocketOutsideLobby(socket);
            if (index != -1)
                return usersOutsideLobby[index].Username;
            return "<Guest>";
        }

        public string getNickNameOfClientSocket(Socket socket)
        {
            int index = getIndexOfSocketInLobby(socket);

            if (index != -1)
                return usersInLobby[index].NickName;

            index = getIndexOfSocketOutsideLobby(socket);
            if (index != -1)
                return usersOutsideLobby[index].NickName;
            return "<Guest>";
        }

        private int getIndexOfSocketInLobby(Socket socket)
        {
            for (int i = 0; i < usersInLobby.Count; i++)
                if (usersInLobby[i].Socket == socket)
                    return i;
            return -1;
        }

        private int getIndexOfSocketOutsideLobby(Socket socket)
        {
            for (int i = 0; i < usersOutsideLobby.Count; i++)
                if (usersOutsideLobby[i].Socket == socket)
                    return i;
            return -1;
        }
        private Socket getSocketWithUsername(string username)
        {
            foreach (User user in usersInLobby)
                if (user.Username == username)
                    return user.Socket;
            return null;
        }

        public Models.Response messageProcessor(Models.Message message, Socket socket)
        {
            if (message.Type == "ClientMessage")
            {
                Models.ClientMessage cm = message.Value.ToObject<Models.ClientMessage>();
                return commandProcessor(cm, socket);
            }
            else if (message.Type == "GameMessage")
            {
                Models.GameMessage gm = message.Value.ToObject<Models.GameMessage>();
                return gameCommandProcessor(gm, socket);
            }
            return null;
        }

        private Models.Response gameCommandProcessor(Models.GameMessage message, Socket socket)
        {
            return null;
        }

        private Models.Response commandProcessor(Models.ClientMessage message, Socket socket)
        {
            string responseToSender = "NotRequired";
            string responseToSockets = "NotRequired";
            List<string> commandArgumentsToSender = new List<string>();
            List<string> commandArgumentsToSockets = new List<string>();
            List<Socket> sockets = new List<Socket>();
            List<List<string>> rooms = new List<List<string>>();
            string data = string.Empty;
            int specialType = 0;
            int index;

            switch (message.Command)
            {
                case "LOGINREQUEST":
                    if (db.checkLogInCredentials(message.Arguments[0], message.Arguments[1]))
                    {
                        if (checkUserLoggedIn(message.Arguments[0]))
                            responseToSender = "ALREADYLOGGEDIN";
                        else
                        {
                            usersInLobby.Add(new User(message.Arguments[0], db.getNickNameOfUser(message.Arguments[0]), socket));
                            responseToSender = "LOGINPERMITTED";

                            index = getIndexOfSocketInLobby(socket);
                            if (index != -1)
                            {
                                responseToSockets = "ADDLOBBYROOMUSER";
                                commandArgumentsToSockets.Add(usersInLobby[index].NickName);
                                sockets = lobbyRoomUsersToSocketList();
                                sockets.Remove(socket);
                            }
                        }
                    }
                    else
                        responseToSender = "LOGINDENIED";
                    break;
                case "FORCELOGOUTREQUEST":
                    Socket s = getSocketWithUsername(message.Arguments[0]);
                    if (s != null)
                    {
                        removeClient(s);
                        sockets.Add(s);
                        responseToSockets = "REMOTEDISCONNECT";
                    }
                    break;
                case "REGISTERREQUEST":
                    {
                        Tuple<bool, List<string>> dbRegisterResponse = db.checkForRegisterInformation(message.Arguments[0], message.Arguments[1], message.Arguments[3]);
                        if (dbRegisterResponse.Item1)
                        {
                            db.insertNewUser(message.Arguments[0], message.Arguments[1], message.Arguments[2], message.Arguments[3]);
                            responseToSender = "REGISTERSUCCESSFULL";
                        }
                        else
                        {
                            responseToSender = "REGISTERFAILED";
                            commandArgumentsToSender = dbRegisterResponse.Item2;
                        }
                    }
                    break;
                case "GETLOBBYROOMUSERS":
                    commandArgumentsToSender = lobbyRoomUsersToCommandArguments(socket);
                    if (commandArgumentsToSender.Count == 0)
                        responseToSender = "NOLOBBYROOMUSERS";
                    else
                        responseToSender = "POPULATELOBBYROOM";
                    break;
                case "FETCHUSERDATA":
                    commandArgumentsToSender = db.fetchUserData(getUsernameOfClientSocket(socket));
                    if (commandArgumentsToSender.Count != 0)
                        responseToSender = "USERDATAFETCHED";
                    else
                        responseToSender = "ERRORFETCHDATA";
                    break;
                case "NEWCHATMESSAGE":
                    sockets = lobbyRoomUsersToSocketList();
                    responseToSockets = message.Command;
                    commandArgumentsToSockets = message.Arguments;
                    break;
                case "GETCARDCOLLECTION":
                    specialType = 2;
                    break;
                case "LEAVELOBBY":
                    lock (lobbyLock)
                    {
                        index = getIndexOfSocketInLobby(socket);
                        if (index != -1)
                        {
                            data = usersInLobby[index].NickName;
                            usersOutsideLobby.Add(usersInLobby[index]);
                            usersInLobby.RemoveAt(index);
                            responseToSockets = "REMOVELOBBYROOMUSER";
                            commandArgumentsToSockets.Add(data);
                            sockets = lobbyRoomUsersToSocketList();
                        };
                        removeSocketFromGameRooms(socket);
                    }
                    break;
                case "JOINLOBBY":
                    index = getIndexOfSocketOutsideLobby(socket);
                    if (index != -1)
                    {
                        usersInLobby.Add(usersOutsideLobby[index]);
                        usersOutsideLobby.RemoveAt(index);
                    }
                    index = getIndexOfSocketInLobby(socket);
                    if (index != -1)
                    {
                        responseToSockets = "ADDLOBBYROOMUSER";
                        commandArgumentsToSockets.Add(usersInLobby[index].NickName);
                        sockets = lobbyRoomUsersToSocketList();
                        sockets.Remove(socket);
                    }
                    break;
                case "GETDECKS":
                    commandArgumentsToSender = db.fetchUserDecks(getUsernameOfClientSocket(socket));
                    if (commandArgumentsToSender.Count == 0)
                        responseToSender = "NODECKS";
                    else
                        responseToSender = "DECKSDELIVERED";
                    break;
                case "CREATEGAMEROOM":
                    index = getIndexOfSocketInLobby(socket);
                    if(index!=-1)
                    {
                        if (db.userHasDecks(getUsernameOfClientSocket(socket)))
                        {
                            Models.GameRoom room = new Models.GameRoom(getFirstRoomIDAvailable(), usersInLobby[index]);
                            GameRooms.Add(room);
                            rooms.Add(gameRoomToCommandArguments(room));
                            sockets = lobbyRoomUsersToSocketList();
                            specialType = 3;
                        }
                        else
                            responseToSender = "DECKSREQUIREDTOJOIN";
                    }
                    break;
                case "GETGAMEROOMS":
                    foreach (Models.GameRoom room in GameRooms)
                        rooms.Add(gameRoomToCommandArguments(room));
                    specialType = 4;
                    break;
                case "CLOSEROOM":
                    if (removeGameRoom(Int32.Parse(message.Arguments[0])))
                    {
                        responseToSockets = "REMOVEROOM";
                        commandArgumentsToSockets = message.Arguments;
                        sockets = lobbyRoomUsersToSocketList();
                    }
                    break;
                case "JOINROOM":
                    index = getIndexOfSocketInLobby(socket);
                    if (index != -1)
                    {
                        if (db.userHasDecks(getUsernameOfClientSocket(socket)))
                        {
                            if (joinRoom(usersInLobby[index], Int32.Parse(message.Arguments[0])))
                            {
                                responseToSockets = "LINKUSERTOROOM";
                                commandArgumentsToSockets = new List<string>() { message.Arguments[0], usersInLobby[index].NickName };
                                sockets = lobbyRoomUsersToSocketList();
                            }
                        }
                        else
                            responseToSender = "DECKSREQUIREDTOJOIN";
                    }
                    break;
                case "LEAVEROOM":
                    index = getIndexOfSocketInLobby(socket);
                    if(index !=-1)
                    {
                        if(leaveRoom(usersInLobby[index], Int32.Parse(message.Arguments[0])))
                        {
                            responseToSockets = "REMOVEUSERFROMROOM";
                            commandArgumentsToSockets = new List<string>() { message.Arguments[0], usersInLobby[index].NickName };
                            sockets = lobbyRoomUsersToSocketList();
                        }
                    }
                    break;
                case "READYROOM":
                    index = getIndexOfSocketInLobby(socket);
                    if(index !=-1)
                    {
                        if (readyRoom(usersInLobby[index], Int32.Parse(message.Arguments[0]), bool.Parse(message.Arguments[1])))
                        {
                            Models.GameRoom gr = getGameRoomById(Int32.Parse(message.Arguments[0]));

                            if (gr != null)
                                if (gr.OwnerReady && gr.JoinedReady)
                                {
                                    responseToSockets = "JOINPREGAMEROOM";
                                    commandArgumentsToSockets = new List<string>() { gr.roomID.ToString() };
                                    sockets.Add(gr.Owner.Socket);
                                    sockets.Add(gr.Joined.Socket);
                                    OnGoingGames.Add(new Models.Game(gr.roomID, gr.Owner.Socket, gr.Joined.Socket));
                                    removeGameRoom(gr.roomID);
                                }
                                else
                                {
                                    responseToSockets = "SETREADY";
                                    commandArgumentsToSockets = message.Arguments;
                                    sockets.Add(gr.Owner.Socket);
                                    sockets.Add(gr.Joined.Socket);
                                }
                        }
                    }
                    break;
                case "DELETEDECK":
                    db.removeDeck(Int32.Parse(message.Arguments[0]));
                    break;
                case "GETDECKLIST":
                    commandArgumentsToSender = db.fetchUserDeckList(getUsernameOfClientSocket(socket));
                    responseToSender = "DECKLISTDELIVERED";
                    break;
                case "READYTOSTART":
                    {
                        lock (readyLock)
                        {
                            Models.Game game = getOnGoingGameByID(Int32.Parse(message.Arguments[0]));
                            if (game.isPlayer1(socket))
                                game.P1Ready = true;
                            else
                                game.P2Ready = true;

                            if (game.P1Ready && game.P2Ready)
                            {
                                responseToSockets = "READYTOGO";
                                sockets.Add(game.Player1Socket);
                                sockets.Add(game.Player2Socket);
                            }
                        }
                    }
                    break;
                case "SETDECK":
                    {
                        Models.Game game = getOnGoingGameByID(Int32.Parse(message.Arguments[0]));
                        string deck = db.getDeckByID(Int32.Parse(message.Arguments[1]));
                        if (game.isPlayer1(socket))
                            game.loadPlayer1InitialData(deck);
                        else
                            game.loadPlayer2InitialData(deck);
                        responseToSender = "DECKSET";
                    }
                    break;
                case "GETHAND":
                    {
                        Models.Game game = getOnGoingGameByID(Int32.Parse(message.Arguments[0]));
                        if (game.isPlayer1(socket))
                            commandArgumentsToSender = game.Player1Hand.Select(x=>x.ToString()).ToList<string>();
                        else
                            commandArgumentsToSender = game.Player2Hand.Select(x => x.ToString()).ToList<string>();
                        responseToSender = "HANDRECEIVED";
                    }
                    break;
                case "GETFIRSTGAMESTATE":
                    {
                        Models.Game game = getOnGoingGameByID(Int32.Parse(message.Arguments[0]));
                        if (game.isPlayer1(socket))
                            if (game.isPlayer1First)
                                responseToSender = "YOURINITTURN";
                            else
                                responseToSender = "OPPINITTURN";
                        else
                        {
                            if (game.isPlayer1First)
                                responseToSender = "OPPINITTURN";
                            else
                                responseToSender = "YOURINITTURN";
                        }
                    }
                    break;
                default: break;
            }

            switch (specialType)
            {
                case 2: return new Models.Response(message.Command, CardCollection);
                case 3: return new Models.Response(message.Command, rooms, sockets);
                case 4: return new Models.Response(message.Command, rooms);
                default: return new Models.Response(message.Command, new Models.ClientMessage(responseToSender, commandArgumentsToSender), new Models.ClientMessage(responseToSockets, commandArgumentsToSockets), sockets);
            }
        }

        private Models.Game getOnGoingGameByID(int id)
        {
            foreach (Models.Game game in OnGoingGames)
                if (game.ID == id)
                    return game;
            return null;
        }

        private Models.GameRoom getGameRoomById(int id)
        {
            for (int i =0;i<GameRooms.Count;i++)
                if (GameRooms[i].roomID == id)
                    return GameRooms[i];
            return null;
        }

        private bool removeGameRoom(int id)
        {
            for(int i =0;i<GameRooms.Count;i++)
                if(GameRooms[i].roomID == id)
                {
                    GameRooms.RemoveAt(i);
                    return true;
                }
            return false;
        }

        private bool joinRoom(User user, int id)
        {
            for(int i =0;i<GameRooms.Count;i++)
                if(GameRooms[i].roomID == id)
                {
                    if (GameRooms[i].Joined == null)
                    {
                        GameRooms[i].Joined = user;
                        return true;
                    }
                    else
                        return false;
                }
            return false;
        }

        private bool leaveRoom(User user, int id)
        {
            for (int i = 0; i < GameRooms.Count; i++)
                if (GameRooms[i].roomID == id)
                {
                    if (GameRooms[i].Joined.NickName == user.NickName)
                    {
                        GameRooms[i].Joined = null;
                        GameRooms[i].OwnerReady = false;
                        GameRooms[i].JoinedReady = false;
                        return true;
                    }
                    else
                        return false;
                }
            return false;
        }

        private bool readyRoom(User user, int id, bool owner)
        {
            for (int i = 0; i < GameRooms.Count; i++)
                if (GameRooms[i].roomID == id)
                {
                    if (owner)
                        GameRooms[i].OwnerReady = true;
                    else
                        GameRooms[i].JoinedReady = true;
                    return true;
                }
            return false;
        }

        private int getFirstRoomIDAvailable()
        {
            List<int> IDs = new List<int>();
            foreach (Models.GameRoom room in GameRooms)
                IDs.Add(room.roomID);

            IDs.Sort();
            for(int i =0;i<IDs.Count;i++)
            {
                if (i != IDs[i])
                    return i;
            }
            return IDs.Count;
        }

        private List<string> lobbyRoomUsersToCommandArguments(Socket socket)
        {
            List<string> nicknames = new List<string>();
            int index = getIndexOfSocketInLobby(socket);

            for (int i = 0; i < usersInLobby.Count; i++)
                if (i != index)
                    nicknames.Add(usersInLobby[i].NickName);

            return nicknames;
        }

        public bool checkUserLoggedIn(string username)
        {
            foreach (User user in usersInLobby)
                if (user.Username == username)
                    return true;
            return false;
        }

        public List<Socket> lobbyRoomUsersToSocketList()
        {
            List<Socket> sockets = sockets = new List<Socket>();
            foreach (User user in usersInLobby)
                sockets.Add(user.Socket);
            return sockets;
        }

        private List<string> gameRoomToCommandArguments(Models.GameRoom GameRoom)
        {
            List<string> commandArguments = new List<string>();

            commandArguments.Add(GameRoom.roomID.ToString());
            commandArguments.Add(GameRoom.Owner.NickName);
            if (GameRoom.Joined != null)
                commandArguments.Add(GameRoom.Joined.NickName);
            else
                commandArguments.Add("*");
            commandArguments.Add(GameRoom.OwnerReady.ToString());
            commandArguments.Add(GameRoom.JoinedReady.ToString());
            commandArguments.Add(GameRoom.State);

            return commandArguments;
        }

        private void removeSocketFromGameRooms(Socket socket)
        {
            for (int i = 0; i < GameRooms.Count; i++)
            {
                if (GameRooms[i].Owner.Socket == socket)
                {
                    GameRooms.RemoveAt(i);
                    i--;
                }
                else if (GameRooms[i].Joined != null && GameRooms[i].Joined.Socket == socket)
                {
                    GameRooms[i].Joined = null;
                    GameRooms[i].OwnerReady = false;
                    GameRooms[i].JoinedReady = false;
                }
            }
        }

        private void removeSocketFromOnGoingGames(Socket socket)
        {
            for (int i = 0; i < OnGoingGames.Count; i++)
                if (OnGoingGames[i].Player1Socket == socket || OnGoingGames[i].Player2Socket == socket)
                {
                    OnGoingGames.RemoveAt(i);
                    break;
                }
        }
    }
}