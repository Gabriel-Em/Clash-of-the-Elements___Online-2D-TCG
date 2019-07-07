using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DM___Client.Models
{
    public class GameRoomGUIModel
    {
        private GUIPages.GUILobbyRoom Parent;

        private Grid grid = new Grid();
        private DockPanel dp = new DockPanel();
        private Border borderForP1 = new Border();
        private Label labelForP1 = new Label();
        private Border borderForP2 = new Border();
        private Label labelForP2 = new Label();
        private Image VSimage = new Image();
        private Label stateLabel = new Label();
        private Button mainButton = new Button();
        private Button readyButton = new Button();

        private bool mainButtonIsJoin;
        private string guestNickName;

        private bool OwnRoom { get; set; }

        private string COLORNOTREADY = "#FE2E2E";
        private string COLORREADY = "#00FF00";

        private Log.Logger logger;

        public int ID { get; set; }
        public Border Border { get; private set; }
        
        public GameRoomGUIModel(GameRoom room, string myNickName, GUIPages.GUILobbyRoom parent_)
        {
            logger = new Log.Logger();
            Parent = parent_;
            ID = room.RoomID;
            if (myNickName == room.Owner)
                OwnRoom = true;
            else
                OwnRoom = false;
            guestNickName = room.Guest;

            ColumnDefinition cd = new ColumnDefinition();
            BrushConverter bc = new BrushConverter();

            // Border
            Border = new Border();
            Border.Height = 100;
            Border.Margin = new Thickness(10);
            Border.CornerRadius = new CornerRadius(20);
            Border.Background = Brushes.Black;
            Border.Opacity = 0.8;
            Border.BorderThickness = new Thickness(2);
            Border.BorderBrush = Brushes.LightGray;

            // GRID

            cd.Width = GridLength.Auto;
            grid.ColumnDefinitions.Add(cd);
            cd = new ColumnDefinition();
            cd.Width = new GridLength(1, GridUnitType.Star);
            grid.ColumnDefinitions.Add(cd);
            cd = new ColumnDefinition();
            cd.Width = GridLength.Auto;
            grid.ColumnDefinitions.Add(cd);

            // PLAYER1 BORDER

            borderForP1.CornerRadius = new CornerRadius(10);
            borderForP1.Background = Brushes.Gold;
            borderForP1.Margin = new Thickness(25, 23, 0, 23);
            borderForP1.Padding = new Thickness(5, 0, 5, 0);
            borderForP1.BorderBrush = Brushes.White;
            borderForP1.BorderThickness = new Thickness(2);

            // PLAYER1 LABEL

            labelForP1.Content = room.Owner;
            labelForP1.FontSize = 25;
            labelForP1.Foreground = Brushes.Black;
            labelForP1.VerticalAlignment = VerticalAlignment.Center;
            labelForP1.FontWeight = FontWeights.Bold;
            borderForP1.Child = labelForP1;

            // IMAGE

            try
            {
                VSimage.Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "/Images/GUI/VS.png", UriKind.Absolute));
            }
            catch (Exception)
            {
                logger.Log("Couldn't load 'versus image' in game room " + ID);
            }

            VSimage.Width = 60;
            VSimage.Margin = new Thickness(25, 0, 25, 0);

            // PLAYER2 BORDER

            borderForP2.CornerRadius = new CornerRadius(10);
            borderForP2.HorizontalAlignment = HorizontalAlignment.Left;
            borderForP2.Background = Brushes.Gold;
            borderForP2.Margin = new Thickness(0, 23, 20, 23);
            borderForP2.Padding = new Thickness(5, 0, 5, 0);
            borderForP2.BorderBrush = Brushes.White;
            borderForP2.BorderThickness = new Thickness(2);

            // PLAYER2 LABEL

            labelForP2.Content = room.Guest;
            labelForP2.FontSize = 25;
            labelForP2.Foreground = Brushes.Black;
            labelForP2.VerticalAlignment = VerticalAlignment.Center;
            labelForP2.FontWeight = FontWeights.Bold;
            borderForP2.Child = labelForP2;

            dp.Children.Add(borderForP1);
            dp.Children.Add(VSimage);
            dp.Children.Add(borderForP2);

            grid.Children.Add(dp);

            // STATE

            stateLabel.Content = room.State;
            stateLabel.FontSize = 23;
            stateLabel.Foreground = Brushes.White;
            stateLabel.HorizontalAlignment = HorizontalAlignment.Center;
            stateLabel.VerticalAlignment = VerticalAlignment.Center;
            stateLabel.FontWeight = FontWeights.Bold;

            grid.Children.Add(stateLabel);
            Grid.SetColumn(stateLabel, 1);

            // mainButton

            Style style = Parent.FindResource("buttonStyle") as Style;
            mainButton.Style = style;
            mainButton.HorizontalAlignment = HorizontalAlignment.Right;
            mainButton.VerticalAlignment = VerticalAlignment.Top;
            mainButton.Cursor = System.Windows.Input.Cursors.Hand;
            mainButton.Background = Brushes.Black;
            mainButton.Foreground = Brushes.White;
            mainButton.FontSize = 20;
            mainButton.Margin = new Thickness(10, 10, 10, 0);
            mainButton.Width = 110;
            mainButton.Click += btnMain_Click;

            // if it's your room then you can close it
            if (OwnRoom)
            {
                mainButton.Content = "Close";
                mainButtonIsJoin = false;
            }
            else
            {
                // if it's not your room then you can join it if it's free, otherwise you can't do anything
                mainButton.Content = "Join";
                mainButtonIsJoin = true;
                if (room.Guest != "*")
                    mainButton.IsEnabled = false;
            }

            // readyButton

            readyButton.Style = style;
            readyButton.HorizontalAlignment = HorizontalAlignment.Right;
            readyButton.VerticalAlignment = VerticalAlignment.Bottom;
            readyButton.Cursor = System.Windows.Input.Cursors.Hand;
            readyButton.Background = Brushes.Black;
            readyButton.Foreground = Brushes.White;
            readyButton.FontSize = 20;
            readyButton.Margin = new Thickness(10,0,10,10);
            readyButton.Width = 110;
            readyButton.Content = "Ready";
            readyButton.IsEnabled = false;
            readyButton.Click += btnReady_Click;

            grid.Children.Add(mainButton);
            grid.Children.Add(readyButton);
            Grid.SetColumn(mainButton, 2);
            Grid.SetColumn(readyButton, 2);

            Border.Child = grid;
        }

        private void btnMain_Click(object sender, RoutedEventArgs e)
        {
            mainButton.IsEnabled = false;

            // if it was your room, then pressing this button will close it
            if (OwnRoom)
                Parent.closeRoom(ID);
            else
            {
                // if it was not your room and it was open to join, then this button will allow you to join the room
                if (mainButtonIsJoin)
                    Parent.joinRoom(ID);
                // otherwise it means that you have already joined the room as a guest so this button will allow you to leave the room
                else
                    Parent.leaveRoom(ID);
            }
        }

        private void btnReady_Click(object sender, RoutedEventArgs e)
        {
            // you're setting yourself as ready to begin the match
            readyButton.IsEnabled = false;
            readyButton.Background = Brushes.Green;
            Parent.sendReadyNotification(ID);
        }

        public void removePlayerFromRoom(string nickName)
        {
            labelForP2.Content = "*";
            borderForP1.Background = Brushes.Gold;
            borderForP2.Background = Brushes.Gold;
            readyButton.IsEnabled = false;
            readyButton.Background = Brushes.Black;
            if (!OwnRoom)
            {
                if (guestNickName == nickName)
                {
                    mainButton.Content = "Join";
                    mainButtonIsJoin = true;
                }
            }

            mainButton.IsEnabled = true;
            guestNickName = "*";
        }

        public void attachPlayerToGameRoom(string nickName, int type)
        {
            BrushConverter bc = new BrushConverter();

            labelForP2.Content = nickName;
            guestNickName = nickName;

            if (type == 1)                          // if you were the one that joined a room
            {
                borderForP1.Background = (Brush)bc.ConvertFrom(COLORNOTREADY);
                borderForP2.Background = (Brush)bc.ConvertFrom(COLORNOTREADY);
                mainButtonIsJoin = false;
                mainButton.Content = "Leave";
                mainButton.IsEnabled = true;
                readyButton.IsEnabled = true;
            }
            else 
            {
                if (OwnRoom)                        // if someone else joined your room
                {
                    borderForP1.Background = (Brush)bc.ConvertFrom(COLORNOTREADY);
                    borderForP2.Background = (Brush)bc.ConvertFrom(COLORNOTREADY);
                    readyButton.IsEnabled = true;
                }
                else
                {
                    mainButton.IsEnabled = false;   // if someone else joined another room
                    borderForP1.Background = (Brush)bc.ConvertFrom("#6E6E6E");
                    borderForP2.Background = (Brush)bc.ConvertFrom("#6E6E6E");
                }
            }
        }

        public void setReady(string playerNickName)
        {
            BrushConverter bc = new BrushConverter();
            if (guestNickName == playerNickName)
                borderForP2.Background = (Brush)bc.ConvertFrom(COLORREADY);
            else
                borderForP1.Background = (Brush)bc.ConvertFrom(COLORREADY);
        }

        public void setState(string state)
        {
            stateLabel.Content = state;
        }
    }
}
