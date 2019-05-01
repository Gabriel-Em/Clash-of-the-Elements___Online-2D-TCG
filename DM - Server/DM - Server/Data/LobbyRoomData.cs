using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DM___Server.Data
{
    public class LobbyRoomData
    {
        private List<User> loggedInUsers;
        private object lobbyRoomLock = new object();

        public List<Models.Card> CardCollection { get; set; }

        public LobbyRoomData(Database db)
        {
            loggedInUsers = new List<User>();
            CardCollection = db.cardCollectionToList();
        }

        public bool checkUserLoggedIn(string username)
        {
            lock (lobbyRoomLock)
            {
                foreach (User user in loggedInUsers)
                    if (user.Username == username)
                        return true;
                return false;
            }
        }

        public void addUserToLoggedIn(string username, string nickname, Socket socket)
        {
            lock (lobbyRoomLock)
            {
                loggedInUsers.Add(
                    new User(
                        username,
                        nickname,
                        socket,
                        "Lobby"));
            }
        }

        public List<Socket> lobbyRoomUsersToSocketList()
        {
            lock (lobbyRoomLock)
            {
                List<Socket> sockets = sockets = new List<Socket>();
                foreach (User user in loggedInUsers)
                    if (user.Location == "Lobby")
                        sockets.Add(user.Socket);
                return sockets;
            }
        }

        public List<string> lobbyRoomUsersToNickNameList(Socket excludedSender)
        {
            lock (lobbyRoomLock)
            {
                List<string> nicknames = new List<string>();

                for (int i = 0; i < loggedInUsers.Count; i++)
                    if (loggedInUsers[i].Location == "Lobby" && loggedInUsers[i].Socket != excludedSender)
                        nicknames.Add(loggedInUsers[i].NickName);

                return nicknames;
            }
        }

        public Socket getSocketByUsername(string username)
        {
            lock (lobbyRoomLock)
            {
                foreach (User user in loggedInUsers)
                    if (user.Username == username)
                        return user.Socket;
                return null;
            }
        }

        public void removeClient(Socket clientSocket)
        {
            // remove client from loggedInUsers

            lock (lobbyRoomLock)
            {
                for (int i = 0; i < loggedInUsers.Count(); i++)
                {
                    if (loggedInUsers[i].Socket == clientSocket)
                    {
                        loggedInUsers.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        public string getUsernameBySocket(Socket sender)
        {
            lock (lobbyRoomLock)
                {
                foreach (User user in loggedInUsers)
                {
                    if (user.Socket == sender)
                        return user.Username;
                }

                return null;
            }
        }

        public string getNickNameBySocket(Socket sender)
        {
            lock (lobbyRoomLock)
            {
                foreach (User user in loggedInUsers)
                {
                    if (user.Socket == sender)
                        return user.NickName;
                }

                return null;
            }
        }

        public User getUserBySocket(Socket sender)
        {
            lock (lobbyRoomLock)
            {
                foreach (User user in loggedInUsers)
                {
                    if (user.Socket == sender)
                        return user;
                }

                return null;
            }
        }

        public void changeLocation(Socket sender, string location)
        {
            lock (lobbyRoomLock)
            {
                for (int i = 0; i < loggedInUsers.Count; i++)
                    if (loggedInUsers[i].Socket == sender)
                    {
                        loggedInUsers[i].Location = location;
                        break;
                    }
            }
        }
    }
}
