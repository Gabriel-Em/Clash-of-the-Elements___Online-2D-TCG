using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DM___Client.Models;
using DM___Client.Models.GUIModels;

namespace DM___Client.GUIWindows
{
    /// <summary>
    /// Interaction logic for GUISelect.xaml
    /// </summary>
    public partial class GUISelect : Window
    {
        private List<SelectGUI_CardGUIModel> cards;

        private int count;
        private string element;

        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        public List<int> selected;
        public bool wasCanceled = true;

        public GUISelect(List<CardGUIModel> listOfCards, string message, int count, string element)
        {
            Thickness margin;

            InitializeComponent();

            this.count = Math.Min(listOfCards.Count, count);
            this.element = element;

            messageBlock.Text = message;

            cards = new List<SelectGUI_CardGUIModel>();
            selected = new List<int>();

            foreach (CardGUIModel cardGUI in listOfCards)
            {
                if (cards.Count == 0)
                    margin = new Thickness(10, 0, 0, 0);
                else
                {
                    margin = cards[cards.Count - 1].Border.Margin;
                    margin.Left += 75;
                }
                SelectGUI_CardGUIModel sCard = new SelectGUI_CardGUIModel(cardGUI, this, margin);
                grdSelect.Children.Add(sCard.Border);
                cards.Add(sCard);
            }
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            string message;
            bool foundElement;

            if (selected.Count != count)
            {
                message = string.Format("You need to select {0} card(s).", count);
                MessageBox.Show(message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (element != null)
            {
                foundElement = false;
                foreach (int index in selected)
                {
                    if (cards[index].cardGUI.Card.Element == element)
                    {
                        foundElement = true;
                        break;
                    }
                }

                if (!foundElement)
                {
                    message = string.Format("At least one card must be of {0} element.", element);
                    MessageBox.Show(message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            selected.Sort();
            wasCanceled = false;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            wasCanceled = true;
            Close();
        }

        public void addToSelectedCards(SelectGUI_CardGUIModel card)
        {
            selected.Add(cards.IndexOf(card));
        }

        public void removeFromSelectedCards(SelectGUI_CardGUIModel card)
        {
            selected.Remove(cards.IndexOf(card));
        }

        public void replaceCancelButtonMessage(string message)
        {
            btnCancel.Content = message;
        }

        public void removeCancelButton()
        {
            grdParent.Children.Remove(btnCancel);
            btnSelect.Margin = new Thickness(0);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
        }
    }
}
