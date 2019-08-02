﻿using System;
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
        private Window parent;
        private Image Image;
        private Button Btn;
        private Grid Grd;
        private TextBlock TxtBlock;
        private bool isSelected { get { return Border.BorderBrush == Brushes.Gold ? true : false; } }
        private Log.Logger logger;

        private string cardBackPath = "/Images/GUI/CardBack.png";
        private string cardsPath = "/Images/Cards/";

        private int type;
        
        public Card Card;
        public Border Border;

        public SelectGUI_CardGUIModel(Card card, GUIWindows.GUISelect parent, Thickness margin, bool clickable=true, int shieldNumber=-1)
        {
            type = 1;
            init(card, parent, margin, clickable, shieldNumber);
        }

        public SelectGUI_CardGUIModel(Card card, GUIWindows.GUIDefend parent, Thickness margin, bool clickable=true)
        {
            type = 2;
            init(card, parent, margin, clickable);
        }

        public SelectGUI_CardGUIModel(Card card, GUIWindows.GUIPeek parent, Thickness margin, bool clickable = true)
        {
            type = 3;
            init(card, parent, margin, clickable);
        }

        private void init(Card card, Window parent, Thickness margin, bool clickable, int shieldNumber=-1)
        {
            logger = new Log.Logger();
            Card = card;
            this.parent = parent;

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

            // Grid
            Grd = new Grid();

            // Button
            Btn = new Button();
            Btn.Style = parent.FindResource("cardButtonStyle") as Style;
            Btn.Cursor = System.Windows.Input.Cursors.Hand;

            if (clickable)
                Btn.Click += Btn_Click;

            Border.Child = Grd;

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

        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            if (!isSelected)
            {
                select();
            }
        }

        public void select()
        {
            int result;

            switch (type)
            {
                case 1:
                    result = ((GUIWindows.GUISelect)parent).addToSelectedCards(this);
                    break;
                case 2:
                    result = ((GUIWindows.GUIDefend)parent).addToSelectedCards(this);
                    break;
                case 3:
                    ((GUIWindows.GUIPeek)parent).deselectAll();
                    if (Card != null)
                        ((GUIWindows.GUIPeek)parent).loadCardInfo(Card);
                    result = 0;
                    break;
                default:
                    result = -1;
                    break;
            }

            if (result == 0)
                Border.BorderBrush = Brushes.Gold;
        }

        public void deselect()
        {
            Border.BorderBrush = Brushes.Transparent;

            switch(type)
            {
                case 1:
                    ((GUIWindows.GUISelect)parent).removeFromSelectedCards(this);
                    break;
                case 2:
                    ((GUIWindows.GUIDefend)parent).removeFromSelectedCards(this);
                    break;
                default:
                    break;
            }
        }
    }
}
