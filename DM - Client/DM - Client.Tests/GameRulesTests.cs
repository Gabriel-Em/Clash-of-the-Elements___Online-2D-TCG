using System.Collections.Generic;
using DM___Client.GamePlay;
using DM___Client.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static DM___Client.Tests.CardFactory;

namespace DM___Client.Tests
{
    [TestClass]
    public class BattleResolutionTests
    {
        private static BattleOutcome Fight(Card attacker, int attackerPower, Card defender, int defenderPower)
        {
            return GameRules.ResolveBattle(attacker, attackerPower, defender, defenderPower);
        }

        [TestMethod]
        public void StrongerAttackerDestroysOnlyTheDefender()
        {
            BattleOutcome outcome = Fight(Creature(5000), 5000, Creature(3000), 3000);

            Assert.IsFalse(outcome.AttackerDestroyed);
            Assert.IsTrue(outcome.DefenderDestroyed);
        }

        [TestMethod]
        public void WeakerAttackerIsDestroyedAlone()
        {
            BattleOutcome outcome = Fight(Creature(2000), 2000, Creature(3000), 3000);

            Assert.IsTrue(outcome.AttackerDestroyed);
            Assert.IsFalse(outcome.DefenderDestroyed);
        }

        [TestMethod]
        public void EqualPowerDestroysBoth()
        {
            BattleOutcome outcome = Fight(Creature(3000), 3000, Creature(3000), 3000);

            Assert.IsTrue(outcome.AttackerDestroyed);
            Assert.IsTrue(outcome.DefenderDestroyed);
        }

        [TestMethod]
        public void ZeroPowerTieDestroysBoth()
        {
            BattleOutcome outcome = Fight(Creature(0), 0, Creature(0), 0);

            Assert.IsTrue(outcome.AttackerDestroyed);
            Assert.IsTrue(outcome.DefenderDestroyed);
        }

        [TestMethod]
        public void OnePowerPointDecidesTheBattle()
        {
            BattleOutcome attackerAhead = Fight(Creature(3001), 3001, Creature(3000), 3000);
            BattleOutcome defenderAhead = Fight(Creature(3000), 3000, Creature(3001), 3001);

            Assert.IsFalse(attackerAhead.AttackerDestroyed);
            Assert.IsTrue(attackerAhead.DefenderDestroyed);
            Assert.IsTrue(defenderAhead.AttackerDestroyed);
            Assert.IsFalse(defenderAhead.DefenderDestroyed);
        }

        [TestMethod]
        public void BattleUsesThePowersPassedInNotThePrintedPower()
        {
            // printed 2000 attacker boosted to 3000 beats a 2500 defender
            BattleOutcome outcome = Fight(Creature(2000), 3000, Creature(2500), 2500);

            Assert.IsFalse(outcome.AttackerDestroyed);
            Assert.IsTrue(outcome.DefenderDestroyed);
        }

        [TestMethod]
        public void PoisonousDefenderThatLosesTakesTheAttackerWithIt()
        {
            BattleOutcome outcome = Fight(Creature(5000), 5000, Creature(1000, "Ether", 3, Keyword("Poisonous")), 1000);

            Assert.IsTrue(outcome.AttackerDestroyed);
            Assert.IsTrue(outcome.DefenderDestroyed);
        }

        [TestMethod]
        public void PoisonousAttackerThatLosesTakesTheDefenderWithIt()
        {
            BattleOutcome outcome = Fight(Creature(1000, "Ether", 3, Keyword("Poisonous")), 1000, Creature(5000), 5000);

            Assert.IsTrue(outcome.AttackerDestroyed);
            Assert.IsTrue(outcome.DefenderDestroyed);
        }

        [TestMethod]
        public void PoisonousWinnerOnlyDestroysTheLoser()
        {
            BattleOutcome outcome = Fight(Creature(5000, "Ether", 3, Keyword("Poisonous")), 5000, Creature(1000), 1000);

            Assert.IsFalse(outcome.AttackerDestroyed);
            Assert.IsTrue(outcome.DefenderDestroyed);
        }

        [TestMethod]
        public void OnePunchAttackerIsDestroyedAfterWinning()
        {
            BattleOutcome outcome = Fight(Creature(5000, "Fire", 1, Keyword("OnePunch")), 5000, Creature(1000), 1000);

            Assert.IsTrue(outcome.AttackerDestroyed);
            Assert.IsTrue(outcome.DefenderDestroyed);
        }

        [TestMethod]
        public void OnePunchDefenderIsDestroyedAfterWinning()
        {
            BattleOutcome outcome = Fight(Creature(1000), 1000, Creature(5000, "Fire", 1, Keyword("OnePunch")), 5000);

            Assert.IsTrue(outcome.AttackerDestroyed);
            Assert.IsTrue(outcome.DefenderDestroyed);
        }

        [TestMethod]
        public void OnePunchLoserChangesNothing()
        {
            BattleOutcome outcome = Fight(Creature(5000), 5000, Creature(1000, "Fire", 1, Keyword("OnePunch")), 1000);

            Assert.IsFalse(outcome.AttackerDestroyed);
            Assert.IsTrue(outcome.DefenderDestroyed);
        }

        [TestMethod]
        public void AraneidaeAlwaysTakesTheOtherCreatureDown()
        {
            Card araneidae = Creature(1000, "Ether", 3, Keyword("Poisonous"), Keyword("OnePunch"));

            foreach (int otherPower in new[] { 500, 1000, 8000 })
            {
                BattleOutcome asAttacker = Fight(araneidae, 1000, Creature(otherPower), otherPower);
                BattleOutcome asDefender = Fight(Creature(otherPower), otherPower, araneidae, 1000);

                Assert.IsTrue(asAttacker.AttackerDestroyed && asAttacker.DefenderDestroyed, "attacking a " + otherPower + " creature");
                Assert.IsTrue(asDefender.AttackerDestroyed && asDefender.DefenderDestroyed, "attacked by a " + otherPower + " creature");
            }
        }

        [TestMethod]
        public void GrantedEffectsCountInBattle()
        {
            CardWithGameProperties defender = InGame(Creature(1000));
            defender.grantedEffects.Add("Poisonous");

            BattleOutcome outcome = Fight(Creature(5000), 5000, defender, 1000);

            Assert.IsTrue(outcome.AttackerDestroyed);
        }
    }

    [TestClass]
    public class AttackPowerTests
    {
        [TestMethod]
        public void CreatureWithoutBonusesUsesItsPrintedPower()
        {
            Assert.AreEqual(3000, GameRules.GetPowerWhileAttacking(Creature(3000), new List<Card>()));
        }

        [TestMethod]
        public void BloodthirstAddsItsBonus()
        {
            // Qroax: 2000 power, attacks with 3000
            Card qroax = Creature(2000, "Fire", 2, Keyword("BloodThirst", 1000));

            Assert.AreEqual(3000, GameRules.GetPowerWhileAttacking(qroax, new List<Card>()));
        }

        [TestMethod]
        public void SeveralBloodthirstBonusesStack()
        {
            Card card = Creature(1000, "Fire", 2, Keyword("BloodThirst", 1000), Keyword("BloodThirst", 500));

            Assert.AreEqual(2500, GameRules.GetPowerWhileAttacking(card, new List<Card>()));
        }

        [TestMethod]
        public void RevengeBonusCountsOnlyCardsOfTheChosenElementInTheGraveyard()
        {
            // +1000 per Fire card (element code 4)
            Card card = Creature(2000, "Fire", 3, Triggered("BloodThirstRevenge", null, "OwnGraveyardCivilization", null, null, 1000, 4));
            List<Card> graveyard = new List<Card>() { Creature(1, "Fire"), Creature(1, "Water"), Spell("Fire"), Creature(1, "Air") };

            Assert.AreEqual(4000, GameRules.GetPowerWhileAttacking(card, graveyard));
        }

        [TestMethod]
        public void RevengeBonusWithAnEmptyGraveyardAddsNothing()
        {
            Card card = Creature(2000, "Fire", 3, Triggered("BloodThirstRevenge", null, "OwnGraveyardCivilization", null, null, 1000, 4));

            Assert.AreEqual(2000, GameRules.GetPowerWhileAttacking(card, new List<Card>()));
        }

        [TestMethod]
        public void RevengeBonusWithoutItsConditionAddsNothing()
        {
            Card card = Creature(2000, "Fire", 3, Triggered("BloodThirstRevenge", null, null, null, null, 1000, 4));
            List<Card> graveyard = new List<Card>() { Creature(1, "Fire") };

            Assert.AreEqual(2000, GameRules.GetPowerWhileAttacking(card, graveyard));
        }
    }

    [TestClass]
    public class ElementCodeTests
    {
        [TestMethod]
        public void CodesMapToTheFiveElements()
        {
            Assert.AreEqual("Air", GameRules.CodeToElement(1));
            Assert.AreEqual("Earth", GameRules.CodeToElement(2));
            Assert.AreEqual("Ether", GameRules.CodeToElement(3));
            Assert.AreEqual("Fire", GameRules.CodeToElement(4));
            Assert.AreEqual("Water", GameRules.CodeToElement(5));
        }

        [TestMethod]
        public void CodesOutsideTheRangeMapToNoElement()
        {
            Assert.AreEqual("", GameRules.CodeToElement(0));
            Assert.AreEqual("", GameRules.CodeToElement(6));
            Assert.AreEqual("", GameRules.CodeToElement(-1));
        }
    }

    [TestClass]
    public class DestructionTests
    {
        [TestMethod]
        public void DestroyedCreatureGoesToTheGraveyard()
        {
            Assert.AreEqual(DestroyedCardDestination.Graveyard, GameRules.GetDestroyedCardDestination(Creature(1000)));
        }

        [TestMethod]
        public void UnkillableCreatureReturnsToHand()
        {
            Assert.AreEqual(DestroyedCardDestination.Hand, GameRules.GetDestroyedCardDestination(Creature(1000, "Fire", 1, Keyword("Unkillable"))));
        }

        [TestMethod]
        public void RecyclableCreatureGoesToTheManaZone()
        {
            Assert.AreEqual(DestroyedCardDestination.ManaZone, GameRules.GetDestroyedCardDestination(Creature(1000, "Fire", 1, Keyword("Recyclable"))));
        }

        [TestMethod]
        public void UnkillableTakesPriorityOverRecyclable()
        {
            Card card = Creature(1000, "Fire", 1, Keyword("Recyclable"), Keyword("Unkillable"));

            Assert.AreEqual(DestroyedCardDestination.Hand, GameRules.GetDestroyedCardDestination(card));
        }

        [TestMethod]
        public void RecyclableGrantedByTerraneSendsTheCreatureToTheManaZone()
        {
            CardWithGameProperties card = InGame(Creature(1000));
            card.grantedEffects.Add("Recyclable");

            Assert.AreEqual(DestroyedCardDestination.ManaZone, GameRules.GetDestroyedCardDestination(card));
        }

        [TestMethod]
        public void GrantedEffectIsLostOnceTheCreatureLeavesTheBattleZone()
        {
            CardWithGameProperties card = InGame(Creature(1000));
            card.grantedEffects.Add("Recyclable");

            card.resetProperties();

            Assert.AreEqual(DestroyedCardDestination.Graveyard, GameRules.GetDestroyedCardDestination(card));
        }
    }

    [TestClass]
    public class EffectLookupTests
    {
        [TestMethod]
        public void PrintedEffectIsFound()
        {
            Assert.IsTrue(GameRules.HasEffect(Creature(1000, "Fire", 1, Keyword("Defender")), "Defender"));
        }

        [TestMethod]
        public void MissingEffectIsNotFound()
        {
            Assert.IsFalse(GameRules.HasEffect(Creature(1000, "Fire", 1, Keyword("Defender")), "Slippery"));
        }

        [TestMethod]
        public void CardWithoutEffectsHasNone()
        {
            Assert.IsFalse(GameRules.HasEffect(Creature(1000), "Defender"));
        }

        [TestMethod]
        public void GrantedEffectIsFound()
        {
            CardWithGameProperties card = InGame(Creature(1000));
            card.grantedEffects.Add("Recyclable");

            Assert.IsTrue(GameRules.HasEffect(card, "Recyclable"));
        }

        [TestMethod]
        public void TriggerIsMatchedExactly()
        {
            Card volta = Creature(1000, "Air", 3, Triggered("SendTo", "Attack", "LessOrEqual", "AnyGround", "OwnersGrave", 23, 2000));

            Assert.IsTrue(GameRules.HasTrigger(volta, "Attack"));
            Assert.IsFalse(GameRules.HasTrigger(volta, "Summon"));
        }

        [TestMethod]
        public void KeywordsHaveNoTrigger()
        {
            Assert.IsFalse(GameRules.HasTrigger(Creature(1000, "Fire", 1, Keyword("Defender")), "Attack"));
        }
    }

    [TestClass]
    public class AttackEligibilityTests
    {
        [TestMethod]
        public void ReadyCreatureCanAttack()
        {
            Assert.IsTrue(GameRules.CanAttack(InGame(Creature(1000))));
        }

        [TestMethod]
        public void CreatureWithSummoningSicknessCannotAttack()
        {
            Assert.IsFalse(GameRules.CanAttack(InGame(Creature(1000), summoned: false)));
        }

        [TestMethod]
        public void CannotAttackCreatureNeverAttacks()
        {
            Assert.IsFalse(GameRules.CanAttack(InGame(Creature(3000, "Fire", 2, Keyword("Defender"), Keyword("CannotAttack")))));
        }

        [TestMethod]
        public void CreatureBreaksOneSafeguardByDefault()
        {
            Assert.AreEqual(1, GameRules.GetSafeguardBreakCount(Creature(1000)));
        }

        [TestMethod]
        public void SafeguardBreakerBreaksExtraSafeguards()
        {
            Assert.AreEqual(2, GameRules.GetSafeguardBreakCount(Creature(8000, "Fire", 9, Keyword("SafeguardBreaker", 1))));
        }

        [TestMethod]
        public void JugguroCannotAttackSafeguardsOrTheOpponent()
        {
            Card jugguro = Creature(7000, "Earth", 5, Keyword("Defender"), Keyword("OnlyAttacksCreatures"));

            Assert.AreEqual(0, GameRules.GetSafeguardBreakCount(jugguro));
            Assert.IsFalse(GameRules.CanAttackOpponent(jugguro));
            Assert.IsTrue(GameRules.CanAttack(InGame(jugguro)));
        }

        [TestMethod]
        public void OrdinaryCreatureCanAttackTheOpponent()
        {
            Assert.IsTrue(GameRules.CanAttackOpponent(Creature(1000)));
        }
    }

    [TestClass]
    public class BlockingTests
    {
        [TestMethod]
        public void ReadyDefenderCanBlock()
        {
            Assert.IsTrue(GameRules.CanBlock(InGame(Creature(3000, "Fire", 2, Keyword("Defender")))));
        }

        [TestMethod]
        public void EngagedDefenderCannotBlock()
        {
            Assert.IsFalse(GameRules.CanBlock(InGame(Creature(3000, "Fire", 2, Keyword("Defender")), engaged: true)));
        }

        [TestMethod]
        public void CreatureWithoutDefenderCannotBlock()
        {
            Assert.IsFalse(GameRules.CanBlock(InGame(Creature(3000))));
        }

        [TestMethod]
        public void SlipperyAttackerCannotBeBlocked()
        {
            Assert.IsFalse(GameRules.CanBeBlocked(Creature(3000, "Water", 4, Keyword("Slippery"))));
            Assert.IsTrue(GameRules.CanBeBlocked(Creature(3000)));
        }
    }

    [TestClass]
    public class EffectTargetTests
    {
        private static readonly SpecialEffect VoltaAttack = Triggered("SendTo", "Attack", "LessOrEqual", "AnyGround", "OwnersGrave", 23, 2000);

        [TestMethod]
        public void PowerCapIsInclusive()
        {
            Card source = Creature(4000);

            Assert.IsTrue(GameRules.IsValidEffectTarget(VoltaAttack, Creature(2000), source));
            Assert.IsFalse(GameRules.IsValidEffectTarget(VoltaAttack, Creature(2001), source));
            Assert.IsTrue(GameRules.IsValidEffectTarget(VoltaAttack, Creature(0), source));
        }

        [TestMethod]
        public void PowerCapOnlyMatchesCreatures()
        {
            Assert.IsFalse(GameRules.IsValidEffectTarget(VoltaAttack, Spell(), Creature(4000)));
        }

        [TestMethod]
        public void EffectOnAnyBattleZoneNeverHitsItsOwnSource()
        {
            Card volta = Creature(1000, "Air", 3, VoltaAttack);

            Assert.IsFalse(GameRules.IsValidEffectTarget(VoltaAttack, volta, volta));
        }

        [TestMethod]
        public void SingleZoneEffectCanTargetItsOwnSource()
        {
            // Glynrid: return a creature from your battle zone to your hand
            SpecialEffect bounce = Triggered("SendTo", "Summon", null, "OwnGround", "OwnHand", 1);
            Card glynrid = Creature(3000, "Water", 4, bounce);

            Assert.IsTrue(GameRules.IsValidEffectTarget(bounce, glynrid, glynrid));
        }

        [TestMethod]
        public void EffectWithoutConditionAcceptsAnyCard()
        {
            SpecialEffect returnFromGrave = Triggered("SendTo", "Summon", null, "OwnGrave", "OwnHand", 2);

            Assert.IsTrue(GameRules.IsValidEffectTarget(returnFromGrave, Spell(), Creature(2000)));
            Assert.IsTrue(GameRules.IsValidEffectTarget(returnFromGrave, Creature(9000), Creature(2000)));
        }
    }

    [TestClass]
    public class ManaPaymentTests
    {
        private static List<CardWithGameProperties> Mana(params string[] elements)
        {
            List<CardWithGameProperties> mana = new List<CardWithGameProperties>();

            foreach (string element in elements)
                mana.Add(InGame(Creature(1, element)));
            return mana;
        }

        [TestMethod]
        public void EnoughManaWithOneMatchingElementPays()
        {
            Assert.IsTrue(GameRules.CanPayCost(Creature(1000, "Fire", 3), Mana("Fire", "Water", "Water")));
        }

        [TestMethod]
        public void ManaWithoutTheCardsElementCannotPay()
        {
            Assert.IsFalse(GameRules.CanPayCost(Creature(1000, "Fire", 3), Mana("Water", "Water", "Air")));
        }

        [TestMethod]
        public void OneManaShortCannotPay()
        {
            Assert.IsFalse(GameRules.CanPayCost(Creature(1000, "Fire", 3), Mana("Fire", "Water")));
        }

        [TestMethod]
        public void EngagedManaDoesNotCount()
        {
            List<CardWithGameProperties> mana = Mana("Fire", "Water", "Water", "Water");
            mana[0].isEngaged = true;

            Assert.IsFalse(GameRules.CanPayCost(Creature(1000, "Fire", 3), mana));
        }

        [TestMethod]
        public void EmptyManaZoneCannotPay()
        {
            Assert.IsFalse(GameRules.CanPayCost(Creature(1000, "Fire", 1), Mana()));
        }

        [TestMethod]
        public void ExactlyEnoughManaPays()
        {
            Assert.IsTrue(GameRules.CanPayCost(Creature(1000, "Water", 1), Mana("Water")));
        }
    }
}
