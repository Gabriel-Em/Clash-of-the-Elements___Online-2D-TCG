using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DM___Client.Models
{
    public class CardWithGameProperties : Card
    {
        public bool hasSummoningSickness { get; set; }
        public bool isTurned { get; set; }

        public CardWithGameProperties(Card card) : base(card)
        {
            if (card.Type != "Spell")
                hasSummoningSickness = true;
            isTurned = false;
        }
    }
}
