using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace DM___Client.Models
{
    public class DeckItemGUIModel
    {
        private Label lbl;
        private Button btn;
        private TextBlock txt;
        private GUIPages.GUICollection Parent;
        private DockPanel dp;

        public Border Border;
        public Models.Card Card { get; private set; }
        public int count { get; private set; }

        public DeckItemGUIModel(Models.CollectionDeckItem cdi, GUIPages.GUICollection parent_)
        {
            Parent = parent_;
            Card = cdi.Card;
            count = cdi.Count;

            // Border
            Border = new Border();
            Border.BorderBrush = Brushes.Transparent;
            Border.BorderThickness = new Thickness(1);

            // Dockpanel
            dp = new DockPanel();

            // Label
            lbl = new Label();
            lbl.Background = Brushes.Black;
            lbl.FontSize = 14;
            lbl.Content = count.ToString() + "x";
            lbl.Foreground = Brushes.Lime;
            lbl.Opacity = 0.8;
            lbl.BorderBrush = Brushes.DarkGray;
            lbl.BorderThickness = new Thickness(1, 1, 0, 1);
            lbl.Width = 25;

            //Button
            Style style = Parent.FindResource("cardButtonStyle") as Style;
            btn = new Button();
            btn.Style = style;
            btn.Cursor = System.Windows.Input.Cursors.Hand;
            btn.Opacity = 0.8;
            btn.Background = Brushes.Black;
            btn.Height = 32;
            btn.Width = 111;
            btn.HorizontalAlignment = HorizontalAlignment.Left;
            btn.HorizontalAlignment = HorizontalAlignment.Center;
            btn.Click += Btn_Click;
            btn.MouseRightButtonDown += Btn_MouseRightButtonDown;
            btn.MouseRightButtonUp += Btn_MouseRightButtonUp;

            // Textblock
            txt = new TextBlock();
            txt.LayoutTransform = new ScaleTransform(1, 0.9);
            txt.TextAlignment = TextAlignment.Center;
            txt.Foreground = Brushes.White;
            txt.TextWrapping = TextWrapping.Wrap;
            txt.FontSize = 11;
            txt.Text = Card.Name;

            btn.Content = txt;

            dp.Children.Add(lbl);
            dp.Children.Add(btn);

            Border.Child = dp;
        }

        private void Btn_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Parent.displayZoomedInImage(Card.Name, Border);
        }

        private void Btn_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Parent.hideZoomedInImage();
        }

        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            Parent.deckItem_Clicked(Card);
        }

        public void increaseCount()
        {
            count++;
            lbl.Content = count.ToString() + "x";
        }

        public void decreaseCount()
        {
            count--;
            lbl.Content = count.ToString() + "x";
        }
    }
}
