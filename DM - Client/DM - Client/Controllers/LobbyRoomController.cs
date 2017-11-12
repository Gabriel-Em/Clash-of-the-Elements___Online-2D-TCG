using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DM___Client.Controllers
{
    public class LobbyRoomController : Controller
    {
        private GUIPages.GUILobbyRoom parent;
        private Models.CardCollection CardCollection;
        public List<Models.GameRoom> GameRooms;
        public Models.UserData userData { get; set; }

        public LobbyRoomController(GUIPages.GUILobbyRoom _parent, Communication _com)
        {
            parent = _parent;
            com = _com;
            CardCollection = new Models.CardCollection();
            GameRooms = new List<Models.GameRoom>();
            userData = new Models.UserData("UNDEFINED", "UNDEFINED", "UNDEFINED", -1, -1);
            loadedData = new List<bool>() { false, false, false, parent.CardCollectionLoaded };
        }

        public override void commandProcessor(Models.ClientMessage message)
        {
            switch (message.Command)
            {
                case "DISCONNECTED":
                    parent.disconnected("Connection to server was lost and a log regarding the incident was created and deposited inside 'Logs' in apps home directory.", 0);
                    break;
                case "REMOTEDISCONNECT":
                    parent.disconnected("Your account was logged in from a different location.", -1);
                    break;
                case "POPULATELOBBYROOM":
                        parent.addLobbyRoomUsers(message.Arguments);
                        loadedData[0] = true;
                    break;
                case "ADDLOBBYROOMUSER":
                    parent.addLobbyRoomUsers(message.Arguments);
                    break;
                case "REMOVELOBBYROOMUSER":
                    parent.removeLobbyRoomUser(message.Arguments[0]);
                    removePlayerFromGameRooms(message.Arguments[0]);
                    break;
                case "USERDATAFETCHED":
                        if(isValidUserData(message.Arguments))
                        {
                            updateUserData(message.Arguments);
                            parent.updateLoggedInAs(userData.NickName);
                            loadedData[1] = true;
                        }
                    break;
                case "NEWCHATMESSAGE":
                    parent.newChatMessage(message.Arguments);
                    break;
                case "NOLOBBYROOMUSERS":
                        loadedData[0] = true;
                    break;
                case "ERRORFETCHDATA":
                    break;
                case "ADDNEWCARDTOCOLLECTION":
                    CardCollection.AddCard(message.Card);
                    break;
                case "COLLECTIONSENT":
                    parent.setCardCollection(CardCollection);
                    CardCollection = null;
                    loadedData[3] = true;
                    break;
                case "ADDGAMEROOM":
                    Models.GameRoom room = roomFromArguments(message.Arguments);
                    GameRooms.Add(room);
                    parent.AddRoomToGUI(room);
                    break;
                case "REMOVEROOM":
                    removeRoom(Int32.Parse(message.Arguments[0]));
                    break;
                case "ROOMSSENT":
                    loadedData[2] = true;
                    break;
                case "LINKUSERTOROOM":
                    linkUserToRoom(message.Arguments);
                    break;
                case "REMOVEUSERFROMROOM":
                    removeUserFromRoom(message.Arguments);
                    break;
                case "SETREADY":
                    setReadyInRoom(message.Arguments);
                    break;
                case "DECKSREQUIREDTOJOIN":
                    parent.noRoomsForMe();
                    break;
                case "JOINPREGAMEROOM":
                    parent.joinPreGameRoom(Int32.Parse(message.Arguments[0]));
                    break;
                default: break;
            }
        }

        private void removePlayerFromGameRooms(string nickName)
        {
            for (int i = 0; i < GameRooms.Count; i++)
            {
                if (GameRooms[i].Owner == nickName)
                {
                    parent.RemoveRoomFromGUI(GameRooms[i].RoomID);
                    GameRooms.RemoveAt(i);
                    i--;
                }
                else if (GameRooms[i].Joined == nickName)
                {
                    parent.playerLeftRoom(GameRooms[i].RoomID, nickName);
                    GameRooms[i].Joined = "*";
                    GameRooms[i].OwnerReady = false;
                    GameRooms[i].JoinedReady = false;
                }
            }
        }

        public void removeRoom(int id)
        {
            for(int i =0;i<GameRooms.Count;i++)
            {
                if(GameRooms[i].RoomID == id)
                {
                    parent.RemoveRoomFromGUI(id);
                    GameRooms.RemoveAt(i);
                    break;
                }
            }
        }

        private void updateUserData(List<string> data)
        {
            userData.NickName = data[0];
            userData.Email = data[1];

            try
            {
                userData.GamesWon = Int32.Parse(data[2]);
            }
            catch
            {
                userData.GamesWon = -1;
            }
            try
            {
                userData.GamesLost = Int32.Parse(data[3]);
            }
            catch
            {
                userData.GamesLost = -1;
            }
        }

        bool isValidUserData(List<string> commandArguments)
        {
            if(commandArguments.Count == 4)
            {
                int value;
                if (int.TryParse(commandArguments[2], out value) && int.TryParse(commandArguments[3], out value))
                    return true;
            }
            return false;
        }

        public bool notEmpty(string text)
        {
            for (int i = 0; i < text.Length; i++)
                if (text[i] != ' ')
                    return true;
            return false;
        }

        public override void loadPageData()
        {
            send(new Models.ClientMessage("GETLOBBYROOMUSERS"));
            send(new Models.ClientMessage("FETCHUSERDATA"));
            send(new Models.ClientMessage("GETGAMEROOMS"));
            if (!parent.CardCollectionLoaded)
                send(new Models.ClientMessage("GETCARDCOLLECTION"));
        }

        private Models.GameRoom roomFromArguments(List<string> arguments)
        {
            int id;
            string owner;
            string joined;
            bool ownerReady;
            bool joinedReady;
            string state;

            id = Int32.Parse(arguments[0]);
            owner = arguments[1];
            joined = arguments[2];
            ownerReady = bool.Parse(arguments[3]);
            joinedReady = bool.Parse(arguments[4]);
            state = arguments[5];

            return new Models.GameRoom(id, owner, joined, ownerReady, joinedReady, state);
        }

        private void linkUserToRoom(List<string> commandArguments)
        {
            int id = Int32.Parse(commandArguments[0]);

            for(int i =0;i<GameRooms.Count;i++)
                if(GameRooms[i].RoomID == id)
                {
                    GameRooms[i].Joined = commandArguments[1];
                    break;
                }
            parent.playerJoinedRoom(id, commandArguments[1]);
        }

        private void removeUserFromRoom(List<string> commandArguments)
        {
            int id = Int32.Parse(commandArguments[0]);

            for (int i = 0; i < GameRooms.Count; i++)
                if (GameRooms[i].RoomID == id)
                {
                    GameRooms[i].Joined = "*";
                    GameRooms[i].JoinedReady = false;
                    GameRooms[i].OwnerReady = false;
                    break;
                }
            parent.playerLeftRoom(id, commandArguments[1]);
        }

        private void setReadyInRoom(List<string> commandArguments)
        {
            int id = Int32.Parse(commandArguments[0]);
            bool owner = bool.Parse(commandArguments[1]);

            for (int i = 0; i < GameRooms.Count; i++)
                if (GameRooms[i].RoomID == id)
                {
                    if (owner)
                        GameRooms[i].JoinedReady = true;
                    else
                        GameRooms[i].OwnerReady = true;
                    break;
                }
            parent.setReady(id, owner);
        }

    }
}
