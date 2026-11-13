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

        // abilities given to this card by other cards' effects while it is in the battle zone
        public List<string> grantedEffects { get; private set; }

        public CardWithGameProperties(Card card) : base(card)
        {
            hasCompletelyBeenSummoned = false;
            isEngaged = false;
            grantedEffects = new List<string>();
        }

        public void resetProperties()
        {
            hasCompletelyBeenSummoned = false;
            isEngaged = false;
            grantedEffects.Clear();
        }
    }
}
