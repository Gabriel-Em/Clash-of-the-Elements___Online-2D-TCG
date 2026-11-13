using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using DM___Server.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DM___Server.Tests
{
    // Reads a copy of the shipped Database.mdf through the server's own Database class.
    // Requires SQL Server Express LocalDB (the (LocalDB)\MSSQLLocalDB instance), like the server itself.
    [TestClass]
    [TestCategory("Integration")]
    public class CardDatabaseTests
    {
        private static readonly string[] Elements = { "Air", "Earth", "Ether", "Fire", "Water" };

        // every effect, trigger and zone route the client game engine implements
        private static readonly string[] SupportedEffects =
        {
            "SendTo", "Draw", "InstantSummon", "GrantRecyclable", "Defender", "Slippery", "CannotAttack", "OnlyAttacksCreatures",
            "SafeguardBreaker", "SafeguardCrusher", "BloodThirst", "BloodThirstRevenge", "Poisonous", "OnePunch",
            "Unkillable", "Recyclable", "ReturnAfterAttack"
        };
        private static readonly string[] SupportedTriggers = { null, "Summon", "Attack", "SafeguardActive" };
        private static readonly string[] SupportedSendToRoutes =
        {
            "OwnDeck>OwnMana", "OwnMana>OwnGrave", "OwnGround>OwnGrave", "OppGround>OppGrave", "OwnMana>OwnHand",
            "OwnGrave>OwnHand", "OwnGround>OwnHand", "OppMana>OppHand", "OppGround>OppHand", "AnyGround>OwnersHand",
            "OwnGrave>OwnGround", "AnyGround>OwnersGrave", "OppGround>OppMana"
        };

        private static string workingDirectory;
        private static string previousDirectory;
        private static List<Card> cards;
        private static Database database;

        private static string FindRepositoryRoot()
        {
            DirectoryInfo directory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

            while (directory != null && !File.Exists(Path.Combine(directory.FullName, "DM - Server", "DM - Server", "Database.mdf")))
                directory = directory.Parent;

            if (directory == null)
                throw new InvalidOperationException("Could not find DM - Server\\DM - Server\\Database.mdf above " + AppDomain.CurrentDomain.BaseDirectory);
            return directory.FullName;
        }

        [ClassInitialize]
        public static void LoadDatabaseCopy(TestContext context)
        {
            string source = Path.Combine(FindRepositoryRoot(), "DM - Server", "DM - Server");

            workingDirectory = Path.Combine(Path.GetTempPath(), "CardDatabaseTests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(workingDirectory);
            File.Copy(Path.Combine(source, "Database.mdf"), Path.Combine(workingDirectory, "Database.mdf"));
            File.Copy(Path.Combine(source, "Database_log.ldf"), Path.Combine(workingDirectory, "Database_log.ldf"));

            // the server opens Database.mdf from its current directory
            previousDirectory = Environment.CurrentDirectory;
            Environment.CurrentDirectory = workingDirectory;

            database = new Database();
            cards = database.cardCollectionToList();
        }

        [ClassCleanup]
        public static void DetachDatabaseCopy()
        {
            string mdfPath = Path.Combine(workingDirectory, "Database.mdf");

            SqlConnection.ClearAllPools();
            using (SqlConnection master = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;Integrated Security=True"))
            {
                master.Open();
                SqlCommand find = new SqlCommand("SELECT d.name FROM sys.databases d JOIN sys.master_files f ON f.database_id = d.database_id WHERE f.physical_name = @path", master);
                find.Parameters.AddWithValue("@path", mdfPath);
                object name = find.ExecuteScalar();

                if (name != null)
                {
                    string quoted = "[" + name.ToString().Replace("]", "]]") + "]";
                    new SqlCommand("ALTER DATABASE " + quoted + " SET SINGLE_USER WITH ROLLBACK IMMEDIATE", master).ExecuteNonQuery();
                    SqlCommand detach = new SqlCommand("EXEC sp_detach_db @name", master);
                    detach.Parameters.AddWithValue("@name", name.ToString());
                    detach.ExecuteNonQuery();
                }
            }

            Environment.CurrentDirectory = previousDirectory;
            Directory.Delete(workingDirectory, true);
        }

        private static Card CardNamed(string name)
        {
            Card card = cards.SingleOrDefault(x => x.Name == name);

            Assert.IsNotNull(card, name + " is missing from the card collection");
            return card;
        }

        [TestMethod]
        public void CollectionHasTheTwentySixCardsWithUniqueIDs()
        {
            Assert.AreEqual(26, cards.Count);
            CollectionAssert.AreEquivalent(Enumerable.Range(1, 26).ToList(), cards.Select(x => x.ID).ToList());
        }

        [TestMethod]
        public void EveryCardBelongsToOneOfTheFiveElements()
        {
            foreach (Card card in cards)
                CollectionAssert.Contains(Elements, card.Element, card.Name);

            CollectionAssert.AreEquivalent(Elements, cards.Select(x => x.Element).Distinct().ToArray());
        }

        [TestMethod]
        public void CreaturesHavePowerAndSpellsDoNot()
        {
            foreach (Card card in cards)
            {
                if (card.Type == "Creature")
                    Assert.IsTrue(card.Power > 0, card.Name);
                else
                    Assert.AreEqual("Spell", card.Type, card.Name);
            }
        }

        [TestMethod]
        public void EveryEffectIsImplementedByTheGameEngine()
        {
            foreach (Card card in cards)
            {
                foreach (SpecialEffect se in card.SpecialEffects)
                {
                    CollectionAssert.Contains(SupportedEffects, se.Effect, card.Name);
                    CollectionAssert.Contains(SupportedTriggers, se.Trigger, card.Name + " " + se.Effect);
                    Assert.IsNotNull(se.Arguments, card.Name + " " + se.Effect);

                    if (se.Effect == "SendTo")
                    {
                        CollectionAssert.Contains(SupportedSendToRoutes, se.TargetFrom + ">" + se.TargetTo, card.Name);
                        if (se.Condition == "LessOrEqual")
                            Assert.AreEqual(2, se.Arguments.Count, card.Name + " needs a count and a power cap");
                    }
                }
            }
        }

        [TestMethod]
        public void VoltaDestroysAllOtherSmallCreaturesWhenItAttacks()
        {
            SpecialEffect se = CardNamed("Volta").SpecialEffects.Single();

            Assert.AreEqual("Attack", se.Trigger);
            Assert.AreEqual("AnyGround", se.TargetFrom);
            Assert.AreEqual("OwnersGrave", se.TargetTo);
            Assert.AreEqual("LessOrEqual", se.Condition);
            CollectionAssert.AreEqual(new List<int>() { 23, 2000 }, se.Arguments);
        }

        [TestMethod]
        public void GulgurdDestroysAllSmallCreaturesWhenSummoned()
        {
            SpecialEffect se = CardNamed("Gulgurd").SpecialEffects.Single();

            Assert.AreEqual("Summon", se.Trigger);
            Assert.AreEqual("AnyGround", se.TargetFrom);
            Assert.AreEqual("OwnersGrave", se.TargetTo);
            Assert.AreEqual("LessOrEqual", se.Condition);
            CollectionAssert.AreEqual(new List<int>() { 23, 3000 }, se.Arguments);
        }

        [TestMethod]
        public void EveryCardHasAnImageInTheClient()
        {
            string images = Path.Combine(FindRepositoryRoot(), "DM - Client", "DM - Client", "Images", "Cards");

            foreach (Card card in cards)
                Assert.IsTrue(File.Exists(Path.Combine(images, card.Name + ".png")), card.Name);
        }

        [TestMethod]
        public void StoredDecksFollowTheDeckBuildingRules()
        {
            HashSet<int> cardIDs = new HashSet<int>(cards.Select(x => x.ID));

            foreach (int deckID in new[] { 1, 3, 7 })
            {
                string deck = database.getDeckByID(deckID);
                List<int[]> items = deck.Split(';').Select(x => x.Split('&').Select(int.Parse).ToArray()).ToList();

                Assert.IsTrue(items.Sum(x => x[1]) <= 40, "deck " + deckID + " has more than 40 cards");
                foreach (int[] item in items)
                {
                    Assert.IsTrue(cardIDs.Contains(item[0]), "deck " + deckID + " uses unknown card " + item[0]);
                    Assert.IsTrue(item[1] >= 1 && item[1] <= 4, "deck " + deckID + " has " + item[1] + " copies of card " + item[0]);
                }
            }
        }
    }
}
