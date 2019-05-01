using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DM___Client.GUIWindows
{
    /// <summary>
    /// Interaction logic for GUI.xaml
    /// </summary>
    public partial class GUI : Window
    {
        private Communication com;
        private DispatcherTimer loadPage = new DispatcherTimer();
        private GUIPages.GUILobbyRoom GUIlobbyRoom;
        private GUIPages.GUICollection GUIcollection;
        private GUIPages.GUILoading GUIloading;
        private GUIPages.GUIGameRoom GUIgameRoom;
        private GUIPages.GUIPreGameRoom GUIpreGameRoom;
        private int currentPageID;
        private Models.CardCollection CardCollection;
        public bool CardCollectionLoaded { get; set; }

        public GUI()
        {
            InitializeComponent();
            CardCollection = new Models.CardCollection();
            CardCollectionLoaded = false;
            loadPage.Interval = new TimeSpan(0, 0, 0, 1, 0);
            loadPage.Tick += LoadPage_Tick;
            com = new Communication();
            loadLogIn();
        }

        // timer that waits for pages to load their data (usually to recieve it from the server) before displaying the page

        private void LoadPage_Tick(object sender, EventArgs e)
        {
            switch (currentPageID)
            {
                case 1:
                    if (GUIlobbyRoom.DoneLoadingData())         // this is initially false and is being checked every second; When page data has been loaded this becomes true
                    {
                        loadPage.Stop();                    // stop the check that occures every second once the data was loaded
                        MainFrame.Content = GUIlobbyRoom;   // display the page
                        GUIloading = null;                  // you no longer need the loading GUI page
                    }
                    else
                        GUIloading.updateMessages(GUIlobbyRoom.getLoadedDataChecklist()); // update the checklist for loaded data
                    break;
                case 2:
                    if(GUIcollection.DoneLoading())
                    {
                        loadPage.Stop();
                        MainFrame.Content = GUIcollection;
                        GUIloading = null;
                    }
                    else
                        GUIloading.updateMessages(GUIcollection.getLoadedDataChecklist());
                    break;
                case 3:
                    if (GUIpreGameRoom.DoneLoading())
                    {
                        loadPage.Stop();
                        MainFrame.Content = GUIpreGameRoom;
                        GUIloading = null;
                    }
                    else
                        GUIloading.updateMessages(GUIpreGameRoom.getLoadedDataChecklist());
                    break;
                case 4:
                    if (GUIgameRoom.DoneLoading())
                    {
                        loadPage.Stop();
                        MainFrame.Content = GUIgameRoom;
                        GUIloading = null;
                        GUIgameRoom.startRolling();
                    }
                    else
                        GUIloading.updateMessages(GUIgameRoom.getLoadedDataChecklist());
                    break;
                default: break;
            }
        }

        // END GAME

        public void loadEndGame(bool youHaveWon)
        {
            GUIPages.GUIEndGame GUIendGame = new GUIPages.GUIEndGame(this, youHaveWon);
            MainFrame.Content = GUIendGame;
        }

        // GAME LOBBY

        public void loadGameLobby()
        {
            GUIlobbyRoom = new GUIPages.GUILobbyRoom(this, com);
            List<string> loadedDataChecklistTitles = new List<string>()
            {
                "Populating User List",
                "Fetching User Data",
                "Fetching Game Rooms",
                "Fetching Card Database"
            };

            GUIloading = new GUIPages.GUILoading(GUIlobbyRoom.BackgroundImageSource, loadedDataChecklistTitles, GUIlobbyRoom.getLoadedDataChecklist());
            MainFrame.Content = GUIloading;
            currentPageID = 1;
            loadPage.Start();
        }

        // COLLECTION

        public void loadCollection()
        {
            GUIcollection = new GUIPages.GUICollection(this, com, CardCollection);
            List<string> loadedDataChecklistTitles = new List<string>()
            {
                "Fetching Decks",
            };

            GUIloading = new GUIPages.GUILoading(GUIcollection.BackgroundImageSource, loadedDataChecklistTitles, GUIcollection.getLoadedDataChecklist());
            MainFrame.Content = GUIloading;
            currentPageID = 2;
            loadPage.Start();
        }

        // GAME ROOM

        public void loadGameRoom(int GameRoomID,int DeckID)
        {
            GUIgameRoom = new GUIPages.GUIGameRoom(this, com, GameRoomID, DeckID, CardCollection);
            List<string> loadedDataChecklistTitles = new List<string>()
            {
                "Setting up initial conditions",
                "Fetching hand",
                "Waiting for opponent"
            };
            GUIloading = new GUIPages.GUILoading(GUIgameRoom.BackgroundImageSource, loadedDataChecklistTitles, GUIgameRoom.getLoadedDataChecklist());
            MainFrame.Content = GUIloading;
            currentPageID = 4;
            loadPage.Start();
        }

        // PRE GAME ROOM

        internal void loadPreGameRoom(int GameRoomID)
        {
            GUIpreGameRoom = new GUIPages.GUIPreGameRoom(this, com, GameRoomID);
            List<string> loadedDataChecklistTitles = new List<string>()
            {
                "Fetching Decks"
            };
            GUIloading = new GUIPages.GUILoading(GUIpreGameRoom.BackgroundImageSource, loadedDataChecklistTitles, GUIpreGameRoom.getLoadedDataChecklist());
            MainFrame.Content = GUIloading;
            currentPageID = 3;
            loadPage.Start();
        }

        // LOG IN

        public void loadLogIn()
        {
            GUIPages.GUILogIn logIn = new GUIPages.GUILogIn(this, com); // create the LogIn GUI page
            MainFrame.Content = logIn;                                  // apply the page to current window
            logIn.tryConnect();                                         // try to connect to the server
        }

        public void changeStatus(string text)   // change the status on the title of the window
        {
            this.Title = "Clash of the Elements v1.0.0.0 | Status: " + text;
        }

        public void setCardCollection(List<Models.Card> cards)
        {
            CardCollection.setCards(cards);
            CardCollectionLoaded = true;
        }
    }
}