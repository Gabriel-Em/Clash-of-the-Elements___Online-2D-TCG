using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DM___Client.Models
{
    public class GameRoom
    {
        public int RoomID { get; set; }
        public string Owner { get; set; }
        public string Joined { get; set; }
        public bool OwnerReady { get; set; }
        public bool JoinedReady { get; set; }
        public string State { get; set; }

        public GameRoom(int RoomID_, string Owner_, string Joined_, bool OwnerReady_, bool JoinedReady_, string State_)
        {
            RoomID = RoomID_;
            Owner = Owner_;
            Joined = Joined_;
            OwnerReady = OwnerReady_;
            JoinedReady = JoinedReady_;
            State = State_;
        }

        public void joinRoom(string user_)
        {
            Joined = user_;
        }

        public void leaveRoom()
        {
            Joined = null;
            JoinedReady = false;
        }

        public void setState(string State_)
        {
            State = State_;
        }

        public void setOwnerReady(bool state)
        {
            OwnerReady = state;
        }

        public void setJoinedReady(bool state)
        {
            JoinedReady = state;
        }
    }
}
