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

namespace DM___Client.Models.GUIModels
{
    public class SelectGUI_CardGUIModel
    {
        private GUIWindows.GUISelect parent;
        private Image Image;
        private Button Btn;
        private bool isSelected { get { if (Border.BorderBrush == Brushes.Gold) return true; return false; } }
        private Log.Logger logger;

        private string cardBackPath = "/Images/GUI/CardBack.png";
        private string cardsPath = "/Images/Cards/";

        public CardGUIModel cardGUI;
        public Border Border;

        public SelectGUI_CardGUIModel(CardGUIModel cardGUI, GUIWindows.GUISelect parent_, Thickness margin)
        {
            logger = new Log.Logger();
            this.cardGUI = cardGUI;
            parent = parent_;

            // Border
            Border = new Border();
            Border.Width = 75;
            Border.Height = 100;
            Border.HorizontalAlignment = HorizontalAlignment.Left;
            Border.Margin = margin;
            Border.BorderThickness = new Thickness(5);
            Border.BorderBrush = Brushes.Transparent;
            TranslateTransform t = new TranslateTransform(0, 0);
            Border.RenderTransform = t;
            Border.Visibility = Visibility.Visible;

            // Button
            Btn = new Button();
            Btn.Style = parent.FindResource("cardButtonStyle") as Style;
            Btn.Cursor = System.Windows.Input.Cursors.Hand;

            Btn.Click += Btn_Click;

            Border.Child = Btn;

            // Image
            Image = new Image();
            try
            {
                if (cardGUI.Card == null)
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
                            cardGUI.Card.Set,
                            cardGUI.Card.Name),
                            UriKind.Absolute));
            }
            catch (Exception ex)
            {
                logger.Log(ex.ToString());
            }

            Image.Stretch = Stretch.UniformToFill;
            Btn.Content = Image;
        }

        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            if (isSelected)
                deselect();
            else
            {
                select();
            }
        }

        public void select()
        {
            int result;

            result = parent.addToSelectedCards(this);

            if (result == 0)
                Border.BorderBrush = Brushes.Gold;
        }
        public void deselect()
        {
            Border.BorderBrush = Brushes.Transparent;
            parent.removeFromSelectedCards(this);
        }
    }
}
