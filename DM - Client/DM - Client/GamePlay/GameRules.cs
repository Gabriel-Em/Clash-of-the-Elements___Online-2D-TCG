using System.Collections.Generic;
using System.Linq;
using DM___Client.Models;

namespace DM___Client.GamePlay
{
    public enum DestroyedCardDestination
    {
        Graveyard,
        Hand,
        ManaZone
    }

    public struct BattleOutcome
    {
        public bool AttackerDestroyed;
        public bool DefenderDestroyed;
    }

    // The rules of the game that don't depend on the board's GUI: battles, attacking, blocking, effect targets and paying costs.
    public static class GameRules
    {
        public static bool HasEffect(Card card, string effect)
        {
            CardWithGameProperties cardInGame;

            foreach (SpecialEffect se in card.SpecialEffects)
                if (se.Effect == effect)
                    return true;

            cardInGame = card as CardWithGameProperties;
            if (cardInGame != null && cardInGame.grantedEffects.Contains(effect))
                return true;

            return false;
        }

        public static bool HasTrigger(Card card, string trigger)
        {
            foreach (SpecialEffect se in card.SpecialEffects)
            {
                if (se.Trigger == trigger)
                    return true;
            }
            return false;
        }

        public static string CodeToElement(int code)
        {
            switch (code)
            {
                case 1:
                    return "Air";
                case 2:
                    return "Earth";
                case 3:
                    return "Ether";
                case 4:
                    return "Fire";
                case 5:
                    return "Water";
            }
            return "";
        }

        // attack bonuses only apply to the attacking creature; ownGraveyard is the graveyard of the attacker's owner
        public static int GetPowerWhileAttacking(Card attacker, IEnumerable<Card> ownGraveyard)
        {
            int power = attacker.Power;

            foreach (SpecialEffect se in attacker.SpecialEffects)
            {
                switch (se.Effect)
                {
                    case "BloodThirst":
                        power += se.Arguments[0];
                        break;
                    case "BloodThirstRevenge":
                        if (se.Condition == "OwnGraveyardCivilization")
                        {
                            string element = CodeToElement(se.Arguments[1]);

                            power += se.Arguments[0] * ownGraveyard.Count(x => x.Element == element);
                        }
                        break;
                }
            }
            return power;
        }

        public static BattleOutcome ResolveBattle(Card attacker, int attackerPower, Card defender, int defenderPower)
        {
            BattleOutcome outcome = new BattleOutcome();

            if (attackerPower > defenderPower)
            {
                outcome.DefenderDestroyed = true;
                outcome.AttackerDestroyed = HasEffect(defender, "Poisonous") || HasEffect(attacker, "OnePunch");
            }
            else
            if (attackerPower < defenderPower)
            {
                outcome.AttackerDestroyed = true;
                outcome.DefenderDestroyed = HasEffect(attacker, "Poisonous") || HasEffect(defender, "OnePunch");
            }
            else
            {
                outcome.AttackerDestroyed = true;
                outcome.DefenderDestroyed = true;
            }

            return outcome;
        }

        public static DestroyedCardDestination GetDestroyedCardDestination(Card card)
        {
            if (HasEffect(card, "Unkillable"))
                return DestroyedCardDestination.Hand;
            if (HasEffect(card, "Recyclable"))
                return DestroyedCardDestination.ManaZone;
            return DestroyedCardDestination.Graveyard;
        }

        public static bool CanAttack(CardWithGameProperties creature)
        {
            return creature.hasCompletelyBeenSummoned && !HasEffect(creature, "CannotAttack");
        }

        public static bool CanAttackOpponent(Card attacker)
        {
            return !HasEffect(attacker, "OnlyAttacksCreatures");
        }

        // number of safeguards one attack breaks, before limiting it to the safeguards the opponent has left
        public static int GetSafeguardBreakCount(Card attacker)
        {
            if (!CanAttackOpponent(attacker))
                return 0;

            foreach (SpecialEffect se in attacker.SpecialEffects)
            {
                if (se.Effect == "SafeguardBreaker")
                    return 1 + se.Arguments[0];
            }
            return 1;
        }

        public static bool CanBlock(CardWithGameProperties creature)
        {
            return !creature.isEngaged && HasEffect(creature, "Defender");
        }

        public static bool CanBeBlocked(Card attacker)
        {
            return !HasEffect(attacker, "Slippery");
        }

        // whether a card in the effect's source zone can be chosen by the effect of sourceCard
        public static bool IsValidEffectTarget(SpecialEffect se, Card candidate, Card sourceCard)
        {
            // effects that affect creatures in any battle zone never affect the card that triggered them
            if (se.TargetFrom != null && se.TargetFrom.Contains("Any") && candidate == sourceCard)
                return false;

            if (se.Condition == "LessOrEqual")
                return candidate.Type == "Creature" && candidate.Power <= se.Arguments[1];

            return true;
        }

        // a card can be paid for with enough ready mana when at least one of it matches the card's element
        public static bool CanPayCost(Card card, IEnumerable<CardWithGameProperties> manaZone)
        {
            List<CardWithGameProperties> readyMana = manaZone.Where(x => !x.isEngaged).ToList();

            return card.Cost <= readyMana.Count && readyMana.Any(x => x.Element == card.Element);
        }
    }
}
