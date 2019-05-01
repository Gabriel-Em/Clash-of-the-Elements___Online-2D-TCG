using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DM___Client.Models
{
    public class CollectionDeck
    {
        public int DeckID;
        public string DeckName;
        public List<CollectionDeckItem> CardList { get; set; }
        public int Count { get; set; }

        public CollectionDeck(int DeckID_, string DeckName_, List<CollectionDeckItem> CardList_)
        {
            DeckID = DeckID_;
            DeckName = DeckName_;
            CardList = CardList_;

            Count = 0;
            foreach (CollectionDeckItem cdi in CardList_)
                Count += cdi.Count;
        }

        public CollectionDeck Clone()
        {
            CollectionDeck deck = new CollectionDeck(DeckID, DeckName, CardList.ToList());

            return deck;
        }

        public bool addCard(Card card)
        {
            bool found;

            found = false;
            foreach (CollectionDeckItem cdi in CardList)
            {
                if (card.ID == cdi.Card.ID)
                {
                    found = true;

                    if (cdi.Count == 4)
                        return false;

                    cdi.Count++;
                    Count++;
                    return true;
                }
            }

            if (!found)
            {
                CardList.Add(new CollectionDeckItem(card, 1));
                Count++;
                return true;
            }

            return false;
        }

        public bool removeCard(Card card)
        {
            foreach (CollectionDeckItem cdi in CardList)
            {
                if (card.ID == cdi.Card.ID)
                {
                    Count--;
                    if (cdi.Count == 1)
                    {
                        CardList.Remove(cdi);
                        return false;
                    }
                    else
                    {
                        cdi.Count--;
                        return true;
                    }
                }
            }
            return false;
        }

        public CollectionDeckItem getCollectionDeckItemByID(int cardID)
        {
            foreach(CollectionDeckItem cdi in CardList)
            {
                if (cdi.Card.ID == cardID)
                    return cdi;
            }
            return null;
        }
    }
}
