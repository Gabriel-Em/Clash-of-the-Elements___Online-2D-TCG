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
        private List<SelectGUI_CardGUIModel> ownCards;
        private List<SelectGUI_CardGUIModel> oppCards;

        private int ownCount;
        private int oppCount;
        private string element;
        private string zone;
        private bool treatCountAsOne;

        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        public List<int> ownSelected;
        public List<int> oppSelected;
        public bool wasCanceled = true;

        public GUISelect(List<CardGUIModel> ownCards, List<CardGUIModel> oppCards, string message, string zone, int ownCount, int oppCount, bool treatCountAsOne=false, string element=null)
        {
            InitializeComponent();

            this.treatCountAsOne = treatCountAsOne;

            this.ownCount = Math.Min(ownCards.Count, ownCount);
            this.oppCount = Math.Min(oppCards.Count, oppCount);

            this.element = element;
            this.zone = zone;

            messageBlock.Text = message;
            messageBlockOwn.Text = string.Format("Own {0} [{1} remaining]", zone, this.ownCount);
            messageBlockOpp.Text = string.Format("Opp {0} [{1} remaining]", zone, this.oppCount);

            ownSelected = new List<int>();
            oppSelected = new List<int>();

            this.ownCards = new List<SelectGUI_CardGUIModel>();
            this.oppCards = new List<SelectGUI_CardGUIModel>();

            cardsToGUI(ownCards, true);
            cardsToGUI(oppCards, false);
        }

        private void cardsToGUI(List<CardGUIModel> cardList, bool own)
        {
            Thickness margin;

            foreach (CardGUIModel cardGUI in cardList)
            {
                if (own && ownCards.Count == 0 || !own && oppCards.Count == 0)
                    margin = new Thickness(10, 0, 0, 0);
                else
                {
                    if (own)
                    {
                        margin = ownCards[ownCards.Count - 1].Border.Margin;
                    }
                    else
                    {
                        margin = oppCards[oppCards.Count - 1].Border.Margin;
                    }
                    margin.Left += 75;
                }
                SelectGUI_CardGUIModel sCard = new SelectGUI_CardGUIModel(cardGUI, this, margin);

                if (own)
                {
                    this.ownCards.Add(sCard);
                    grdSelectOwn.Children.Add(sCard.Border);
                }
                else
                {
                    this.oppCards.Add(sCard);
                    grdSelectOpp.Children.Add(sCard.Border);
                }
            }
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            string message;
            bool foundElement;

            if (ownCount != 0)
            {
                message = string.Format("You need to select {0} more of your own card(s).", ownCount);
                MessageBox.Show(message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (oppCount != 0)
            {
                message = string.Format("You need to select {0} more of your opponent's card(s).", oppCount);
                MessageBox.Show(message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            if (element != null)
            {
                foundElement = false;
                foreach (int index in ownSelected)
                {
                    if (ownCards[index].cardGUI.Card.Element == element)
                    {
                        foundElement = true;
                        break;
                    }
                }

                if (!foundElement)
                {
                    foreach (int index in oppSelected)
                    {
                        if (oppCards[index].cardGUI.Card.Element == element)
                        {
                            foundElement = true;
                            break;
                        }
                    }
                }

                if (!foundElement)
                {
                    message = string.Format("At least one card must be of {0} element.", element);
                    MessageBox.Show(message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            oppSelected.Sort();
            ownSelected.Sort();
            wasCanceled = false;

            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            wasCanceled = true;
            Close();
        }

        public int addToSelectedCards(SelectGUI_CardGUIModel card)
        {
            if (ownCards.Contains(card))
            {
                if (ownCount > 0)
                {
                    ownSelected.Add(ownCards.IndexOf(card));
                    messageBlockOwn.Text = string.Format("Own {0} [{1} remaining]", zone, --(this.ownCount));
                    if (treatCountAsOne)
                    {
                        if (oppCards.Count > 0)
                            messageBlockOpp.Text = string.Format("Opp {0} [{1} remaining]", zone, --(this.oppCount));
                    }
                }
                else
                    return - 1;
            }
            else
            {
                if (oppCount > 0)
                {
                    oppSelected.Add(oppCards.IndexOf(card));
                    messageBlockOpp.Text = string.Format("Opp {0} [{1} remaining]", zone, --(this.oppCount));
                    if (treatCountAsOne)
                    {
                        if (ownCards.Count > 0)
                            messageBlockOwn.Text = string.Format("Own {0} [{1} remaining]", zone, --(this.ownCount));
                    }
                }
                else
                    return -1;
            }
            return 0;
        }

        public void removeFromSelectedCards(SelectGUI_CardGUIModel card)
        {
            if (ownCards.Contains(card))
            {
                ownSelected.Remove(ownCards.IndexOf(card));
                messageBlockOwn.Text = string.Format("Own {0} [{1} remaining]", zone, ++(this.ownCount));
                if (treatCountAsOne)
                {
                    if (oppCount < oppCards.Count)
                        messageBlockOpp.Text = string.Format("Opp {0} [{1} remaining]", zone, ++(this.oppCount));
                }
            }
            else
            {
                oppSelected.Remove(oppCards.IndexOf(card));
                messageBlockOpp.Text = string.Format("Opp {0} [{1} remaining]", zone, ++(this.oppCount));
                if (treatCountAsOne)
                {
                    if (ownCount < ownCards.Count)
                        messageBlockOwn.Text = string.Format("Own {0} [{1} remaining]", zone, ++(this.ownCount));
                }
            }
        }

        public void replaceCancelButtonMessage(string message)
        {
            btnCancel.Content = message;
        }

        public void removeCancelButton()
        {
            grdButtons.Children.Remove(btnCancel);
            btnSelect.Margin = new Thickness(0);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
        }
    }
}
