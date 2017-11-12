using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DM___Server.Models
{
    public class GameRoom
    {
        public int roomID { get; set; }
        public User Owner { get; set; }
        public User Joined { get; set; }
        public bool OwnerReady { get; set; }
        public bool JoinedReady { get; set; }
        public string State { get; set; }
        
        public GameRoom(int roomID_, User Owner_)
        {
            roomID = roomID_;
            Owner = Owner_;
            State = "Waiting...";
            OwnerReady = false;
            Joined = null;
            JoinedReady = false;
        }

        public void joinRoom(User user_)
        {
            Joined = user_;
        }

        public void leaveRoom()
        {
            Joined = null;
            JoinedReady = false;
        }

        public bool setOwnerReady(bool state)
        {
            OwnerReady = state;
            return GameStarted();
        }

        public bool setJoinedReady(bool state)
        {
            JoinedReady = state;
            return GameStarted();
        }

        private bool GameStarted()
        {
            if (OwnerReady && JoinedReady)
            {
                State = "Ongoing Match";
                return true;
            }

            return false;
        }
    }
}
