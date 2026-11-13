using System.Collections.Generic;
using System.Linq;
using DM___Client.Models;

namespace DM___Client.Tests
{
    // builds cards the same way the server sends them: "None" fields arrive as null
    internal static class CardFactory
    {
        public static SpecialEffect Keyword(string effect, params int[] arguments)
        {
            return new SpecialEffect(effect, null, null, arguments.Length == 0 ? new List<int>() { -1 } : arguments.ToList(), null, null);
        }

        public static SpecialEffect Triggered(string effect, string trigger, string condition, string targetFrom, string targetTo, params int[] arguments)
        {
            return new SpecialEffect(effect, trigger, condition, arguments.ToList(), targetFrom, targetTo);
        }

        public static Card Creature(int power, string element = "Fire", int cost = 1, params SpecialEffect[] effects)
        {
            return new Card(1, "Creature", "", "Creature", element, cost, "Race", "", power, effects.ToList());
        }

        public static Card Spell(string element = "Fire", int cost = 1, params SpecialEffect[] effects)
        {
            return new Card(2, "Spell", "", "Spell", element, cost, null, "", -1, effects.ToList());
        }

        public static CardWithGameProperties InGame(Card card, bool engaged = false, bool summoned = true)
        {
            CardWithGameProperties cardInGame = new CardWithGameProperties(card);

            cardInGame.isEngaged = engaged;
            cardInGame.hasCompletelyBeenSummoned = summoned;
            return cardInGame;
        }
    }
}
