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
        private GUIPages.GUILobbyRoom lobbyRoom;
        private GUIPages.GUICollection collection;
        private GUIPages.GUILoading loading;
        private GUIPages.GUIGameRoom gameRoom;
        private GUIPages.GUIPreGameRoom preGameRoom;
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

        private void LoadPage_Tick(object sender, EventArgs e)
        {
            switch (currentPageID)
            {
                case 1:
                    if (lobbyRoom.DoneLoading())
                    {
                        loadPage.Stop();
                        MainFrame.Content = lobbyRoom;
                        loading = null;
                    }
                    else
                        loading.updateMessages(lobbyRoom.getLoadedData());
                    break;
                case 2:
                    if(collection.DoneLoading())
                    {
                        loadPage.Stop();
                        MainFrame.Content = collection;
                        loading = null;
                    }
                    else
                        loading.updateMessages(collection.getLoadedData());
                    break;
                case 3:
                    if (preGameRoom.DoneLoading())
                    {
                        loadPage.Stop();
                        MainFrame.Content = preGameRoom;
                        loading = null;
                    }
                    else
                        loading.updateMessages(preGameRoom.getLoadedData());
                    break;
                case 4:
                    if (gameRoom.DoneLoading())
                    {
                        loadPage.Stop();
                        MainFrame.Content = gameRoom;
                        gameRoom.startRolling();
                        loading = null;
                    }
                    else
                        loading.updateMessages(gameRoom.getLoadedData());
                    break;
                default: break;
            }
        }

        public void loadGameLobby()
        {
            lobbyRoom = new GUIPages.GUILobbyRoom(this, com);
            List<string> messages = new List<string>()
            {
                "Populating User List",
                "Fetching User Data",
                "Fetching Game Rooms",
                "Fetching Card Database"
            };

            loading = new GUIPages.GUILoading(lobbyRoom.BackgroundImageSource, messages, lobbyRoom.getLoadedData());
            MainFrame.Content = loading;
            currentPageID = 1;
            loadPage.Start();
        }

        public void loadCollection()
        {
            collection = new GUIPages.GUICollection(this, com, CardCollection);
            List<string> messages = new List<string>()
            {
                "Fetching Decks",
            };

            loading = new GUIPages.GUILoading(collection.BackgroundImageSource, messages, collection.getLoadedData());
            MainFrame.Content = loading;
            currentPageID = 2;
            loadPage.Start();
        }

        public void loadGameRoom(int GameRoomID,int DeckID)
        {
            gameRoom = new GUIPages.GUIGameRoom(this, com, GameRoomID, DeckID, CardCollection);
            List<string> messages = new List<string>()
            {
                "Setting up initial conditions",
                "Fetching hand",
                "Waiting for opponent"
            };
            loading = new GUIPages.GUILoading(gameRoom.BackgroundImageSource, messages, gameRoom.getLoadedData());
            MainFrame.Content = loading;
            currentPageID = 4;
            loadPage.Start();
        }

        internal void loadPreGameRoom(int GameRoomID)
        {
            preGameRoom = new GUIPages.GUIPreGameRoom(this, com, GameRoomID);
            List<string> messages = new List<string>()
            {
                "Fetching Decks"
            };
            loading = new GUIPages.GUILoading(preGameRoom.BackgroundImageSource, messages, preGameRoom.getLoadedData());
            MainFrame.Content = loading;
            currentPageID = 3;
            loadPage.Start();
        }

        public void loadLogIn()
        {
            GUIPages.GUILogIn logIn = new GUIPages.GUILogIn(this, com);
            MainFrame.Content = logIn;
            logIn.tryConnect();
        }

        public void changeStatus(string text)
        {
            this.Title = "Duel Masters v1.0.0.0 | Status: " + text;
        }

        public void setCardCollection(Models.CardCollection CardCollection_)
        {
            CardCollection = CardCollection_.Clone();
            CardCollectionLoaded = true;
        }
    }
}