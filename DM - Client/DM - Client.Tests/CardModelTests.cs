using System.Collections.Generic;
using DM___Client.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static DM___Client.Tests.CardFactory;

namespace DM___Client.Tests
{
    [TestClass]
    public class CardWithGamePropertiesTests
    {
        [TestMethod]
        public void NewCardInPlayIsReadyToBeSummonedWithNoGrantedEffects()
        {
            CardWithGameProperties card = new CardWithGameProperties(Creature(1000));

            Assert.IsFalse(card.isEngaged);
            Assert.IsFalse(card.hasCompletelyBeenSummoned);
            Assert.AreEqual(0, card.grantedEffects.Count);
        }

        [TestMethod]
        public void ResetClearsEveryPerGameProperty()
        {
            CardWithGameProperties card = InGame(Creature(1000), engaged: true, summoned: true);
            card.grantedEffects.Add("Recyclable");

            card.resetProperties();

            Assert.IsFalse(card.isEngaged);
            Assert.IsFalse(card.hasCompletelyBeenSummoned);
            Assert.AreEqual(0, card.grantedEffects.Count);
        }

        [TestMethod]
        public void ResetIsIdempotent()
        {
            CardWithGameProperties card = InGame(Creature(1000), engaged: true);

            card.resetProperties();
            card.resetProperties();

            Assert.IsFalse(card.isEngaged);
            Assert.AreEqual(0, card.grantedEffects.Count);
        }

        [TestMethod]
        public void GrantedEffectsAreNotSharedBetweenCopiesOfTheSameCard()
        {
            Card printed = Creature(1000);
            CardWithGameProperties first = new CardWithGameProperties(printed);
            CardWithGameProperties second = new CardWithGameProperties(printed);

            first.grantedEffects.Add("Recyclable");

            Assert.AreEqual(0, second.grantedEffects.Count);
        }

        [TestMethod]
        public void CopyDoesNotChangeThePrintedCardsEffects()
        {
            Card printed = Creature(1000, "Fire", 1, Keyword("Defender"));
            CardWithGameProperties copy = new CardWithGameProperties(printed);

            copy.SpecialEffects.Add(Keyword("Slippery"));

            Assert.AreEqual(1, printed.SpecialEffects.Count);
        }
    }

    [TestClass]
    public class CollectionDeckTests
    {
        private static Card CardWithID(int id)
        {
            return new Card(id, "Card " + id, "", "Creature", "Fire", 1, "Race", "", 1000, new List<SpecialEffect>());
        }

        [TestMethod]
        public void NewDeckCountsEveryCopy()
        {
            CollectionDeck deck = new CollectionDeck(1, "Deck", new List<CollectionDeckItem>()
            {
                new CollectionDeckItem(CardWithID(1), 4),
                new CollectionDeckItem(CardWithID(2), 2)
            });

            Assert.AreEqual(6, deck.Count);
        }

        [TestMethod]
        public void EmptyDeckHasNoCards()
        {
            Assert.AreEqual(0, new CollectionDeck(1, "Deck", new List<CollectionDeckItem>()).Count);
        }

        [TestMethod]
        public void UpToFourCopiesOfACardCanBeAdded()
        {
            CollectionDeck deck = new CollectionDeck(1, "Deck", new List<CollectionDeckItem>());

            for (int i = 0; i < 4; i++)
                Assert.IsTrue(deck.addCard(CardWithID(7)), "copy " + (i + 1));

            Assert.AreEqual(4, deck.Count);
        }

        [TestMethod]
        public void FifthCopyIsRejected()
        {
            CollectionDeck deck = new CollectionDeck(1, "Deck", new List<CollectionDeckItem>() { new CollectionDeckItem(CardWithID(7), 4) });

            Assert.IsFalse(deck.addCard(CardWithID(7)));
            Assert.AreEqual(4, deck.Count);
        }

        [TestMethod]
        public void CopyLimitIsPerCard()
        {
            CollectionDeck deck = new CollectionDeck(1, "Deck", new List<CollectionDeckItem>() { new CollectionDeckItem(CardWithID(7), 4) });

            Assert.IsTrue(deck.addCard(CardWithID(8)));
            Assert.AreEqual(5, deck.Count);
        }
    }
}
