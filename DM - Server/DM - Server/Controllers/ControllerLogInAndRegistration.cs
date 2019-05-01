using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DM___Server.Controllers
{
    public class ControllerLogInAndRegistration
    {
        private Data.LobbyRoomData _lobbyRoomData;
        private Database _db;
        private Controller _parent;

        public ControllerLogInAndRegistration(Database db, Data.LobbyRoomData lobbyRoomData, Controller parent)
        {
            _db = db;
            _lobbyRoomData = lobbyRoomData;
            _parent = parent;
        }

        public Models.Response processLogInRequest(Models.ClientMessage message, Socket sender)
        {
            Models.Response response = new Models.Response(sender);

            // message.Arguments[0] = username
            // message.Arguments[1] = password

            // check log in information
            if (_db.checkLogInCredentials(message.stringArguments[0], message.stringArguments[1]))
            {
                // check if user is already logged in
                if (_lobbyRoomData.checkUserLoggedIn(message.stringArguments[0]))
                    response.responseCommandToSender = "ALREADYLOGGEDIN";
                else
                {
                    // add user to list of logged in users
                    string nickname = _db.getNickNameOfUser(message.stringArguments[0]);
                    _lobbyRoomData.addUserToLoggedIn(message.stringArguments[0], nickname, sender);

                    // send log in confirmation message
                    response.responseCommandToSender = "LOGINPERMITTED";

                    // update every logged in user's lobby room
                    response.responseCommandToSockets = "ADDLOBBYROOMUSER";
                    response.commandStringArgumentsToSockets.Add(nickname);
                  
                    response.socketsToNotify = _lobbyRoomData.lobbyRoomUsersToSocketList();
                    response.socketsToNotify.Remove(sender);
                }
            }
            else
                response.responseCommandToSender = "LOGINDENIED";

            return response;
        }

        public Models.Response processForceLogOutRequest(Models.ClientMessage message, Socket sender)
        {
            Models.Response response = new Models.Response(sender);

            // message.Arguments[0] = username

            Socket s = _lobbyRoomData.getSocketByUsername(message.stringArguments[0]);
            if (s != null)
            {
                response.responseCommandToSockets = "REMOTEDISCONNECT";
                response.socketsToNotify.Add(s);
                _parent.removeClient(s);
            }

            return response;
        }

        public Models.Response processRegisterRequest(Models.ClientMessage message, Socket sender)
        {
            Models.Response response = new Models.Response(sender);

            /* 
             * message.Arguments:
             * 
             * 0 - username
             * 1 - nickname
             * 2 - password
             * 3 - email
             */

            Tuple<bool, List<string>> dbRegisterResponse = _db.checkForRegisterInformation(message.stringArguments[0], message.stringArguments[1], message.stringArguments[3]);
            
            // if registration was successful
            if (dbRegisterResponse.Item1)
            {
                _db.insertNewUser(message.stringArguments[0], message.stringArguments[1], message.stringArguments[2], message.stringArguments[3]);
                response.responseCommandToSender = "REGISTERSUCCESSFULL";
            }
            else
            {
                response.responseCommandToSender = "REGISTERFAILED";
                response.commandStringArgumentsToSender = dbRegisterResponse.Item2;
            }

            return response;
        }
    }
}
