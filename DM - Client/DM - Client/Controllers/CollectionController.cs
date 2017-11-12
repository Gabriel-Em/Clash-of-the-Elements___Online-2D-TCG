using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DM___Client.Controllers
{
    class CollectionController : Controller
    {
        private GUIPages.GUICollection parent;
        private Models.CardCollection CardCollection;

        public List<Models.CollectionDeck> Decks;
        public int currentIndex;
        public List<Models.Card> currentList;

        public CollectionController(GUIPages.GUICollection _parent, Communication _com, Models.CardCollection CardCollection_)
        {
            parent = _parent;
            com = _com;
            currentIndex = 0;
            currentList = CardCollection_.Cards;
            loadedData = new List<bool>() { false };
            CardCollection = CardCollection_;
            Decks = new List<Models.CollectionDeck>();
        }

        public override void commandProcessor(Models.ClientMessage message)
        {
            switch(message.Command)
            {
                case "DISCONNECTED":
                    parent.disconnected("Connection to server was lost and a log regarding the incident was created and deposited inside 'Logs' in apps home directory.", 0);
                    break;
                case "REMOTEDISCONNECT":
                    parent.disconnected("Your account was logged in from a different location.", -1);
                    break;
                case "NODECKS":
                    loadedData[0] = true;
                    break;
                case "DECKSDELIVERED":
                    commandArgumentsToDecks(message.Arguments);
                    parent.loadDecksToGUI();
                    loadedData[0] = true;
                    break;
                default: break;
            }
        }

        private void commandArgumentsToDecks(List<string> commandArguments)
        {
            int id;
            string deckName;
            int cardID;
            int count;

            foreach (string argument in commandArguments)
            {
                string[] splitData = argument.Split(',');

                id = Int32.Parse(splitData[0]);
                deckName = splitData[1];

                string[] deckItems = splitData[2].Split(';');
                List<Models.CollectionDeckItem> cdi = new List<Models.CollectionDeckItem>();
                foreach(string deckItem in deckItems)
                {
                    string[] split = deckItem.Split('&');
                    cardID = Int32.Parse(split[0]);
                    count = Int32.Parse(split[1]);
                    cdi.Add(new Models.CollectionDeckItem(CardCollection.getCardById(cardID), count));
                }

                cdi = cdi.OrderBy(x => x.Card.Cost).ToList<Models.CollectionDeckItem>();
                Decks.Add(new Models.CollectionDeck(id, deckName, cdi));
            }
        }

        public override void loadPageData()
        {
            send(new Models.ClientMessage("GETDECKS"));
        }

        public void filterCollection(string Type, string Civilization, string Cost, string costType, string Race, string Power, string powerType, string Set)
        {
            List<Models.Card> cards = new List<Models.Card>();
            bool isOk;
            foreach (Models.Card card in CardCollection.Cards)
            {
                isOk = true;
                if (Type != "Any" && card.Type != Type)
                    isOk = false;
                else if (Civilization != "Any" && card.Civilization != Civilization)
                    isOk = false;
                else if (card.Type == "Spell" && Race != "Any" || Race != "Any" && card.Race != Race)
                    isOk = false;
                else if (Set != "Any" && card.Set != Set)
                    isOk = false;
                else
                {
                    switch (costType)
                    {
                        case "=":
                            if (Cost != "Any" && card.Cost != Int32.Parse(Cost))
                                isOk = false;
                            break;
                        case "<=":
                            if (Cost != "Any" && card.Cost > Int32.Parse(Cost))
                                isOk = false;
                            break;
                        case ">=":
                            if (Cost != "Any" && card.Cost < Int32.Parse(Cost))
                                isOk = false;
                            break;
                    }
                        if (card.Type == "Spell" && Power != "Any")
                            isOk = false;
                        else
                        switch (powerType)
                        {
                            case "=":
                                if (Power != "Any" && card.Power != Int32.Parse(Power))
                                    isOk = false;
                                break;
                            case "<=":
                                if (Power != "Any" && card.Power > Int32.Parse(Power))
                                    isOk = false;
                                break;
                            case ">=":
                                if (Power != "Any" && card.Power < Int32.Parse(Power))
                                    isOk = false;
                                break;
                        }
                }

                if (isOk)
                    cards.Add(card);
            }

            currentIndex = 0;
            currentList = cards;
        }

        public Models.CollectionDeck getDeckByID(int id)
        {
            foreach (Models.CollectionDeck deck in Decks)
                if (deck.DeckID == id)
                    return deck;
            return null;
        }

        public void removeDeck(int ID)
        {
            for(int i =0;i<Decks.Count;i++)
                if(Decks[i].DeckID == ID)
                {
                    Decks.RemoveAt(i);
                    break;
                }
        }
    }
}
