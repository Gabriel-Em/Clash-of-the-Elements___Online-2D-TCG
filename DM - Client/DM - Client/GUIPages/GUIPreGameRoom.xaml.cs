using System;
using System.Collections.Generic;
using System.Linq;
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

namespace DM___Client.GUIPages
{
    /// <summary>
    /// Interaction logic for GUIPreGameRoom.xaml
    /// </summary>
    public partial class GUIPreGameRoom : Page
    {
        private GUIWindows.GUI parent;
        private Controllers.PreGameRoomController ctrl;
        private DispatcherTimer checkServerResponse = new DispatcherTimer();
        private List<Models.PreGameDeckGUIModel> DecksGUI;
        private int selectedDeckID;
        private int GameRoomID;
        public ImageSource BackgroundImageSource { get { return backgroundImage.Source; } }

        public GUIPreGameRoom(GUIWindows.GUI parent_, Communication com_, int GameRoomID_)
        {
            InitializeComponent();
            parent = parent_;
            GameRoomID = GameRoomID_;
            DecksGUI = new List<Models.PreGameDeckGUIModel>();
            ctrl = new Controllers.PreGameRoomController(this, com_);
            selectedDeckID = -1;
            checkServerResponse.Interval = new TimeSpan(0, 0, 0, 0, 100);
            checkServerResponse.Tick += checkServerResponse_Tick;
            ctrl.loadPageData();
            beginListening();
        }

        private void beginListening()
        {
            if (!ctrl.Listening)
                ctrl.beginListen();
            if (!checkServerResponse.IsEnabled)
                checkServerResponse.Start();
        }

        private void stopListening()
        {
            if (ctrl.Listening)
                ctrl.stopListen();
            if (checkServerResponse.IsEnabled)
                checkServerResponse.Stop();
        }

        private void checkServerResponse_Tick(object sender, EventArgs e)
        {
            if (ctrl.hasReceivedResponse())
                ctrl.messageProcessor(ctrl.getReceivedResponse());
        }

        public void disconnected(string message, int type)
        {
            stopListening();
            parent.changeStatus("Not connected!");
            GUIWindows.GUIDisconnected disconnectWindow = new GUIWindows.GUIDisconnected(message, type);
            disconnectWindow.ShowDialog();

            parent.loadLogIn();
        }

        public List<bool> getLoadedData()
        {
            return ctrl.getLoadedData();
        }

        public bool DoneLoading()
        {
            if (ctrl.getLoadedData().Contains(false))
                return false;
            return true;
        }

        public void loadDecksToGUI()
        {
            foreach (Models.PreGameDeckListElement deck in ctrl.Decks)
            {
                Models.PreGameDeckGUIModel pgdguim = new Models.PreGameDeckGUIModel(deck.ID, deck.DeckName, this);
                stackDecks.Children.Add(pgdguim.Border);
                DecksGUI.Add(pgdguim);
            }
        }

        public void SelectDeck(int ID)
        {
            selectedDeckID = ID;
            for (int i = 0; i < DecksGUI.Count; i++)
                DecksGUI[i].deselect();
        }

        private void btnSelectDeck_Click(object sender, RoutedEventArgs e)
        {
            if (selectedDeckID == -1)
                MessageBox.Show("You must select a deck from the list above first", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            else
            {
                stopListening();
                parent.loadGameRoom(GameRoomID, selectedDeckID);
            }
        }
    }
}