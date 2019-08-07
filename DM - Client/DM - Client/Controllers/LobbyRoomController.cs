using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DM___Client.Models;

namespace DM___Client.Controllers
{
    public class LobbyRoomController : Controller
    {
        private GUIPages.GUILobbyRoom parent;
        public List<Models.GameRoom> GameRooms;
        public Models.UserData userData { get; set; }

        public LobbyRoomController(GUIPages.GUILobbyRoom _parent, Communication _com)
        {
            parent = _parent;
            com = _com;
            GameRooms = new List<Models.GameRoom>();
            userData = new Models.UserData();
            loadedDataChecklist = new List<bool>() { false, false, false, parent.CardCollectionLoaded };
        }

        public override void loadPageData()
        {
            send(new Models.ClientMessage("GETLOBBYROOMUSERS"));
            send(new Models.ClientMessage("FETCHUSERDATA"));
            send(new Models.ClientMessage("GETGAMEROOMS"));
            if (!parent.CardCollectionLoaded)
                send(new Models.ClientMessage("GETCARDCOLLECTION"));
        }

        public override void clientCommandProcessor(Models.ClientMessage message)
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
                        parent.addLobbyRoomUsers(message.stringArguments);
                        loadedDataChecklist[0] = true;
                    break;
                case "ADDLOBBYROOMUSER":
                    parent.addLobbyRoomUsers(message.stringArguments);
                    break;
                case "REMOVELOBBYROOMUSER":
                    parent.removeLobbyRoomUser(message.stringArguments[0]);
                    if (message.stringArguments.Count == 1)
                        removePlayerFromGameRooms(message.stringArguments[0]);
                    break;
                case "USERDATAFETCHED":
                        if(isValidUserData(message.stringArguments))
                        {
                            updateUserData(message.stringArguments);
                            parent.updateLoggedInAs(userData.NickName);
                            loadedDataChecklist[1] = true;
                        }
                    break;
                case "NEWCHATMESSAGE":
                    parent.newChatMessage(message.stringArguments[0], message.stringArguments[1]);
                    break;
                case "NOLOBBYROOMUSERS":
                        loadedDataChecklist[0] = true;
                    break;
                case "ERRORFETCHDATA":
                    userData.Username = message.stringArguments[0];
                    break;
                case "YOUGOTCARDS":
                    parent.setCardCollection(message.CardCollection);
                    loadedDataChecklist[3] = true;
                    break;
                case "YOUGOTROOMS":
                    populateGameRooms(message.stringArguments);
                    loadedDataChecklist[2] = true;
                    break;
                case "ADDGAMEROOM":
                    createRoom(message.stringArguments[0]);
                    break;
                case "CLOSEROOM":
                    closeRoom(Int32.Parse(message.stringArguments[0]));
                    break;
                case "LINKUSERTOROOM":
                    linkUserToRoom(message.stringArguments);
                    break;
                case "REMOVEUSERFROMROOM":
                    removeUserFromRoom(message.stringArguments);
                    break;
                case "SETREADY":
                    setReadyInRoom(message.intArguments[0], message.stringArguments[0]);
                    break;
                case "DECKSREQUIREDTOJOIN":
                    parent.noRoomsForMe();
                    break;
                case "JOINPREGAMEROOM":
                    processJoinPreGameRoom(message);
                    break;
                case "SETROOMSTATE":
                    processSetRoomState(message);
                    break;
                case "ALREADYINAROOM":
                    parent.alreadyInARoom();
                    break;
                default: break;
            }
        }

        // GAME ROOMS

        // populates the GUI and game room list with game rooms received from server
        private void populateGameRooms(List<string> roomArguments)
        {
            foreach (string roomArgument in roomArguments)
            {
                createRoom(roomArgument);
            }
        }

        // creates a room object and its GUI
        public void createRoom(string roomArguments)
        {
            Models.GameRoom room = roomFromArguments(roomArguments);
            GameRooms.Add(room);
            parent.AddRoomToGUI(room);
        }

        // removes a room object and its GUI
        public void closeRoom(int id)
        {
            for (int i = 0; i < GameRooms.Count; i++)
            {
                if (GameRooms[i].RoomID == id)
                {
                    parent.RemoveRoomFromGUI(id);
                    GameRooms.RemoveAt(i);
                    break;
                }
            }
        }

        // updates a room object and its GUI with a user that joined that room
        private void linkUserToRoom(List<string> commandArguments)
        {
            int roomID = Int32.Parse(commandArguments[1]);

            for (int i = 0; i < GameRooms.Count; i++)
                if (GameRooms[i].RoomID == roomID)
                {
                    GameRooms[i].Guest = commandArguments[0];
                    break;
                }
            parent.playerJoinedRoom(roomID, commandArguments[0]);
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
                else if (GameRooms[i].Guest == nickName)
                {
                    parent.removePlayerFromRoom(GameRooms[i].RoomID, nickName);
                    GameRooms[i].Guest = "*";
                    GameRooms[i].OwnerIsReady = false;
                    GameRooms[i].GuestIsReady = false;
                }
            }
        }

        private Models.GameRoom roomFromArguments(string roomArguments)
        {
            int id;
            string owner;
            string guest;
            bool ownerIsReady;
            bool joinedIsReady;
            string state;

            string[] arguments = roomArguments.Split('`');
            id = Int32.Parse(arguments[0]);
            owner = arguments[1];
            guest = arguments[2];
            ownerIsReady = bool.Parse(arguments[3]);
            joinedIsReady = bool.Parse(arguments[4]);
            state = arguments[5];

            return new Models.GameRoom(id, owner, guest, ownerIsReady, joinedIsReady, state);
        }

        private void removeUserFromRoom(List<string> commandArguments)
        {
            int id = Int32.Parse(commandArguments[1]);

            for (int i = 0; i < GameRooms.Count; i++)
                if (GameRooms[i].RoomID == id)
                {
                    GameRooms[i].Guest = "*";
                    GameRooms[i].GuestIsReady = false;
                    GameRooms[i].OwnerIsReady = false;
                    break;
                }
            parent.removePlayerFromRoom(id, commandArguments[0]);
        }

        private void setReadyInRoom(int RoomId, string NickName)
        {
            for (int i = 0; i < GameRooms.Count; i++)
                if (GameRooms[i].RoomID == RoomId)
                {
                    if (GameRooms[i].Owner == NickName)
                        GameRooms[i].OwnerIsReady = true;
                    else
                        GameRooms[i].GuestIsReady = true;
                    break;
                }
            parent.setReady(RoomId, NickName);
        }

        // USER DATA

        private void updateUserData(List<string> data)
        {
            userData.NickName = data[0];
            userData.Email = data[1];
            userData.GamesWon = Int32.Parse(data[2]);
            userData.GamesLost = Int32.Parse(data[3]);
            userData.Username = data[4];
        }

        bool isValidUserData(List<string> commandArguments)
        {
            if(commandArguments.Count == 5)
            {
                int value;
                if (int.TryParse(commandArguments[2], out value) && int.TryParse(commandArguments[3], out value))
                    return true;
            }
            return false;
        }

        // CHAT MESSAGES

        public bool notEmpty(string text)
        {
            for (int i = 0; i < text.Length; i++)
                if (text[i] != ' ')
                    return true;
            return false;
        }

        private void processJoinPreGameRoom(Models.ClientMessage message)
        {
            int roomID;

            roomID = message.intArguments[0];

            foreach(Models.GameRoom gameRoom in GameRooms)
                if (gameRoom.RoomID == roomID)
                {
                    if (gameRoom.Owner == userData.NickName)
                    {
                        send(new Models.ClientMessage("SETROOMSTATE", new List<string>() { roomID.ToString(), "Battle in progress..." }));
                    }
                    break;
                }
            parent.joinPreGameRoom(roomID, message.stringArguments[0]);
        }

        private void processSetRoomState(Models.ClientMessage message)
        {
            parent.setRoomState(Int32.Parse(message.stringArguments[0]), message.stringArguments[1]);
        }
    }
}
