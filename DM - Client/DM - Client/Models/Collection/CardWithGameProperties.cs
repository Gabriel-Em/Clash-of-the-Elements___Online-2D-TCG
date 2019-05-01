using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DM___Client.Models
{
    public class CardWithGameProperties : Card
    {
        public bool hasCompletelyBeenSummoned { get; set; }
        public bool isEngaged { get; set; }

        public CardWithGameProperties(Card card) : base(card)
        {
            hasCompletelyBeenSummoned = false;
            isEngaged = false;
        }

        public void resetProperties()
        {
            hasCompletelyBeenSummoned = false;
            isEngaged = false;
        }
    }
}
