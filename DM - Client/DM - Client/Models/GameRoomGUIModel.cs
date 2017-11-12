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
        private GUIPages.GUILobbyRoom Parent;
        private bool mainIsJoin;
        private string joinedNickname;
        private bool OwnRoom { get; set; }

        public int ID { get; set; }
        public Border Border { get; private set; }
        
        public GameRoomGUIModel(GameRoom room, string myNickName, GUIPages.GUILobbyRoom parent_)
        {
            Parent = parent_;
            ID = room.RoomID;
            if (myNickName == room.Owner)
                OwnRoom = true;
            else
                OwnRoom = false;
            joinedNickname = room.Joined;

            ColumnDefinition cd = new ColumnDefinition();
            BrushConverter bc = new BrushConverter();

            // Border
            Border = new Border();
            Border.Height = 100;
            Border.Margin = new Thickness(10);
            Border.CornerRadius = new CornerRadius(20);
            Border.Background = Brushes.Black;
            Border.Opacity = 0.8;
            Border.BorderThickness = new Thickness(3);
            Border.BorderBrush = Brushes.Gold;

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
            catch (Exception ex)
            {
                if (!Directory.Exists(@".\Logs"))
                    Directory.CreateDirectory(@".\Logs");

                StreamWriter file = new StreamWriter(@".\Logs\" + DateTime.Now.ToString("yyyy -dd-MM--HH-mm-ss") + Guid.NewGuid().ToString() + "_Crash_Log.txt");
                file.Write(ex.ToString());
                file.Close();
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

            labelForP2.Content = room.Joined;
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
            if (OwnRoom)
            {
                mainButton.Content = "Close";
                mainIsJoin = false;
            }
            else
            {
                mainButton.Content = "Join";
                mainIsJoin = true;
                if (room.Joined != "*")
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
            if (OwnRoom)
                Parent.closeRoom(ID);
            else
            {
                if (mainIsJoin)
                    Parent.joinRoom(ID);
                else
                    Parent.leaveRoom(ID);
            }
        }

        private void btnReady_Click(object sender, RoutedEventArgs e)
        {
            readyButton.IsEnabled = false;
            mainButton.IsEnabled = false;
            Parent.ready(ID, OwnRoom);
        }

        public void leave(string nickName)
        {
            labelForP2.Content = "*";
            borderForP1.Background = Brushes.Gold;
            borderForP2.Background = Brushes.Gold;
            readyButton.IsEnabled = false;
            if (!OwnRoom)
            {
                if (joinedNickname == nickName)
                {
                    mainButton.Content = "Join";
                    mainIsJoin = true;
                }
                mainButton.IsEnabled = true;
            }

            joinedNickname = "*";
        }

        public void join(string nickName, int type)
        {
            BrushConverter bc = new BrushConverter();

            labelForP2.Content = nickName;
            joinedNickname = nickName;

            if (type == 1)
            {
                borderForP1.Background = (Brush)bc.ConvertFrom("#FFC33333");
                borderForP2.Background = (Brush)bc.ConvertFrom("#FFC33333");
                mainIsJoin = false;
                mainButton.Content = "Leave";
                mainButton.IsEnabled = true;
                readyButton.IsEnabled = true;
            }
            else
            {
                if (OwnRoom)
                {
                    borderForP1.Background = (Brush)bc.ConvertFrom("#FFC33333");
                    borderForP2.Background = (Brush)bc.ConvertFrom("#FFC33333");
                    readyButton.IsEnabled = true;
                }
                else
                    mainButton.IsEnabled = false;
            }
        }

        internal void setReady(bool owner)
        {
            BrushConverter bc = new BrushConverter();
            if (owner)
                borderForP1.Background = (Brush)bc.ConvertFrom("#FF2C9E2C");
            else
                borderForP2.Background = (Brush)bc.ConvertFrom("#FF2C9E2C");
        }
    }
}
