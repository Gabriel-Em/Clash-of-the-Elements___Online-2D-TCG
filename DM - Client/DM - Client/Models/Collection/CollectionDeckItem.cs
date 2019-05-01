using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DM___Client.Models
{
    public class CollectionDeckItem
    {
        public Card Card { get; set; }
        public int Count { get; set; }

        public CollectionDeckItem(Card Card_, int Count_)
        {
            Card = Card_;
            Count = Count_;
        }
    }
}
