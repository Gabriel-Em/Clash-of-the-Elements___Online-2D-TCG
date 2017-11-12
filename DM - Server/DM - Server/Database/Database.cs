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

        public Database()
        {
            string mdfPath = Path.Combine(Directory.GetCurrentDirectory(), "Database.mdf");
            con = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + mdfPath + ";Integrated Security=True");
            
        }

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
                    cmd.Parameters.AddWithValue("@gameswon_", 0);
                    cmd.Parameters.AddWithValue("@gameslost_", 0);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    if (!Directory.Exists(@".\Logs"))
                        Directory.CreateDirectory(@".\Logs");

                    StreamWriter file = new StreamWriter(@".\Logs\" + DateTime.Now.ToString("yyyy -dd-MM--HH-mm-ss") + Guid.NewGuid().ToString() + "_Crash_Log.txt");
                    file.Write(ex.ToString());
                    file.Close();
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
                    if (!Directory.Exists(@".\Logs"))
                        Directory.CreateDirectory(@".\Logs");

                    StreamWriter file = new StreamWriter(@".\Logs\" + DateTime.Now.ToString("yyyy -dd-MM--HH-mm-ss") + Guid.NewGuid().ToString() + "_Crash_Log.txt");
                    file.Write(ex.ToString());
                    file.Close();
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
                    if (!Directory.Exists(@".\Logs"))
                        Directory.CreateDirectory(@".\Logs");

                    StreamWriter file = new StreamWriter(@".\Logs\" + DateTime.Now.ToString("yyyy -dd-MM--HH-mm-ss") + Guid.NewGuid().ToString() + "_Crash_Log.txt");
                    file.Write(ex.ToString());
                    file.Close();
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
                    if (!Directory.Exists(@".\Logs"))
                        Directory.CreateDirectory(@".\Logs");

                    StreamWriter file = new StreamWriter(@".\Logs\" + DateTime.Now.ToString("yyyy -dd-MM--HH-mm-ss") + Guid.NewGuid().ToString() + "_Crash_Log.txt");
                    file.Write(ex.ToString());
                    file.Close();
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
                    if (!Directory.Exists(@".\Logs"))
                        Directory.CreateDirectory(@".\Logs");

                    StreamWriter file = new StreamWriter(@".\Logs\" + DateTime.Now.ToString("yyyy -dd-MM--HH-mm-ss") + Guid.NewGuid().ToString() + "_Crash_Log.txt");
                    file.Write(ex.ToString());
                    file.Close();
                }
                finally
                {
                    con.Close();
                }
            }
            return commandArguments;
        }

        public List<Models.Card> cardCollectionToList()
        {
            int ID;
            string Name;
            string Type;
            string Civilization;
            int Cost;
            string Race;
            string Text;
            int Power;
            int ManaNumber;
            string Set;
            List<Models.SpecialEffect> SpecialEffects;
            List<Models.Card> cards = new List<Models.Card>();

            lock (sqlLock)
            {
                try
                {
                    if (con.State == System.Data.ConnectionState.Closed)
                        con.Open();
                    SqlCommand cmd = new SqlCommand("SELECT * FROM CardCollection", con);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ID = Int32.Parse(reader["ID"].ToString());
                            Name = reader["Name"].ToString();
                            Type = reader["Type"].ToString();
                            Civilization = reader["Civilization"].ToString();
                            Cost = Int32.Parse(reader["Cost"].ToString());
                            if (reader["Race"].ToString() == string.Empty)
                                Race = null;
                            else
                                Race = reader["Race"].ToString();
                            if (reader["Text"].ToString() == string.Empty)
                                Text = null;
                            else
                                Text = reader["Text"].ToString();
                            if (reader["Power"].ToString() == string.Empty)
                                Power = -1;
                            else
                                Power = Int32.Parse(reader["Power"].ToString());
                            ManaNumber = Int32.Parse(reader["Mana Number"].ToString());
                            Set = reader["Set"].ToString();

                            if (reader["Special Effect"].ToString() == string.Empty)
                                SpecialEffects = null;
                            else
                            {
                                SpecialEffects = new List<Models.SpecialEffect>();
                                string[] effects = reader["Special Effect"].ToString().Split(';');
                                string[] arguments;
                                string[] targetFrom;
                                string[] targetTo;

                                if (reader["Effect Arguments"].ToString() == string.Empty)
                                    arguments = null;
                                else
                                    arguments = reader["Effect Arguments"].ToString().Split(';');

                                if (reader["Effect Target From"].ToString() == string.Empty)
                                    targetFrom = null;
                                else
                                    targetFrom = reader["Effect Target From"].ToString().Split(';');

                                if (reader["Effect Target To"].ToString() == string.Empty)
                                    targetTo = null;
                                else
                                    targetTo = reader["Effect Target To"].ToString().Split(';');

                                for (int i = 0; i < effects.Length; i++)
                                {
                                    string tTo;
                                    string tFrom;
                                    List<string> listOfArguments;

                                    if (targetTo != null && i < targetTo.Length)
                                        tTo = targetTo[i];
                                    else
                                        tTo = null;

                                    if (targetFrom != null && i < targetFrom.Length)
                                        tFrom = targetFrom[i];
                                    else
                                        tFrom = null;

                                    if (arguments != null && i < arguments.Length)
                                        listOfArguments = arguments[i].Split('&').ToList<string>();
                                    else
                                        listOfArguments = null;
                                    SpecialEffects.Add(new Models.SpecialEffect(effects[i], listOfArguments, tFrom, tTo));
                                }
                            }
                            cards.Add(new Models.Card(ID, Name, Type, Civilization, Cost, Race, Text, Power, ManaNumber, Set, SpecialEffects));
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (!Directory.Exists(@".\Logs"))
                        Directory.CreateDirectory(@".\Logs");

                    StreamWriter file = new StreamWriter(@".\Logs\" + DateTime.Now.ToString("yyyy -dd-MM--HH-mm-ss") + Guid.NewGuid().ToString() + "_Crash_Log.txt");
                    file.Write(ex.ToString());
                    file.Close();
                }
                finally
                {
                    con.Close();
                }
            }
            return cards;
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
                    if (!Directory.Exists(@".\Logs"))
                        Directory.CreateDirectory(@".\Logs");

                    StreamWriter file = new StreamWriter(@".\Logs\" + DateTime.Now.ToString("yyyy -dd-MM--HH-mm-ss") + Guid.NewGuid().ToString() + "_Crash_Log.txt");
                    file.Write(ex.ToString());
                    file.Close();
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
                    SqlCommand cmd = new SqlCommand("SELECT Id, DeckName FROM Decks WHERE Username = @username_", con);
                    cmd.Parameters.AddWithValue("@username_", username);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            decks.Add(String.Format("{0},{1}", reader["Id"], reader["DeckName"]));
                    }
                }
                catch (Exception ex)
                {
                    if (!Directory.Exists(@".\Logs"))
                        Directory.CreateDirectory(@".\Logs");

                    StreamWriter file = new StreamWriter(@".\Logs\" + DateTime.Now.ToString("yyyy -dd-MM--HH-mm-ss") + Guid.NewGuid().ToString() + "_Crash_Log.txt");
                    file.Write(ex.ToString());
                    file.Close();
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
                            decks.Add(String.Format("{0},{1},{2}", reader["Id"], reader["DeckName"], reader["CardList"].ToString()));
                    }
                }
                catch (Exception ex)
                {
                    if (!Directory.Exists(@".\Logs"))
                        Directory.CreateDirectory(@".\Logs");

                    StreamWriter file = new StreamWriter(@".\Logs\" + DateTime.Now.ToString("yyyy -dd-MM--HH-mm-ss") + Guid.NewGuid().ToString() + "_Crash_Log.txt");
                    file.Write(ex.ToString());
                    file.Close();
                }
                finally
                {
                    con.Close();
                }
            }
            return decks;
        }

        public bool userHasDecks(string username)
        {
            bool response = false;

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
                        if (reader.Read())
                            response = true;
                    }
                }
                catch (Exception ex)
                {
                    if (!Directory.Exists(@".\Logs"))
                        Directory.CreateDirectory(@".\Logs");

                    StreamWriter file = new StreamWriter(@".\Logs\" + DateTime.Now.ToString("yyyy -dd-MM--HH-mm-ss") + Guid.NewGuid().ToString() + "_Crash_Log.txt");
                    file.Write(ex.ToString());
                    file.Close();
                }
                finally
                {
                    con.Close();
                }
            }
            return response;
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
                    if (!Directory.Exists(@".\Logs"))
                        Directory.CreateDirectory(@".\Logs");

                    StreamWriter file = new StreamWriter(@".\Logs\" + DateTime.Now.ToString("yyyy -dd-MM--HH-mm-ss") + Guid.NewGuid().ToString() + "_Crash_Log.txt");
                    file.Write(ex.ToString());
                    file.Close();
                }
                finally
                {
                    con.Close();
                }
            }
            return response;
        }
    }
}
