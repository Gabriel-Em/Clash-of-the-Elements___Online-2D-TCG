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
        public string Guest { get; set; }
        public bool OwnerIsReady { get; set; }
        public bool GuestIsReady { get; set; }
        public string State { get; set; }

        public GameRoom(int RoomID_, string Owner_, string Guest_, bool OwnerReady_, bool JoinedReady_, string State_)
        {
            RoomID = RoomID_;
            Owner = Owner_;
            Guest = Guest_;
            OwnerIsReady = OwnerReady_;
            GuestIsReady = JoinedReady_;
            State = State_;
        }

        public void joinRoom(string user_)
        {
            Guest = user_;
        }

        public void leaveRoom()
        {
            Guest = null;
            GuestIsReady = false;
        }

        public void setState(string State_)
        {
            State = State_;
        }

        public void setOwnerState(bool state)
        {
            OwnerIsReady = state;
        }

        public void setJoinedState(bool state)
        {
            GuestIsReady = state;
        }
    }
}
