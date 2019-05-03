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
        private bool isSelected { get { return Border.BorderBrush == Brushes.Gold; } }
        private Log.Logger logger;

        public Models.CardWithGameProperties Card;
        public Border Border;

        public CardGUIModel(Models.CardWithGameProperties Card_, GUIPages.GUIGameRoom parent_, Thickness margin, Visibility visibility)
        {
            logger = new Log.Logger();
            Card = Card_;
            parent = parent_;

            // Border
            Border = new Border();
            Border.Width = 73;
            Border.Height = 100;
            Border.VerticalAlignment = VerticalAlignment.Top;
            Border.HorizontalAlignment = HorizontalAlignment.Left;
            Border.Margin = margin;
            Border.BorderThickness = new Thickness(5);
            Border.BorderBrush = Brushes.Transparent;
            Border.Visibility = visibility;

            // Button
            Btn = new Button();
            Btn.Style = parent.FindResource("cardButtonStyle") as Style;
            Btn.Cursor = System.Windows.Input.Cursors.Hand;
            Btn.Click += cardButton_Click;

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
                logger.Log(ex.ToString());
            }

            Image.Stretch = Stretch.UniformToFill;
            Image.MouseRightButtonUp += parent.cardImage_MouseRightButtonUp;
            Image.MouseRightButtonDown += parent.cardImage_MouseRightButtonDown;

            Btn.Content = Image;
        }

        private void cardButton_Click(object sender, RoutedEventArgs e)
        {
            if (Card != null)
            {
                parent.loadCardInfo(Card);
                if (isSelected)
                    deselect();
                else
                {
                    if (parent.isAbleToSelect(this))
                    {
                        select();
                    }
                }
            }
        }

        public void select()
        {
            parent.deselectAll();
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

        public CardGUIModel Clone()
        {
            CardGUIModel cardGUI = new CardGUIModel(Card, parent, Border.Margin, Border.Visibility);
            cardGUI.Border.RenderTransform = this.Border.RenderTransform;

            return cardGUI;
        }

        public void setCard(Models.CardWithGameProperties card)
        {
            Card = card;

            try
            {
                if (Card == null)
                    Image.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "/Images/GUI/CardBack.png", UriKind.Absolute));
                else
                    Image.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "/Images/Cards/" + Card.Name + ".png", UriKind.Absolute));
            }
            catch (Exception ex)
            {
                logger.Log(ex.ToString());
            }
        }

        public void setMargin(Thickness margin)
        {
            Border.Margin = margin;
        }
    }
}
