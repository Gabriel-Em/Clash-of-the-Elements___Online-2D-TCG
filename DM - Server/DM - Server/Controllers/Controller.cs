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
        private Database db;

        private object lobbyLock;
        private object readyLock;

        private Controllers.ControllerLogInAndRegistration ctrlLogInAndRegistration;
        private Controllers.ControllerLobby ctrlLobby;
        private Controllers.ControllerCollection ctrlCollection;
        private Controllers.ControllerPreGameRoom ctrlPreGameRoom;
        private Controllers.ControllerOnGoingGame ctrlOnGoingGame;

        public Data.LobbyRoomData lobbyRoomData;
        public Data.GameRoomsData gameRoomData;
        public Data.OnGoingGamesData onGoingGamesData;

        public Controller()
        {
            lobbyLock = new object();
            readyLock = new object();
            db = new DM___Server.Database();

            lobbyRoomData = new Data.LobbyRoomData(db);
            gameRoomData = new Data.GameRoomsData();
            onGoingGamesData = new Data.OnGoingGamesData();

            ctrlLogInAndRegistration = new Controllers.ControllerLogInAndRegistration(db, lobbyRoomData, this);
            ctrlLobby = new Controllers.ControllerLobby(db, lobbyRoomData, gameRoomData, onGoingGamesData);
            ctrlCollection = new Controllers.ControllerCollection(db, lobbyRoomData);
            ctrlPreGameRoom = new Controllers.ControllerPreGameRoom(db, lobbyRoomData);
            ctrlOnGoingGame = new Controllers.ControllerOnGoingGame(db, onGoingGamesData, lobbyRoomData);
        }

        public Models.Response messageProcessor(Models.Message message, Socket socket)
        {
            if (message.Type == "ClientMessage")
            {
                Models.ClientMessage cm = message.Value.ToObject<Models.ClientMessage>();
                return clientCommandProcessor(cm, socket);
            }
            else if (message.Type == "GameMessage")
            {
                Models.GameMessage gm = message.Value.ToObject<Models.GameMessage>();
                return gameCommandProcessor(gm, socket);
            }
            return null;
        }

        private Models.Response gameCommandProcessor(Models.GameMessage message, Socket sender)
        {
            Models.Response response;

            switch (message.Command)
            {
                case "PLAYASMANA":
                    response = ctrlOnGoingGame.processPlayAsMana(message, sender);
                    break;
                case "SETPHASE":
                    response = ctrlOnGoingGame.processSetPhase(message, sender);
                    break;
                case "ENDTURN":
                    response = ctrlOnGoingGame.processEndTurn(message, sender);
                    break;
                case "STARTTURN":
                    response = ctrlOnGoingGame.processStartTurn(message, sender);
                    break;
                case "DRAWCARD":
                    response = ctrlOnGoingGame.processDrawCard(message, sender);
                    break;
                case "SUMMON":
                    response = ctrlOnGoingGame.processSummon(message, sender);
                    break;
                case "ATTACKSAFEGUARDS":
                    response = ctrlOnGoingGame.processAttackSafeguards(message, sender);
                    break;
                case "BROKENSAFEGUARDS":
                    response = ctrlOnGoingGame.processBrokenSafeguards(message, sender);
                    break;
                case "YOUBROKEGUARD":
                    response = ctrlOnGoingGame.processYouBrokeGuard(message, sender);
                    break;
                case "ATTACKCREATURE":
                    response = ctrlOnGoingGame.processAttackCreature(message, sender);
                    break;
                case "BATTLE":
                    response = ctrlOnGoingGame.processDoBattle(message, sender);
                    break;
                case "SENDTO":
                    response = ctrlOnGoingGame.processSendTo(message, sender);
                    break;
                case "ATTACKOPPONENT":
                    response = ctrlOnGoingGame.processAttackOpponent(message, sender);
                    break;
                case "ISURRENDER":
                    response = ctrlOnGoingGame.processISurrender(message, sender);
                    break;
                default:
                    response = null;
                    break;
            }

            if(response != null)
                response.Type = 2;

            return response;
        }

        private Models.Response clientCommandProcessor(Models.ClientMessage message, Socket sender)
        {
            Models.Response response;

            switch (message.Command)
            {
                case "LOGINREQUEST":
                    response = ctrlLogInAndRegistration.processLogInRequest(message, sender);
                    break;
                case "FORCELOGOUTREQUEST":
                    response = ctrlLogInAndRegistration.processForceLogOutRequest(message, sender);
                    break;
                case "REGISTERREQUEST":
                    response = ctrlLogInAndRegistration.processRegisterRequest(message, sender);
                    break;
                case "GETLOBBYROOMUSERS":
                    response = ctrlLobby.processGetLobbyRoomUsers(sender);
                    break;
                case "FETCHUSERDATA":
                    response = ctrlLobby.processFetchUserData(sender);
                    break;
                case "NEWCHATMESSAGE":
                    response = ctrlLobby.processNewChatMessage(message, sender);
                    break;
                case "GETCARDCOLLECTION":
                    response = ctrlLobby.processGetCardCollection(sender);
                    break;
                case "LEAVELOBBY":
                    response = ctrlLobby.processLeaveLobby(message, sender);
                    break;
                case "JOINLOBBY":
                    response = ctrlLobby.processJoinLobby(sender);
                    break;
                case "GETGAMEROOMS":
                    response = ctrlLobby.processGetGameRooms(sender);
                    break;
                case "CREATEGAMEROOM":
                    response = ctrlLobby.processCreateGameRoom(sender);
                    break;
                case "CLOSEROOM":
                    response = ctrlLobby.processCloseGameRoom(message, sender);
                    break;
                case "JOINROOM":
                    response = ctrlLobby.processJoinGameRoom(message, sender);
                    break;
                case "LEAVEROOM":
                    response = ctrlLobby.processLeaveGameRoom(message, sender);
                    break;
                case "SETREADY":
                    response = ctrlLobby.processSetReady(message, sender);
                    break;
                case "GETDECKS":
                    response = ctrlCollection.processGetDecks(sender);
                    break;
                case "DELETEDECK":
                    response = ctrlCollection.processDeleteDeck(message);
                    break;
                case "GETDECKLIST":
                    response = ctrlPreGameRoom.processGetDeckList(sender);
                    break;
                case "READYTOSTART":
                    response = ctrlOnGoingGame.processReadyToStart(message, sender);
                    break;
                case "FETCHDECK":
                    response = ctrlOnGoingGame.processFetchDeck(message, sender);
                    break;
                case "GETHAND":
                    response = ctrlOnGoingGame.processGetHand(message, sender);
                    break;
                case "GETINITIALGAMESTATE":
                    response = ctrlOnGoingGame.processGetFirstGameState(message, sender);
                    break;
                case "SETROOMSTATE":
                    response = ctrlLobby.processSetRoomState(message);
                    break;
                case "CREATEDECK":
                    response = ctrlCollection.processCreateDeck(message, sender);
                    break;
                case "UPDATEDECK":
                    response = ctrlCollection.processUpdateDeck(message);
                    break;
                default:
                    response = null;
                    break;
            }

            if (response != null)
                response.Type = 1;
            return response;
        }

        public bool checkIfUserIsGuest(Socket socket)
        {
            if (lobbyRoomData.getUsernameBySocket(socket) == null)
                return true;
            return false;
        }

        public void removeClient(Socket clientSocket)
        {
            lobbyRoomData.removeClient(clientSocket);
            gameRoomData.removeClient(clientSocket);
            onGoingGamesData.removeGameByClient(clientSocket);
        }
    }
}