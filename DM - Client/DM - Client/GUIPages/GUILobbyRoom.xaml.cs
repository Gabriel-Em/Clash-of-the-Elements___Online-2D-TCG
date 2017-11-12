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
        private GUIWindows.GUI parent;
        private Controllers.LobbyRoomController ctrl;
        private DispatcherTimer checkServerResponse = new DispatcherTimer();
        private bool chatWindowEmpty;
        private List<Models.GameRoomGUIModel> GameRoomsGUI;

        public ImageSource BackgroundImageSource { get { return backgroundImage.Source; } }
        public bool CardCollectionLoaded { get { return parent.CardCollectionLoaded; } }

        public GUILobbyRoom(GUIWindows.GUI parent_, Communication com_)
        {
            InitializeComponent();
            parent = parent_;
            chatWindowEmpty = true;
            GameRoomsGUI = new List<Models.GameRoomGUIModel>();
            ctrl = new Controllers.LobbyRoomController(this, com_);
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
            GUIWindows.GUIDisconnected disconnectWindow = new GUIWindows.GUIDisconnected(message,type);
            disconnectWindow.ShowDialog();
            
            parent.loadLogIn();
        }

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

        public void removeLobbyRoomUser(string user)
        {
            for(int i =0;i<listBoxUsers.Items.Count;i++)
                if (((ListBoxItem)listBoxUsers.Items[i]).Content.ToString() == user)
                {
                    listBoxUsers.Items.RemoveAt(i);
                    break;
                }
        }

        public void updateLoggedInAs(string nickName)
        {
            lblLoggedInAs.Content = nickName;
        }

        private void btnSubmitText_Click(object sender, RoutedEventArgs e)
        {
            sendChatMessage();
        }

        internal void noRoomsForMe()
        {
            MessageBox.Show("You need to have at least 1 deck in your collection before interacting with game rooms.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        internal void joinPreGameRoom(int RoomID)
        {
            stopListening();
            ctrl.send(new Models.ClientMessage("LEAVELOBBY"));
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

        public void newChatMessage(List<string> arguments)
        {
            TextRange tr = new TextRange(richTextboxChat.Document.ContentEnd,richTextboxChat.Document.ContentEnd);
            if (!chatWindowEmpty)
                tr.Text = "\n";
            chatWindowEmpty = false;
            tr.Text += arguments[0] + ": ";
            tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
            tr = new TextRange(richTextboxChat.Document.ContentEnd, richTextboxChat.Document.ContentEnd);
            tr.Text = arguments[1];
            tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);
            richTextboxChat.ScrollToEnd();
        }

        private void TextBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != System.Windows.Input.Key.Enter)
                return;
            sendChatMessage();
        }

        private void btnDecks_Click(object sender, RoutedEventArgs e)
        {
            stopListening();
            ctrl.send(new Models.ClientMessage("LEAVELOBBY"));
            parent.loadCollection();
        }

        public void setCardCollection(Models.CardCollection CardCollection)
        {
            parent.setCardCollection(CardCollection);
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

        private void btnQuit_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to quit?", "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                System.Windows.Application.Current.Shutdown();
        }

        private void btnCreateLobbyRoom_Click(object sender, RoutedEventArgs e)
        {
            ctrl.send(new Models.ClientMessage("CREATEGAMEROOM"));
        }

        public void AddRoomToGUI(Models.GameRoom room)
        {
            Models.GameRoomGUIModel grguim = new Models.GameRoomGUIModel(room, ctrl.userData.NickName, this);
            GameRoomsGUI.Add(grguim);
            stackRooms.Children.Add(grguim.Border);
        }

        public void RemoveRoomFromGUI(int id)
        {
            for(int i =0;i<GameRoomsGUI.Count;i++)
            {
                if(GameRoomsGUI[i].ID == id)
                {
                    stackRooms.Children.Remove(GameRoomsGUI[i].Border);
                    GameRoomsGUI.RemoveAt(i);
                    break;
                }
            }
        }

        public void closeRoom(int id)
        {
            ctrl.send(new Models.ClientMessage("CLOSEROOM", new List<string>() { id.ToString() }));
        }

        public void joinRoom(int id)
        {
            ctrl.send(new Models.ClientMessage("JOINROOM", new List<string>() { id.ToString() }));
        }

        public void leaveRoom(int id)
        {
            ctrl.send(new Models.ClientMessage("LEAVEROOM", new List<string>() { id.ToString() }));
        }

        public void ready(int id, bool owner)
        {
            ctrl.send(new Models.ClientMessage("READYROOM", new List<string>() { id.ToString(), owner.ToString() }));
        }

        public void playerJoinedRoom(int id, string nickName)
        {
            for (int i = 0; i < GameRoomsGUI.Count; i++)
                if (GameRoomsGUI[i].ID == id)
                {
                    if (ctrl.userData.NickName == nickName)
                        GameRoomsGUI[i].join(nickName, 1); // you joined a room
                    else
                        GameRoomsGUI[i].join(nickName, 2); // someone else joined a room
                    break;
                }
        }

        public void playerLeftRoom(int id, string nickName)
        {
            for (int i = 0; i < GameRoomsGUI.Count; i++)
                if (GameRoomsGUI[i].ID == id)
                {
                    GameRoomsGUI[i].leave(nickName);
                    break;
                }
        }

        internal void setReady(int id, bool owner)
        {
            for (int i = 0; i < GameRoomsGUI.Count; i++)
                if (GameRoomsGUI[i].ID == id)
                {
                    GameRoomsGUI[i].setReady(owner);
                    break;
                }
        }
    }
}