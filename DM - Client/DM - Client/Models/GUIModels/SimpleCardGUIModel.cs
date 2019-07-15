using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DM___Client.Models.GUIModels
{
    public class SimpleCardGUIModel
    {
        private Image Image;
        private Log.Logger logger;

        private string cardBackPath = "/Images/GUI/CardBack.png";
        private string cardsPath = "/Images/Cards/";

        public Models.Card Card;
        public Border Border;

        public SimpleCardGUIModel(Models.Card card, int width=75, int height=104)
        {
            logger = new Log.Logger();

            // Border
            Border = new Border();
            Border.Width = width;
            Border.Height = height;

            Border.VerticalAlignment = VerticalAlignment.Top;
            Border.HorizontalAlignment = HorizontalAlignment.Left;
            Border.Margin = new Thickness(0);
            Border.BorderThickness = new Thickness(5);
            Border.BorderBrush = Brushes.Transparent;

            // Image
            Image = new Image();
            try
            {
                if (card == null)
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
                            card.Set,
                            card.Name),
                            UriKind.Absolute));

            }
            catch (Exception ex)
            {
                logger.Log(ex.ToString());
            }

            Image.Stretch = Stretch.UniformToFill;

            Border.Child = Image;
        }
    }
}
