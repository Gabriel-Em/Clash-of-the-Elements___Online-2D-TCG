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
    /// Interaction logic for GUILobbyRoom.xaml
    /// </summary>
    public partial class GUILobbyRoom : Page
    {
        // perent Window
        private GUIWindows.GUI parent;

        // attached controller
        private Controllers.LobbyRoomController ctrl;

        // mechanism that checks for server commands and responses
        private DispatcherTimer checkServerResponse = new DispatcherTimer();

        // a list for all GUI's belonging to game rooms
        private List<Models.GameRoomGUIModel> listGameRoomsGUI;

        // utility boolean - helps the GUI know when to start messages with new line or not
        private bool chatWindowEmpty;

        // information requested by the parent window
        public ImageSource BackgroundImageSource { get { return backgroundImage.Source; } }
        public bool CardCollectionLoaded { get { return parent.CardCollectionLoaded; } }

        public GUILobbyRoom(GUIWindows.GUI parent_, Communication com_)
        {
            InitializeComponent();

            // attach parent and controller
            parent = parent_;
            ctrl = new Controllers.LobbyRoomController(this, com_);

            // initialize unitialized data

            chatWindowEmpty = true;
            listGameRoomsGUI = new List<Models.GameRoomGUIModel>();
            
            // initialize the timer responsible with checking for messages from the server
            checkServerResponse.Interval = new TimeSpan(0, 0, 0, 0, 100);
            checkServerResponse.Tick += checkServerResponse_Tick;

            // start loading page data
            ctrl.loadPageData();

            // start listening
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

        // what happens during a disconnect event
        public void disconnected(string message, int type)
        {
            stopListening();
            parent.changeStatus("Not connected!");
            GUIWindows.GUIDisconnected disconnectWindow = new GUIWindows.GUIDisconnected(message, type);
            disconnectWindow.ShowDialog();
            
            parent.loadLogIn();
        }

        // adds a new user that joined the lobby to GUI
        public void addLobbyRoomUsers(List<string> Users)
        {
            foreach(string user in Users)
            {
                ListBoxItem newItem = new ListBoxItem();
                newItem.Content = user;
                newItem.FontSize = 15;
                newItem.FontWeight = FontWeights.Bold;
                newItem.Foreground = Brushes.Black;
                newItem.Cursor = Cursors.Hand;
                newItem.Margin = new Thickness(0, 0, 0, 3);
                listBoxUsers.Items.Add(newItem);
            }
            listBoxUsers.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("Content", System.ComponentModel.ListSortDirection.Ascending));
        }

        // removes a user that left the lobby from GUI
        public void removeLobbyRoomUser(string user)
        {
            for (int i = 0; i < listBoxUsers.Items.Count; i++)
                if (((ListBoxItem)listBoxUsers.Items[i]).Content.ToString() == user)
                {
                    listBoxUsers.Items.RemoveAt(i);
                    break;
                }
        }

        // updates the lable that tells you who you're logged in as
        public void updateLoggedInAs(string nickName)
        {
            lblLoggedInAs.Content = nickName;
        }

        // attempts to send a chat message
        private void btnSubmitText_Click(object sender, RoutedEventArgs e)
        {
            sendChatMessage();
        }

        // executes if you're attempting to join a game room while you have no decks in your collection
        public void noRoomsForMe()
        {
            MessageBox.Show("You need to have at least 1 deck in your collection before interacting with game rooms.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        // executes when a match is about to start
        public void joinPreGameRoom(int RoomID)
        {
            stopListening();
            ctrl.send(new Models.ClientMessage("LEAVELOBBY", new List<string>() { "PreGameRoom" }));
            parent.loadPreGameRoom(RoomID);
        }

        private void sendChatMessage()
        {
            string message = txtTypeInChat.Text;
            message = message.Replace("\r", "");
            message = message.Replace("\n", "");

            if (ctrl.notEmpty(message))
            {
                List<string> commandArguments = new List<string>() { ctrl.userData.NickName, message };
                ctrl.send(new Models.ClientMessage("NEWCHATMESSAGE", commandArguments));
            }
            else
                MessageBox.Show("The message can't be empty.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

            txtTypeInChat.Clear();
        }

        // updates the rich text box with a new received text message
        public void newChatMessage(string nickName, string message)
        {
            TextRange tr = new TextRange(richTextboxChat.Document.ContentEnd,richTextboxChat.Document.ContentEnd);
            if (!chatWindowEmpty)
                tr.Text = "\n";
            chatWindowEmpty = false;
            tr.Text += nickName + ": ";
            tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
            tr = new TextRange(richTextboxChat.Document.ContentEnd, richTextboxChat.Document.ContentEnd);
            tr.Text = message;
            tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);
            richTextboxChat.ScrollToEnd();
        }

        // checks if the Enter key was pressed and if so, sends written message to server
        private void TextBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != System.Windows.Input.Key.Enter)
                return;
            sendChatMessage();
        }

        // executes when you request to visit your collection
        private void btnDecks_Click(object sender, RoutedEventArgs e)
        {
            stopListening();
            ctrl.send(new Models.ClientMessage("LEAVELOBBY", new List<string>() { "Collection" }));
            parent.loadCollection();
        }

        // used by parent GUI to transfer the list of cards received from the server after joining the lobby
        public void setCardCollection(List<Models.Card> cards)
        {
            parent.setCardCollection(cards);
        }

        // retrieves the list check list for which data needs to load/was already loaded
        public List<bool> getLoadedDataChecklist()
        {
            return ctrl.getLoadedDataChecklist();
        }

        // checks if all data that needed to load has loaded
        public bool DoneLoadingData()
        {
            if (ctrl.getLoadedDataChecklist().Contains(false))
                return false;
            return true;
        }

        // quits the entire application
        private void btnQuit_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to quit?", "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                System.Windows.Application.Current.Shutdown();
        }

        // sends a request to create a new lobby room to the server
        private void btnCreateLobbyRoom_Click(object sender, RoutedEventArgs e)
        {
            ctrl.send(new Models.ClientMessage("CREATEGAMEROOM"));
        }

        // adds a new game room to GUI
        public void AddRoomToGUI(Models.GameRoom room)
        {
            Models.GameRoomGUIModel grguim = new Models.GameRoomGUIModel(room, ctrl.userData.NickName, this);
            listGameRoomsGUI.Add(grguim);
            stackRooms.Children.Add(grguim.Border);
        }

        // removes a game room from GUI
        public void RemoveRoomFromGUI(int id)
        {
            for(int i =0;i<listGameRoomsGUI.Count;i++)
            {
                if(listGameRoomsGUI[i].ID == id)
                {
                    stackRooms.Children.Remove(listGameRoomsGUI[i].Border);
                    listGameRoomsGUI.RemoveAt(i);
                    break;
                }
            }
        }

        // sends a request to close a game room
        public void closeRoom(int roomID)
        {
            ctrl.send(new Models.ClientMessage("CLOSEROOM", new List<string>() { roomID.ToString() }));
        }

        // sends a request to join a game room
        public void joinRoom(int roomID)
        {
            ctrl.send(new Models.ClientMessage("JOINROOM", new List<string>() { roomID.ToString() }));
        }

        // sends a request to leave a game room
        public void leaveRoom(int roomID)
        {
            ctrl.send(new Models.ClientMessage("LEAVEROOM", new List<string>() { roomID.ToString() }));
        }

        // sends a ready notification to the server
        public void sendReadyNotification(int roomID)
        {
            ctrl.send(new Models.ClientMessage("SETREADY", new List<string>() { roomID.ToString() }));
        }

        private Models.GameRoomGUIModel getRoomByID(int roomID)
        {
            foreach (Models.GameRoomGUIModel grgui in listGameRoomsGUI)
            {
                if (grgui.ID == roomID)
                    return grgui;
            }
            return null;
        }

        // updates the GUI of a game room when a user has joined it
        public void playerJoinedRoom(int roomID, string nickName)
        {
            Models.GameRoomGUIModel gameRoomGUI;

            gameRoomGUI = getRoomByID(roomID);

            if (gameRoomGUI != null)
            {
                if (ctrl.userData.NickName == nickName)
                    gameRoomGUI.attachPlayerToGameRoom(nickName, 1); // you joined a room
                else
                    gameRoomGUI.attachPlayerToGameRoom(nickName, 2); // someone else joined a room
                gameRoomGUI.setState("Waiting...");
            }
        }

        // updates the GUI of a game room when a user has left it

        public void removePlayerFromRoom(int roomID, string playerNickName)
        {
            Models.GameRoomGUIModel gameRoomGUI;

            gameRoomGUI = getRoomByID(roomID);

            if (gameRoomGUI != null)
            {
                gameRoomGUI.removePlayerFromRoom(playerNickName);
                gameRoomGUI.setState("Open...");
            }
        }

        public void setRoomState(int roomID, string state)
        {
            Models.GameRoomGUIModel gameRoomGUI;

            gameRoomGUI = getRoomByID(roomID);

            if (gameRoomGUI != null)
            {
                gameRoomGUI.setState(state);
            }
        }

        // updates the GUI of a game room when a user has set himself as ready
        public void setReady(int id, string playerNickName)
        {
            for (int i = 0; i < listGameRoomsGUI.Count; i++)
                if (listGameRoomsGUI[i].ID == id)
                {
                    listGameRoomsGUI[i].setReady(playerNickName);
                    break;
                }
        }

        private void lblLoggedInAs_MouseUp(object sender, MouseButtonEventArgs e)
        {
            GUIWindows.GUIUserData gUIUserData = new GUIWindows.GUIUserData(ctrl.userData);

            gUIUserData.Show();
        }
    }
}