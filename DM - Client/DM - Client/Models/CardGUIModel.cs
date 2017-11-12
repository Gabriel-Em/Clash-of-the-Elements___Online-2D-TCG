using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.IO;
using System.Windows.Threading;
using System.Windows.Media.Animation;

namespace DM___Client.Models
{
    public class CardGUIModel
    {
        private GUIPages.GUIGameRoom parent;
        private Image Image;
        private Button Btn;
        private bool isSelected { get { if (Border.BorderBrush == Brushes.Gold) return true; return false; } }

        public Models.CardWithGameProperties Card;
        public Border Border;
        public int type;
        /*
         * 0 - animation
         * 1 - own shield
         * -1 - opp shield
         * 2 - own mana
         * -2 - opp mana
         * 3 - own battlezone
         * -3 - opp battlezone
         * 4 - own graveyard
         * 4 - opp graveyard
         * 5
         */

        public CardGUIModel(Models.CardWithGameProperties Card_, GUIPages.GUIGameRoom parent_, Thickness margin, Visibility visibility, int type_)
        {
            Card = Card_;
            parent = parent_;
            type = type_;

            // Border
            Border = new Border();
            Border.Width = 65;
            Border.Height = 88;
            Border.HorizontalAlignment = HorizontalAlignment.Left;
            Border.Margin = margin;
            Border.BorderThickness = new Thickness(3);
            Border.BorderBrush = Brushes.Transparent;
            TranslateTransform t = new TranslateTransform(0, 0);
            Border.RenderTransform = t;
            Border.Visibility = visibility;

            // Button
            Btn = new Button();
            Btn.Style = parent.FindResource("cardButtonStyle") as Style;
            Btn.Cursor = System.Windows.Input.Cursors.Hand;

            if (type != -1)
                Btn.Click += Btn_Click;

            Border.Child = Btn;

            // Image
            Image = new Image();
            try
            {
                if (Card == null)
                    Image.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "/Images/GUI/CardBack.png", UriKind.Absolute));
                else
                    Image.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "/Images/Cards/" + Card.Name + ".png", UriKind.Absolute));
                
            }
            catch(Exception ex)
            {
                if (!Directory.Exists(@".\Logs"))
                    Directory.CreateDirectory(@".\Logs");

                StreamWriter file = new StreamWriter(@".\Logs\" + DateTime.Now.ToString("yyyy -dd-MM--HH-mm-ss") + Guid.NewGuid().ToString() + "_Crash_Log.txt");
                file.Write(ex.ToString());
                file.Close();
            }
            Btn.Content = Image;
        }

        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            if (Card != null)
                parent.loadCardInfo(Card);
            if (isSelected)
                deselect();
            else
            {
                if (parent.isAbleToSelect(this))
                {
                    if (parent.Phase == "Mana phase")
                        parent.deselectAll();
                    select();
                }
            }
        }

        public void select()
        {
            Border.BorderBrush = Brushes.Gold;
            parent.addToSelectedCards(this);
        }
        public void deselect()
        {
            Border.BorderBrush = Brushes.Transparent;
            parent.removeFromSelectedCards(this);
        }

        public void turnVisibilityON()
        {
            Border.Visibility = Visibility.Visible;
        }

        public void turnVisibilityOFF()
        {
            Border.Visibility = Visibility.Hidden;
        }
    }
}
