using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DM___Server.Data
{
    public class GameRoomsData
    {
        private object gameRoomLock = new object();
        public List<Models.GameRoom> GameRooms { get; set; }

        public GameRoomsData()
        {
            GameRooms = new List<Models.GameRoom>();
        }

        public int getFirstRoomIDAvailable()
        {
            lock (gameRoomLock)
            {
                List<int> IDs = new List<int>();
                foreach (Models.GameRoom room in GameRooms)
                    IDs.Add(room.roomID);

                IDs.Sort();
                for (int i = 0; i < IDs.Count; i++)
                {
                    if (i != IDs[i])
                    {
                        return i;
                    }
                }
                return IDs.Count;
            }
        }

        public Models.GameRoom createNewGameRoom(User owner)
        {
            lock (gameRoomLock)
            {
                Models.GameRoom room = new Models.GameRoom(getFirstRoomIDAvailable(), owner);
                GameRooms.Add(room);
                return room;
            }
        }

        public void closeGameRoom(int id)
        {
            lock (gameRoomLock)
            {
                for (int i = 0; i < GameRooms.Count; i++)
                {
                    if (GameRooms[i].roomID == id)
                    {
                        GameRooms.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        public bool linkUserToRoom(User user, int roomID)
        {
            lock (gameRoomLock)
            {
                for (int i = 0; i < GameRooms.Count; i++)
                {
                    if (GameRooms[i].roomID == roomID)
                    {
                        if (GameRooms[i].Guest == null)
                        {
                            GameRooms[i].Guest = user;
                            GameRooms[i].State = "Waiting...";
                            return true;
                        }
                        else
                            return false;
                    }
                }
                return false;
            }
        }

        public bool removeUserFromGameRoom(User user, int roomID)
        {
            lock (gameRoomLock)
            {
                for (int i = 0; i < GameRooms.Count; i++)
                    if (GameRooms[i].roomID == roomID)
                    {
                        if (GameRooms[i].Guest.NickName == user.NickName)
                        {
                            GameRooms[i].Guest = null;
                            GameRooms[i].OwnerIsReady = false;
                            GameRooms[i].GuestIsReady = false;
                            GameRooms[i].State = "Open...";
                            return true;
                        }
                        else
                            return false;
                    }
                return false;
            }
        }

        public bool setUserReady(User user, int roomID)
        {
            lock (gameRoomLock)
            {
                for (int i = 0; i < GameRooms.Count; i++)
                    if (GameRooms[i].roomID == roomID)
                    {
                        if (GameRooms[i].Owner.NickName == user.NickName)
                            GameRooms[i].OwnerIsReady = true;
                        else
                            GameRooms[i].GuestIsReady = true;
                        return true;
                    }
                return false;
            }
        }

        public Models.GameRoom getRoomByID(int roomID)
        {
            lock (gameRoomLock)
            {
                foreach (Models.GameRoom room in GameRooms)
                    if (room.roomID == roomID)
                        return room;
                return null;
            }
        }

        public void removeClient(Socket clientSocket)
        {
            lock (gameRoomLock)
            {
                for (int i = 0; i < GameRooms.Count; i++)
                {
                    if (GameRooms[i].Owner.Socket == clientSocket)
                    {
                        GameRooms.RemoveAt(i);
                        i--;
                    }
                    else if (GameRooms[i].Guest != null && GameRooms[i].Guest.Socket == clientSocket)
                    {
                        GameRooms[i].Guest = null;
                        GameRooms[i].OwnerIsReady = false;
                        GameRooms[i].GuestIsReady = false;
                    }
                }
            }
        }
    }
}
