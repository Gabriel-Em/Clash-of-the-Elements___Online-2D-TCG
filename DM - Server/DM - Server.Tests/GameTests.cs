using System;
using System.Collections.Generic;
using System.Linq;
using DM___Server.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DM___Server.Tests
{
    [TestClass]
    public class GameTests
    {
        // 40 cards: ten different cards, four copies each
        private const string FullDeck = "1&4;2&4;3&4;4&4;5&4;6&4;7&4;8&4;9&4;10&4";

        private static Game NewGame()
        {
            return new Game(1, null, null);
        }

        private static List<int> AllCardsOfPlayer1(Game game)
        {
            return game.listPlayer1Deck
                .Concat(game.listPlayer1Safeguards)
                .Concat(game.listPlayer1Hand)
                .Concat(game.listPlayer1ManaZone)
                .Concat(game.listPlayer1BattleGround)
                .Concat(game.listPlayer1Graveyard)
                .ToList();
        }

        private static List<int> Sorted(IEnumerable<int> cards)
        {
            List<int> sorted = cards.ToList();
            sorted.Sort();
            return sorted;
        }

        [TestMethod]
        public void SetupDealsFiveSafeguardsAndFiveCards()
        {
            Game game = NewGame();

            game.loadPlayer1InitialData(FullDeck);

            Assert.AreEqual(5, game.listPlayer1Safeguards.Count);
            Assert.AreEqual(5, game.listPlayer1Hand.Count);
            Assert.AreEqual(30, game.listPlayer1Deck.Count);
        }

        [TestMethod]
        public void SetupKeepsEveryCardOfTheDeck()
        {
            Game game = NewGame();
            List<int> expected = new List<int>();

            for (int id = 1; id <= 10; id++)
                expected.AddRange(Enumerable.Repeat(id, 4));

            game.loadPlayer1InitialData(FullDeck);

            CollectionAssert.AreEqual(expected, Sorted(AllCardsOfPlayer1(game)));
        }

        [TestMethod]
        public void SetupForPlayer2DoesNotTouchPlayer1()
        {
            Game game = NewGame();

            game.loadPlayer2InitialData(FullDeck);

            Assert.AreEqual(5, game.listPlayer2Safeguards.Count);
            Assert.AreEqual(5, game.listPlayer2Hand.Count);
            Assert.AreEqual(0, AllCardsOfPlayer1(game).Count);
        }

        [TestMethod]
        public void SetupWithASmallDeckDealsWhatItCan()
        {
            Game game = NewGame();

            game.loadPlayer1InitialData("1&4;2&3");

            Assert.AreEqual(5, game.listPlayer1Safeguards.Count);
            Assert.AreEqual(2, game.listPlayer1Hand.Count);
            Assert.AreEqual(0, game.listPlayer1Deck.Count);
        }

        [TestMethod]
        public void SetupWithExactlyFiveCardsLeavesAnEmptyHand()
        {
            Game game = NewGame();

            game.loadPlayer1InitialData("3&5");

            Assert.AreEqual(5, game.listPlayer1Safeguards.Count);
            Assert.AreEqual(0, game.listPlayer1Hand.Count);
        }

        [TestMethod]
        public void MalformedDeckListIsRejected()
        {
            Assert.ThrowsException<FormatException>(() => NewGame().loadPlayer1InitialData(""));
            Assert.ThrowsException<FormatException>(() => NewGame().loadPlayer1InitialData("1&x"));
        }

        [TestMethod]
        public void DrawingMovesTheTopCardToTheHand()
        {
            Game game = NewGame();
            game.listPlayer1Deck.AddRange(new[] { 11, 12 });

            int drawn = game.drawCard(1);

            Assert.AreEqual(11, drawn);
            CollectionAssert.AreEqual(new List<int>() { 12 }, game.listPlayer1Deck);
            CollectionAssert.AreEqual(new List<int>() { 11 }, game.listPlayer1Hand);
        }

        [TestMethod]
        public void DrawingFromAnEmptyDeckReturnsNoCard()
        {
            Game game = NewGame();

            Assert.AreEqual(-1, game.drawCard(1));
            Assert.AreEqual(-1, game.drawCard(2));
            Assert.AreEqual(0, game.listPlayer1Hand.Count);
            Assert.AreEqual(0, game.listPlayer2Hand.Count);
        }

        [TestMethod]
        public void SummonMovesTheCardToTheSummonersBattleZone()
        {
            Game game = NewGame();
            game.listPlayer1Hand.Add(5);
            game.listPlayer2Hand.Add(6);

            game.summon(5, 1);
            game.summon(6, 2);

            CollectionAssert.AreEqual(new List<int>() { 5 }, game.listPlayer1BattleGround);
            CollectionAssert.AreEqual(new List<int>() { 6 }, game.listPlayer2BattleGround);
            Assert.AreEqual(0, game.listPlayer1Hand.Count + game.listPlayer2Hand.Count);
            Assert.AreEqual(0, game.listPlayer1Safeguards.Count + game.listPlayer2Safeguards.Count);
        }

        [TestMethod]
        public void PlayingAsManaMovesOneCopyFromHandToMana()
        {
            Game game = NewGame();
            game.listPlayer1Hand.AddRange(new[] { 5, 5 });

            game.playAsMana(5, 1);

            CollectionAssert.AreEqual(new List<int>() { 5 }, game.listPlayer1Hand);
            CollectionAssert.AreEqual(new List<int>() { 5 }, game.listPlayer1ManaZone);
        }

        [TestMethod]
        public void BrokenSafeguardGoesToItsOwnersHand()
        {
            Game game = NewGame();
            game.listPlayer2Safeguards.AddRange(new[] { 21, 22, 23 });

            int revealed = game.breakSafeguard(1, 2);

            Assert.AreEqual(22, revealed);
            CollectionAssert.AreEqual(new List<int>() { 21, 23 }, game.listPlayer2Safeguards);
            CollectionAssert.AreEqual(new List<int>() { 22 }, game.listPlayer2Hand);
        }

        [TestMethod]
        public void BreakingASafeguardThatDoesNotExistFails()
        {
            Game game = NewGame();

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => game.breakSafeguard(0, 1));
        }
    }
}
