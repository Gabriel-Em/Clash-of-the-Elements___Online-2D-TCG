using System;
using System.Collections.Generic;
using DM___Server.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DM___Server.Tests
{
    [TestClass]
    public class SpecialEffectParsingTests
    {
        [TestMethod]
        public void CardWithoutEffectsHasAnEmptyList()
        {
            List<SpecialEffect> effects = Database.parseSpecialEffects("None", "-1", "None", "None", "None", "None");

            Assert.AreEqual(0, effects.Count);
        }

        [TestMethod]
        public void KeywordIsParsedWithNullFields()
        {
            List<SpecialEffect> effects = Database.parseSpecialEffects("Defender", "-1", "None", "None", "None", "None");

            Assert.AreEqual(1, effects.Count);
            Assert.AreEqual("Defender", effects[0].Effect);
            Assert.IsNull(effects[0].Trigger);
            Assert.IsNull(effects[0].Condition);
            Assert.IsNull(effects[0].TargetFrom);
            Assert.IsNull(effects[0].TargetTo);
            CollectionAssert.AreEqual(new List<int>() { -1 }, effects[0].Arguments);
        }

        [TestMethod]
        public void SeveralEffectsAreParsedInOrder()
        {
            // Baraq: destroy an opponent's creature with 3000 power or less when summoned and whenever it attacks
            List<SpecialEffect> effects = Database.parseSpecialEffects(
                "SendTo;SendTo", "1&3000;1&3000", "Summon;Attack", "LessOrEqual;LessOrEqual", "OppGround;OppGround", "OppGrave;OppGrave");

            Assert.AreEqual(2, effects.Count);
            Assert.AreEqual("Summon", effects[0].Trigger);
            Assert.AreEqual("Attack", effects[1].Trigger);
            CollectionAssert.AreEqual(new List<int>() { 1, 3000 }, effects[1].Arguments);
            Assert.AreEqual("OppGround", effects[1].TargetFrom);
            Assert.AreEqual("OppGrave", effects[1].TargetTo);
        }

        [TestMethod]
        public void MixedKeywordAndTriggeredEffectsKeepTheirOwnFields()
        {
            // Glynrid
            List<SpecialEffect> effects = Database.parseSpecialEffects(
                "Defender;SendTo;Draw", "-1;1;1", "None;Summon;Summon", "None;None;None", "None;OwnGround;None", "None;OwnHand;None");

            Assert.AreEqual(3, effects.Count);
            Assert.IsNull(effects[0].Trigger);
            Assert.AreEqual("OwnGround", effects[1].TargetFrom);
            Assert.IsNull(effects[2].TargetFrom);
            CollectionAssert.AreEqual(new List<int>() { 1 }, effects[2].Arguments);
        }

        [TestMethod]
        public void NegativeAndLargeArgumentsAreKept()
        {
            List<SpecialEffect> effects = Database.parseSpecialEffects("BloodThirst", "2147483647", "None", "None", "None", "None");

            CollectionAssert.AreEqual(new List<int>() { int.MaxValue }, effects[0].Arguments);
        }

        [TestMethod]
        public void NonNumericArgumentIsRejected()
        {
            Assert.ThrowsException<FormatException>(() => Database.parseSpecialEffects("Draw", "x", "Summon", "None", "None", "None"));
        }

        [TestMethod]
        public void MissingFieldForAnEffectIsRejected()
        {
            Assert.ThrowsException<IndexOutOfRangeException>(() => Database.parseSpecialEffects("Draw;Draw", "1;1", "Summon", "None;None", "None;None", "None;None"));
        }
    }
}
