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
        private Grid Grd;
        private TextBlock TxtBlock;
        private bool isSelected { get { return Border.BorderBrush == Brushes.Gold; } }
        private Log.Logger logger;

        private string cardBackPath = "/Images/GUI/CardBack.png";
        private string cardsPath = "/Images/Cards/";

        public Models.CardWithGameProperties Card;
        public Border Border;
        public int ShieldNumber { get; set; }

        public CardGUIModel(Models.CardWithGameProperties Card_, GUIPages.GUIGameRoom parent_, Thickness margin, Visibility visibility, int shieldNumber=-1)
        {
            logger = new Log.Logger();
            Card = Card_;
            parent = parent_;
            ShieldNumber = shieldNumber;

            // Border
            Border = new Border();
            Border.Width = 75;
            Border.Height = 100;

            Border.VerticalAlignment = VerticalAlignment.Top;
            Border.HorizontalAlignment = HorizontalAlignment.Left;
            Border.Margin = margin;
            Border.BorderThickness = new Thickness(5);
            Border.BorderBrush = Brushes.Transparent;
            Border.Visibility = visibility;

            //Grid

            Grd = new Grid();

            // Button
            Btn = new Button();
            Btn.Style = parent.FindResource("cardButtonStyle") as Style;
            Btn.Cursor = System.Windows.Input.Cursors.Hand;
            Btn.Click += cardButton_Click;

            Border.Child = Grd;

            // Image
            Image = new Image();
            try
            {
                if (Card == null)
                    Image.Source = new System.Windows.Media.Imaging.BitmapImage(
                        new Uri(
                            string.Format("{0}{1}",
                            AppDomain.CurrentDomain.BaseDirectory,
                            cardBackPath),
                            UriKind.Absolute));
                else
                    Image.Source = new System.Windows.Media.Imaging.BitmapImage(
                        new Uri(
                            string.Format("{0}{1}{2}/{3}.jpg",
                            AppDomain.CurrentDomain.BaseDirectory,
                            cardsPath,
                            Card.Set,
                            Card.Name),
                            UriKind.Absolute));

            }
            catch (Exception ex)
            {
                logger.Log(ex.ToString());
            }

            Image.Stretch = Stretch.UniformToFill;

            Image.MouseRightButtonUp += parent.cardImage_MouseRightButtonUp;
            Image.MouseRightButtonDown += parent.cardImage_MouseRightButtonDown;

            Btn.Content = Image;
            Grd.Children.Add(Btn);

            if (shieldNumber != -1)
            {
                // Textblock

                TxtBlock = new TextBlock();
                TxtBlock.Text = shieldNumber.ToString();
                TxtBlock.FontSize = 15;
                TxtBlock.Foreground = Brushes.White;
                TxtBlock.FontWeight = FontWeights.Bold;
                TxtBlock.HorizontalAlignment = HorizontalAlignment.Left;
                TxtBlock.VerticalAlignment = VerticalAlignment.Top;
                TxtBlock.Margin = new Thickness(10, 5, 0, 0);
                Grd.Children.Add(TxtBlock);
            }
        }

        private void cardButton_Click(object sender, RoutedEventArgs e)
        {
            if (Card != null)
            {
                if (parent.isPartOfGraveyard(this))
                {
                    parent.peekIntoGraveyard();
                    return;
                }

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
                    Image.Source = new System.Windows.Media.Imaging.BitmapImage(
                        new Uri(
                            string.Format("{0}{1}",
                            AppDomain.CurrentDomain.BaseDirectory,
                            cardBackPath),
                            UriKind.Absolute));
                else
                    Image.Source = new System.Windows.Media.Imaging.BitmapImage(
                        new Uri(
                            string.Format("{0}{1}{2}/{3}.jpg",
                            AppDomain.CurrentDomain.BaseDirectory,
                            cardsPath,
                            Card.Set,
                            Card.Name),
                            UriKind.Absolute));

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

        public void removeTextBlock()
        {
            Grd.Children.Remove(TxtBlock);
        }
    }
}
