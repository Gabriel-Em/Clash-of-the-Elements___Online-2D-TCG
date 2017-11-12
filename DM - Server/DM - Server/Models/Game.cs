using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DM___Server.Models
{
    public class Game
    {
        public List<int> Player1Deck;
        public List<int> Player2Deck;
        public List<int> Player1ManaZone;
        public List<int> Player2ManaZone;
        public List<int> Player1Graveyard;
        public List<int> Player2Graveyard;
        public List<int> Player1BattleZone;
        public List<int> Player2BattleZone;
        public List<int> Player1Shields;
        public List<int> Player2Shields;
        public List<int> Player1Hand;
        public List<int> Player2Hand;
        public Socket Player1Socket;
        public Socket Player2Socket;
        public bool P1Ready { get; set; }
        public bool P2Ready { get; set; }
        public int ID { get; set; }
        public bool isPlayer1First { get; set; }

        public Game(int ID_, Socket player1, Socket player2)
        {
            ID = ID_;
            Player1Socket = player1;
            Player2Socket = player2;
            Player1Deck = new List<int>();
            Player2Deck = new List<int>();
            Player1ManaZone = new List<int>();
            Player2ManaZone = new List<int>();
            Player1Graveyard = new List<int>();
            Player2Graveyard = new List<int>();
            Player1BattleZone = new List<int>();
            Player2BattleZone = new List<int>();
            Player1Shields = new List<int>();
            Player2Shields = new List<int>();
            Player1Hand = new List<int>();
            Player2Hand = new List<int>();
            P1Ready = false;
            P2Ready = false;

            isPlayer1First = true;
            //Random r = new Random();

            //if (r.Next(1, 10000) % 2 == 0)
            //    isPlayer1First = true;
            //else
            //    isPlayer1First = false;
        }

        public void loadPlayer1InitialData(string deck)
        {
            Player1Deck = commandArgumentToDeck(deck);
            for (int i = 0; i < 5; i++)
                Player1Shields.Add(drawCard(ref Player1Deck));
            for (int i =0;i<5;i++)
                Player1Hand.Add(drawCard(ref Player1Deck));
        }

        public void loadPlayer2InitialData(string deck)
        {
            Player2Deck = commandArgumentToDeck(deck);
            for (int i = 0; i < 5; i++)
                Player2Shields.Add(drawCard(ref Player2Deck));
            for (int i = 0; i < 5; i++)
                Player2Hand.Add(drawCard(ref Player2Deck));
        }

        public bool isPlayer1(Socket socket)
        {
            if (Player1Socket == socket)
                return true;
            return false;
        }

        private List<int> commandArgumentToDeck(string argument)
        {
            int cardID;
            int count;

            string[] deckItems = argument.Split(';');
            List<int> Deck = new List<int>();

            foreach (string deckItem in deckItems)
            {
                string[] split = deckItem.Split('&');
                cardID = Int32.Parse(split[0]);
                count = Int32.Parse(split[1]);
                for (int k = 0; k < count; k++)
                    Deck.Add(cardID);
            }
            return ShuffleDeck(Deck);
        }

        private List<int> ShuffleDeck(List<int> deck)
        {
            RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
            int n = deck.Count;
            while (n > 1)
            {
                byte[] box = new byte[1];
                do provider.GetBytes(box);
                while (!(box[0] < n * (Byte.MaxValue / n)));
                int k = (box[0] % n);
                n--;
                int value = deck[k];
                deck[k] = deck[n];
                deck[n] = value;
            }

            return deck;
        }

        private int drawCard(ref List<int> deck)
        {
            int id;
            if (deck.Count > 0)
            {
                id = deck[0];
                deck.RemoveAt(0);
            }
            else
                id = -1;
            return id;
        }
    }
}
