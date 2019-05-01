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
        public User Guest { get; set; }
        public bool OwnerIsReady { get; set; }
        public bool GuestIsReady { get; set; }
        public string State { get; set; }
        
        public GameRoom(int roomID_, User Owner_)
        {
            roomID = roomID_;
            Owner = Owner_;
            Guest = null;
            OwnerIsReady = false;
            GuestIsReady = false;
            State = "Open...";
        }

        public void joinRoom(User user_)
        {
            Guest = user_;
        }

        public void leaveRoom()
        {
            Guest = null;
            GuestIsReady = false;
        }

        public bool SetOwnerState(bool state)
        {
            OwnerIsReady = state;
            return checkBothAreReady();
        }

        public bool setGuestState(bool state)
        {
            GuestIsReady = state;
            return checkBothAreReady();
        }

        private bool checkBothAreReady()
        {
            if (OwnerIsReady && GuestIsReady)
            {
                State = "Ongoing Match";
                return true;
            }

            return false;
        }

        public override string ToString()
        {
            string guestNickName;

            guestNickName = Guest != null ? Guest.NickName : "*";

            return
                roomID.ToString() + "`" +
                Owner.NickName + "`" +
                guestNickName + "`" +
                OwnerIsReady.ToString() + "`" + 
                GuestIsReady.ToString() + "`" + 
                State;
        }
    }
}
