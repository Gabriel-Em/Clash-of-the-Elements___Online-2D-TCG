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
        public List<Models.Card> filteredCollection;

        private const string ANY = "Any";
        private const int COMPARELESSTHANOREQUAL = 8;
        private const int COMPAREGREATERTHANOREQUAL = 9;
        private const int COMPAREEQUAL = 10;

        public CollectionController(GUIPages.GUICollection _parent, Communication _com, Models.CardCollection CardCollection_)
        {
            // attach parent, communication class and card collection
            parent = _parent;
            com = _com;
            CardCollection = CardCollection_;

            // the index of the first card that shows on GUI from the filteredCollection
            currentIndex = 0;

            // in the beginning there are no filters set
            filteredCollection = CardCollection_.Cards;

            // list of decks you have
            Decks = new List<Models.CollectionDeck>();

            loadedDataChecklist = new List<bool>() { false };
        }

        public override void loadPageData()
        {
            send(new Models.ClientMessage("GETDECKS"));
        }

        public override void clientCommandProcessor(Models.ClientMessage message)
        {
            switch(message.Command)
            {
                case "DISCONNECTED":
                    parent.disconnected("Connection to server was lost and a log regarding the incident was created and deposited inside 'Logs' in apps home directory.", 0);
                    break;
                case "REMOTEDISCONNECT":
                    parent.disconnected("Your account was logged in from a different location.", -1);
                    break;
                case "NODECKSFOUND":
                    loadedDataChecklist[0] = true;
                    break;
                case "DECKSDELIVERED":
                    commandArgumentsToDecks(message.stringArguments);
                    parent.loadDecksToGUI();
                    loadedDataChecklist[0] = true;
                    break;
                case "DECKCREATED":
                    processDeckCreated(message);
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
            List<Models.CollectionDeckItem> cardDeckItems;

            foreach (string argument in commandArguments)
            {
                string[] splitData = argument.Split('`');

                id = Int32.Parse(splitData[0]);
                deckName = splitData[1];

                cardDeckItems = new List<Models.CollectionDeckItem>();
                if (splitData.Length == 3)
                {
                    string[] deckItems = splitData[2].Split(';');
                    foreach (string deckItem in deckItems)
                    {
                        string[] split = deckItem.Split('&');
                        cardID = Int32.Parse(split[0]);
                        count = Int32.Parse(split[1]);
                        cardDeckItems.Add(new Models.CollectionDeckItem(CardCollection.getCardById(cardID), count));
                    }
                }

                if (cardDeckItems.Count != 0)
                    cardDeckItems = cardDeckItems.OrderBy(x => x.Card.Cost).ToList<Models.CollectionDeckItem>();

                Decks.Add(new Models.CollectionDeck(id, deckName, cardDeckItems));
            }
        }

        public void filterCollection(string cardType, string cardElement, int cardCost, int costComparator, int cardPower, int powerComparator)
        {
            List<Models.Card> filteredCards;
            bool keep;

            filteredCards = new List<Models.Card>();

            foreach (Models.Card card in CardCollection.Cards)
            {
                keep = true;
                if (cardType != ANY && card.Type != cardType)
                    keep = false;
                else if (cardElement != ANY && card.Element != cardElement)
                    keep = false;
                else
                {
                    if (cardCost != -1)
                    {
                        switch (costComparator)
                        {
                            case COMPARELESSTHANOREQUAL:
                                if (card.Cost > cardCost)
                                    keep = false;
                                break;
                            case COMPAREGREATERTHANOREQUAL:
                                if (card.Cost < cardCost)
                                    keep = false;
                                break;
                            case COMPAREEQUAL:
                                if (card.Cost != cardCost)
                                    keep = false;
                                break;
                        }
                    }
                    if (keep != false && cardPower != -1)
                    {
                        if (card.Type == "Spell")
                        {
                            keep = false;
                        }
                        else
                        {
                            switch (powerComparator)
                            {
                                case COMPARELESSTHANOREQUAL:
                                    if (card.Power > cardPower)
                                        keep = false;
                                    break;
                                case COMPAREGREATERTHANOREQUAL:
                                    if (card.Power < cardPower)
                                        keep = false;
                                    break;
                                case COMPAREEQUAL:
                                    if (card.Power != cardPower)
                                        keep = false;
                                    break;
                            }
                        }
                    }
                }

                if (keep)
                    filteredCards.Add(card);
            }

            currentIndex = 0;
            filteredCollection = filteredCards;
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

        private void processDeckCreated(Models.ClientMessage message)
        {
            Decks.Add(new Models.CollectionDeck(message.intArguments[0], message.stringArguments[0], new List<Models.CollectionDeckItem>()));
            parent.createNewDeckGUI(message.intArguments[0], message.stringArguments[0]);
        }

        public string collectionDeckToStringArgument(Models.CollectionDeck deck)
        {
            List<string> arguments;

            arguments = new List<string>();

            foreach(Models.CollectionDeckItem cdi in deck.CardList)
            {
                arguments.Add(string.Format("{0}&{1}", cdi.Card.ID, cdi.Count));
            }

            return String.Join(";", arguments.ToArray());
        }

        public void updateDeck(Models.CollectionDeck deck)
        {
            for (int i = 0; i < Decks.Count; i++)
            {
                if (Decks[i].DeckID == deck.DeckID)
                {
                    Decks[i] = deck.Clone();
                    break;
                }
            }
        }
    }
}
