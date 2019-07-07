using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DM___Server
{
    public class Database
    {
        private SqlConnection con;
        private object sqlLock = new object();
        private Log.Logger logger;

        public Database()
        {
            logger = new Log.Logger();
            string mdfPath = Path.Combine(Directory.GetCurrentDirectory(), "Database.mdf");
            con = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + mdfPath + ";Integrated Security=True");
        }

        #region Fetching card collection from DB to memory

        public List<Models.Card> cardCollectionToList()
        {
            int ID;
            string Name;
            string Set;
            string Type;
            string Element;
            int Cost;
            string Race;
            string Text;
            int Power;
            List<Models.SpecialEffect> SpecialEffects;
            List<Models.Card> cards = new List<Models.Card>();

            try
            {
                if (con.State == System.Data.ConnectionState.Closed)
                    con.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM CardCollection", con);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // get card data from db

                        ID = Int32.Parse(reader["ID"].ToString());
                        Name = reader["Name"].ToString();
                        Set = reader["Set"].ToString();
                        Type = reader["Type"].ToString();
                        Element = reader["Element"].ToString();
                        Cost = Int32.Parse(reader["Cost"].ToString());
                        Race = reader["Race"].ToString() != string.Empty ? reader["Race"].ToString() : null;
                        Text = reader["Text"].ToString() != string.Empty ? reader["Text"].ToString() : null;
                        Power = reader["Power"].ToString() != string.Empty ? Int32.Parse(reader["Power"].ToString()) : -1;

                        // get card SpecialEffect from db
                        if (reader["Special Effect"].ToString() == string.Empty)
                            SpecialEffects = null;
                        else
                        {
                            SpecialEffects = parseDBSpecialEffects(reader);
                        }
                        cards.Add(new Models.Card(ID, Name, Set, Type, Element, Cost, Race, Text, Power, SpecialEffects));
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log(ex.ToString());
            }
            finally
            {
                con.Close();
            }
            return cards;
        }

        private List<Models.SpecialEffect> parseDBSpecialEffects(SqlDataReader reader)
        {
            List<Models.SpecialEffect> SpecialEffects = new List<Models.SpecialEffect>();

            string[] effects = reader["Special Effect"].ToString().Split(';');
            string[] rawArguments = reader["Effect Arguments"].ToString().Split(';');
            string[] triggers = reader["Trigger"].ToString().Split(';');
            string[] conditions = reader["Condition"].ToString().Split(';');
            string[] targetFrom = reader["Effect Target From"].ToString().Split(';');
            string[] targetTo = reader["Effect Target To"].ToString().Split(';');

            if (effects[0] != "None")
            {
                for (int i = 0; i < effects.Length; i++)
                {
                    string[] strArgs = rawArguments[i].Split('&');
                    List<int> arguments = new List<int>();

                    foreach (string strArg in strArgs)
                    {
                        arguments.Add(Int32.Parse(strArg));
                    }

                    SpecialEffects.Add(
                        new Models.SpecialEffect(
                            effects[i],
                            triggers[i] != "None" ? triggers[i] : null,
                            conditions[i] != "None" ? conditions[i] : null,
                            arguments.Count != 0 ? arguments : null,
                            targetFrom[i] != "None" ? targetFrom[i] : null,
                            targetTo[i] != "None" ? targetTo[i] : null
                            )
                            );
                }
            }

            return SpecialEffects;
        }
        #endregion

        public void insertNewUser(string username, string nickname, string password, string email)
        {
            lock (sqlLock)
            {
                try
                {
                    if (con.State == System.Data.ConnectionState.Closed)
                        con.Open();
                    SqlCommand cmd = new SqlCommand("INSERT INTO Users(Username,Password,Email,NickName) VALUES (@username_,@password_,@email_,@nickname_)", con);
                    cmd.Parameters.AddWithValue("@username_", username);
                    cmd.Parameters.AddWithValue("@password_", password);
                    cmd.Parameters.AddWithValue("@email_", email);
                    cmd.Parameters.AddWithValue("@nickname_", nickname);
                    cmd.ExecuteNonQuery();

                    cmd = new SqlCommand("INSERT INTO UserData(GamesWon,GamesLost,Username) VALUES (@gameswon_,@gameslost_,@username_)", con);
                    cmd.Parameters.AddWithValue("@username_", username);
                    cmd.Parameters.AddWithValue("@gameswon_", 0);
                    cmd.Parameters.AddWithValue("@gameslost_", 0);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    logger.Log(ex.Message);
                }
                finally
                {
                    con.Close();
                }
            }
        }

        public bool checkLogInCredentials(string username, string password)
        {
            bool responseMessage = false;

            lock (sqlLock)
            {
                try
                {
                    if (con.State == System.Data.ConnectionState.Closed)
                        con.Open();
                    SqlCommand cmd = new SqlCommand("SELECT Password FROM Users WHERE Username = @username_", con);
                    cmd.Parameters.AddWithValue("@username_", username);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                            if (password == String.Format("{0}", reader["Password"]))
                                responseMessage = true;
                    }
                }
                catch (Exception ex)
                {
                    logger.Log(ex.Message);
                }
                finally
                {
                    con.Close();
                }
            }
            return responseMessage;
        }

        public Tuple<bool,List<string>> checkForRegisterInformation(string username, string nickname, string email)
        {
            bool responseMessage = true;
            List<string> invalidEntries = new List<string>();

            lock (sqlLock)
            {
                try
                {
                    if (con.State == System.Data.ConnectionState.Closed)
                        con.Open();
                    SqlCommand cmd = new SqlCommand("SELECT Username FROM Users WHERE Username = @username_", con);
                    cmd.Parameters.AddWithValue("@username_", username);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            responseMessage = false;
                            invalidEntries.Add("Username");
                        }
                    }
                    cmd = new SqlCommand("SELECT NickName FROM Users WHERE NickName = @nickname_", con);
                    cmd.Parameters.AddWithValue("@nickname_", nickname);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            responseMessage = false;
                            invalidEntries.Add("NickName");
                        }
                    }
                    cmd = new SqlCommand("SELECT Email FROM Users WHERE Email = @email_", con);
                    cmd.Parameters.AddWithValue("@email_", email);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            responseMessage = false;
                            invalidEntries.Add("Email");
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Log(ex.Message);
                }
                finally
                {
                    con.Close();
                }
            }
            return new Tuple<bool, List<string>>(responseMessage, invalidEntries);
        }

        public string getNickNameOfUser(string username)
        {
            string responseMessage = "Unknown";

            lock (sqlLock)
            {
                try
                {
                    if (con.State == System.Data.ConnectionState.Closed)
                        con.Open();
                    SqlCommand cmd = new SqlCommand("SELECT NickName FROM Users WHERE Username = @username_", con);
                    cmd.Parameters.AddWithValue("@username_", username);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                            responseMessage = String.Format("{0}", reader["NickName"]);
                    }
                }
                catch (Exception ex)
                {
                    logger.Log(ex.Message);
                }
                finally
                {
                    con.Close();
                }
            }
            return responseMessage;
        }

        public List<string> fetchUserData(string username)
        {
            List<string> commandArguments = new List<string>();

            lock (sqlLock)
            {
                try
                {
                    if (con.State == System.Data.ConnectionState.Closed)
                        con.Open();
                    SqlCommand cmd = new SqlCommand("SELECT NickName, Email, GamesWon, GamesLost FROM Users INNER JOIN UserData ON Users.Username = UserData.Username WHERE Users.Username = @username_", con);
                    cmd.Parameters.AddWithValue("@username_", username);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            commandArguments.Add(String.Format("{0}", reader["NickName"]));
                            commandArguments.Add(String.Format("{0}", reader["Email"]));
                            commandArguments.Add(String.Format("{0}", reader["GamesWon"]));
                            commandArguments.Add(String.Format("{0}", reader["GamesLost"]));
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Log(ex.Message);
                }
                finally
                {
                    con.Close();
                }
            }
            commandArguments.Add(username);
            return commandArguments;
        }

        public void removeDeck(int ID)
        {
            lock (sqlLock)
            {
                try
                {
                    if (con.State == System.Data.ConnectionState.Closed)
                        con.Open();
                    SqlCommand cmd = new SqlCommand("Delete FROM Decks WHERE Id = @ID_", con);
                    cmd.Parameters.AddWithValue("@ID_", ID);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    logger.Log(ex.Message);
                }
                finally
                {
                    con.Close();
                }
            }
        }

        internal List<string> fetchUserDeckList(string username)
        {
            List<string> decks = new List<string>();

            lock (sqlLock)
            {
                try
                {
                    if (con.State == System.Data.ConnectionState.Closed)
                        con.Open();
                    SqlCommand cmd = new SqlCommand("SELECT Id, DeckName, CardList FROM Decks WHERE Username = @username_", con);
                    cmd.Parameters.AddWithValue("@username_", username);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (reader["CardList"].ToString() != string.Empty && getDeckCardCount(reader["CardList"].ToString()) == 40)
                            {
                                decks.Add(String.Format("{0},{1}", reader["Id"], reader["DeckName"]));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Log(ex.Message);
                }
                finally
                {
                    con.Close();
                }
            }
            return decks;
        }

        public List<string> fetchUserDecks(string username)
        {
            List<string> decks = new List<string>();

            lock (sqlLock)
            {
                try
                {
                    if (con.State == System.Data.ConnectionState.Closed)
                        con.Open();
                    SqlCommand cmd = new SqlCommand("SELECT Id, DeckName, CardList FROM Decks WHERE Username = @username_", con);
                    cmd.Parameters.AddWithValue("@username_", username);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (reader["CardList"].ToString() == string.Empty)
                                decks.Add(String.Format("{0}`{1}", reader["Id"], reader["DeckName"]));
                            else
                                decks.Add(String.Format("{0}`{1}`{2}", reader["Id"], reader["DeckName"], reader["CardList"].ToString()));
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Log(ex.Message);
                }
                finally
                {
                    con.Close();
                }
            }
            return decks;
        }

        public bool userHasPlayableDecks(string username)
        {
            lock (sqlLock)
            {
                try
                {
                    if (con.State == System.Data.ConnectionState.Closed)
                        con.Open();
                    SqlCommand cmd = new SqlCommand("SELECT * FROM Decks WHERE Username = @username_", con);
                    cmd.Parameters.AddWithValue("@username_", username);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (getDeckCardCount(reader["CardList"].ToString()) == 40)
                                return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Log(ex.Message);
                }
                finally
                {
                    con.Close();
                }
            }
            return false;
        }

        private int getDeckCardCount(string deck)
        {
            string[] cards;
            int count;

            cards = deck.Split(';');
            count = 0;

            foreach(string card in cards)
            {
                count += Int32.Parse(card.Split('&')[1]);
            }

            return count;
        }

        public string getDeckByID(int ID)
        {
            string response = "";

            lock (sqlLock)
            {
                try
                {
                    if (con.State == System.Data.ConnectionState.Closed)
                        con.Open();
                    SqlCommand cmd = new SqlCommand("SELECT * FROM Decks WHERE Id = @ID_", con);
                    cmd.Parameters.AddWithValue("@ID_", ID);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                            response = reader["CardList"].ToString();
                    }
                }
                catch (Exception ex)
                {
                    logger.Log(ex.Message);
                }
                finally
                {
                    con.Close();
                }
            }
            return response;
        }

        public void updateUserData(string victorUsername, string defeatedUsername)
        {
            lock (sqlLock)
            {
                try
                {
                    if (con.State == System.Data.ConnectionState.Closed)
                        con.Open();
                    SqlCommand cmd = new SqlCommand("UPDATE UserData SET GamesWon = (SELECT GamesWon from UserData where Username = @victor) + 1 WHERE Username = @victor", con);
                    cmd.Parameters.AddWithValue("@victor", victorUsername);
                    cmd.ExecuteNonQuery();

                    cmd = new SqlCommand("UPDATE UserData SET GamesLost = (SELECT GamesLost from UserData where Username = @defeated) + 1 WHERE Username = @defeated", con);
                    cmd.Parameters.AddWithValue("@defeated", defeatedUsername);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    logger.Log(ex.Message);
                }
                finally
                {
                    con.Close();
                }
            }
        }

        public int createDeck(string username, string deckname)
        {
            lock (sqlLock)
            {
                try
                {
                    if (con.State == System.Data.ConnectionState.Closed)
                        con.Open();
                    SqlCommand cmd = new SqlCommand("INSERT into Decks (DeckName,Username) values (@deckname, @username) SELECT SCOPE_IDENTITY()", con);
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@deckname", deckname);

                    return Int32.Parse(cmd.ExecuteScalar().ToString());
                }
                catch (Exception ex)
                {
                    logger.Log(ex.ToString());
                }
                finally
                {
                    con.Close();
                }
                return -1;
            }
        }

        public void updateDeck(int deckID, string cardlist)
        {
            lock (sqlLock)
            {
                try
                {
                    if (con.State == System.Data.ConnectionState.Closed)
                        con.Open();
                    SqlCommand cmd = new SqlCommand("Update Decks Set CardList = @cardlist where Id = @id", con);
                    cmd.Parameters.AddWithValue("@id", deckID);
                    cmd.Parameters.AddWithValue("@cardlist", cardlist);

                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    logger.Log(ex.ToString());
                }
                finally
                {
                    con.Close();
                }
            }
        }
    }
}
