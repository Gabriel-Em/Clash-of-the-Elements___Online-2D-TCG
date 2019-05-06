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
        public int ID { get; set; }

        public List<int> listPlayer1Deck;
        public List<int> listPlayer2Deck;

        public List<int> listPlayer1ManaZone;
        public List<int> listPlayer2ManaZone;

        public List<int> listPlayer1Graveyard;
        public List<int> listPlayer2Graveyard;

        public List<int> listPlayer1BattleGround;
        public List<int> listPlayer2BattleGround;

        public List<int> listPlayer1Safeguards;
        public List<int> listPlayer2Safeguards;

        public List<int> listPlayer1Hand;
        public List<int> listPlayer2Hand;

        public Socket Player1Socket;
        public Socket Player2Socket;

        public bool isP1Ready { get; set; }
        public bool isP2Ready { get; set; }

        public bool isPlayer1First { get; set; }

        public Game(int ID_, Socket player1, Socket player2)
        {
            ID = ID_;
            Player1Socket = player1;
            Player2Socket = player2;

            initLists();

            isP1Ready = false;
            isP2Ready = false;

            Random r = new Random();

            if (r.Next(1, 10000) % 2 == 0)
                isPlayer1First = true;
            else
                isPlayer1First = false;
        }

        private void initLists()
        {
            listPlayer1Deck = new List<int>();
            listPlayer2Deck = new List<int>();
            listPlayer1ManaZone = new List<int>();
            listPlayer2ManaZone = new List<int>();
            listPlayer1Graveyard = new List<int>();
            listPlayer2Graveyard = new List<int>();
            listPlayer1Safeguards = new List<int>();
            listPlayer2BattleGround = new List<int>();
            listPlayer1Safeguards = new List<int>();
            listPlayer2Safeguards = new List<int>();
            listPlayer1Hand = new List<int>();
            listPlayer2Hand = new List<int>();
        }

        public void loadPlayer1InitialData(string deck)
        {
            listPlayer1Deck = commandArgumentToDeck(deck);
            for (int i = 0; i < 5; i++)
            {
                listPlayer1Safeguards.Add(listPlayer1Deck[0]);
                listPlayer1Deck.RemoveAt(0);
            }
            for (int i = 0; i < 5; i++)
                drawCard(1);
        }

        public void loadPlayer2InitialData(string deck)
        {
            listPlayer2Deck = commandArgumentToDeck(deck);
            for (int i = 0; i < 5; i++)
            {
                listPlayer2Safeguards.Add(listPlayer2Deck[0]);
                listPlayer2Deck.RemoveAt(0);
            }
            for (int i = 0; i < 5; i++)
                drawCard(2);
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

        public int drawCard(int player)
        {
            int cardID;

            if (player == 1)
            {
                if (listPlayer1Deck.Count > 0)
                {
                    cardID = listPlayer1Deck[0];
                    listPlayer1Deck.RemoveAt(0);
                    listPlayer1Hand.Add(cardID);
                }
                else
                    cardID = -1;
            }
            else
            {
                if (listPlayer2Deck.Count > 0)
                {
                    cardID = listPlayer2Deck[0];
                    listPlayer2Deck.RemoveAt(0);
                    listPlayer2Hand.Add(cardID);
                }
                else
                    cardID = -1;
            }
            return cardID;
        }

        public void playAsMana(int cardID, int player)
        {
            if (player == 1)
            {
                listPlayer1Hand.Remove(cardID);
                listPlayer1ManaZone.Add(cardID);
            }
            else
            {
                listPlayer2Hand.Remove(cardID);
                listPlayer2ManaZone.Add(cardID);
            }
        }

        public void summon(int cardID, int player)
        {
            if (player == 1)
            {
                listPlayer1Hand.Remove(cardID);
                listPlayer1Safeguards.Add(cardID);
            }
            else
            {
                listPlayer2Hand.Remove(cardID);
                listPlayer2BattleGround.Add(cardID);
            }
        }

        public int breakSafeguard(int safeguardIndex, int player)
        {
            int cardID;

            if (player == 1)
            {
                cardID = listPlayer1Safeguards[safeguardIndex];
                listPlayer1Safeguards.RemoveAt(safeguardIndex);
                listPlayer1Hand.Add(cardID);
            }
            else
            {
                cardID = listPlayer2Safeguards[safeguardIndex];
                listPlayer2Safeguards.RemoveAt(safeguardIndex);
                listPlayer2Hand.Add(cardID);
            }
            return cardID;
        }
    }
}
