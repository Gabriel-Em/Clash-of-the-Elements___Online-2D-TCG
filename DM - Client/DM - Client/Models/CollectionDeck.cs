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
        public List<CollectionDeckItem> CardList;

        public CollectionDeck(int DeckID_, string DeckName_, List<CollectionDeckItem> CardList_)
        {
            DeckID = DeckID_;
            DeckName = DeckName_;
            CardList = CardList_;
        }
    }
}
