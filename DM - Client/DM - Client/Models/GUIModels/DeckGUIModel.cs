﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DM___Client.Models
{
    public class DeckGUIModel
    {
        private GUIPages.GUICollection Parent;
        private string DeckName;
        private Button btn;

        public int ID;
        public Border Border { get; private set; }
        public bool isSelected;

        public DeckGUIModel(int ID_, string DeckName_, GUIPages.GUICollection parent_)
        {
            Parent = parent_;
            ID = ID_;
            DeckName = DeckName_;

            // Border
            Border = new Border();
            Border.Width = 129;
            Border.BorderBrush = System.Windows.Media.Brushes.Transparent;
            Border.BorderThickness = new Thickness(3);
            Border.Margin = new Thickness(0, 0, 0, 0);

            // Button
            Style style = Parent.FindResource("buttonStyle") as Style;
            btn = new Button();
            btn.Style = style;
            btn.Content = DeckName;
            btn.Cursor = System.Windows.Input.Cursors.Hand;
            btn.Opacity = 0.8;
            btn.Background = System.Windows.Media.Brushes.Black;
            btn.Foreground = System.Windows.Media.Brushes.White;
            btn.FontSize = 15;
            btn.Height = 30;
            btn.Width = 123;
            btn.HorizontalAlignment = HorizontalAlignment.Center;
            btn.Click += deckBtn_Click;
            Border.Child = btn;
        }

        private void deckBtn_Click(object sender, RoutedEventArgs e)
        {
            if (isSelected)
            {
                Parent.DeselectDeck();
                deselect();
            }
            else
            {
                select();
            }
        }

        public void deselect()
        {
            Border.BorderBrush = System.Windows.Media.Brushes.Transparent;
            isSelected = false;
        }

        public void select()
        {
            Parent.deselectAll();
            Border.BorderBrush = System.Windows.Media.Brushes.Gold;
            Parent.SelectDeck(ID);
            isSelected = true;
        }
    }
}
