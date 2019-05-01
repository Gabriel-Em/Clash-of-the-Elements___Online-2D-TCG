using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DM___Client.Models
{
    public class PreGameDeckGUIModel
    {
        private GUIPages.GUIPreGameRoom Parent;
        private string DeckName;
        private Button btn;

        public int ID;
        public Border Border { get; private set; }

        public PreGameDeckGUIModel(int ID_, string DeckName_, GUIPages.GUIPreGameRoom parent_)
        {
            Parent = parent_;
            ID = ID_;
            DeckName = DeckName_;

            // Border
            Border = new Border();
            Border.Width = 429;
            Border.BorderBrush = System.Windows.Media.Brushes.Transparent;
            Border.BorderThickness = new Thickness(3);
            Border.Margin = new Thickness(0, 10, 0, 0);

            // Button
            Style style = Parent.FindResource("buttonStyle") as Style;
            btn = new Button();
            btn.Style = style;
            btn.Content = DeckName;
            btn.Cursor = System.Windows.Input.Cursors.Hand;
            btn.Opacity = 0.8;
            btn.Background = System.Windows.Media.Brushes.Black;
            btn.Foreground = System.Windows.Media.Brushes.LightGoldenrodYellow;
            btn.FontSize = 25;
            btn.Height = 45;
            btn.Width = 423;
            btn.HorizontalAlignment = HorizontalAlignment.Center;
            btn.Click += Btn_Click;
            Border.Child = btn;
        }


        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            Parent.SelectDeck(ID);
            Border.BorderBrush = System.Windows.Media.Brushes.Gold;
        }

        public void deselect()
        {
            Border.BorderBrush = System.Windows.Media.Brushes.Transparent;
        }
    }
}