using DM___Client.Models;
using DM___Client.Models.GUIModels;
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

namespace DM___Client.GUIWindows
{
    /// <summary>
    /// Interaction logic for GUIDefend.xaml
    /// </summary>
    public partial class GUIDefend : Window
    {
        private List<SelectGUI_CardGUIModel> ownDefenders;
        private CardWithGameProperties attacker;
        private CardWithGameProperties target;

        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        public int SelectedDefender { get; set; }

        public GUIDefend(List<CardGUIModel> ownDefenders, CardWithGameProperties attacker, CardWithGameProperties target, string message, int count = 1)
        {
            InitializeComponent();

            this.attacker = attacker;
            this.target = target;

            if (count == 999)
            {
                txtYou.Visibility = Visibility.Visible;
            }
            else if (count != 1)
            {
                txtTarget.Text = string.Format("Target (x{0})", count);
            }

            messageBlock.Text = message;

            this.ownDefenders = new List<SelectGUI_CardGUIModel>();

            cardsToGUI(ownDefenders);
        }

        private void cardsToGUI(List<CardGUIModel> cardList)
        {
            Thickness margin;
            SelectGUI_CardGUIModel card;

            foreach (CardGUIModel cardGUI in cardList)
            {
                if (ownDefenders.Count == 0)
                    margin = new Thickness(10, 0, 0, 0);
                else
                {
                    margin = ownDefenders[ownDefenders.Count - 1].Border.Margin;
                    margin.Left += 75;
                }
                SelectGUI_CardGUIModel sCard = new SelectGUI_CardGUIModel(cardGUI.Card, this, margin);
                this.ownDefenders.Add(sCard);
                grdSelectOwn.Children.Add(sCard.Border);
            }

            card = new SelectGUI_CardGUIModel(attacker, this, new Thickness(0));
            grdAttacker.Children.Add(card.Border);
            if (txtYou.Visibility != Visibility.Visible)
            {
                card = new SelectGUI_CardGUIModel(target, this, new Thickness(0));
                grdTarget.Children.Add(card.Border);
            }
        }

        private void btnDefend_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnDontDefend_Click(object sender, RoutedEventArgs e)
        {
            SelectedDefender = -1;
            Close();
        }

        public int addToSelectedCards(SelectGUI_CardGUIModel card)
        {
            if (card.Card != null)
                loadCardInfo(card.Card);

            if (card.Card != attacker && card.Card != target)
            {
                foreach (SelectGUI_CardGUIModel guiCard in ownDefenders)
                {
                    if (guiCard != card)
                        guiCard.deselect();
                }
                SelectedDefender = ownDefenders.IndexOf(card);
                return 0;
            }
            return -1;
        }

        private void loadCardInfo(Models.Card Card)
        {
            rchCardInfo.Document.Blocks.Clear();

            TextRange tr = new TextRange(rchCardInfo.Document.ContentEnd, rchCardInfo.Document.ContentEnd);
            tr.Text += "Name: ";
            tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
            tr = new TextRange(rchCardInfo.Document.ContentEnd, rchCardInfo.Document.ContentEnd);
            tr.Text += Card.Name;
            tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);
            tr = new TextRange(rchCardInfo.Document.ContentEnd, rchCardInfo.Document.ContentEnd);
            tr.Text = "\nType: ";
            tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
            tr = new TextRange(rchCardInfo.Document.ContentEnd, rchCardInfo.Document.ContentEnd);
            tr.Text = Card.Type;
            tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);
            tr = new TextRange(rchCardInfo.Document.ContentEnd, rchCardInfo.Document.ContentEnd);
            tr.Text = "\nElement: ";
            tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
            tr = new TextRange(rchCardInfo.Document.ContentEnd, rchCardInfo.Document.ContentEnd);
            tr.Text = Card.Element;
            tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);
            tr = new TextRange(rchCardInfo.Document.ContentEnd, rchCardInfo.Document.ContentEnd);
            tr.Text = "\nCost: ";
            tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
            tr = new TextRange(rchCardInfo.Document.ContentEnd, rchCardInfo.Document.ContentEnd);
            tr.Text = Card.Cost.ToString();
            tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);
            if (Card.Race != null)
            {
                tr = new TextRange(rchCardInfo.Document.ContentEnd, rchCardInfo.Document.ContentEnd);
                tr.Text = "\nRace: ";
                tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
                tr = new TextRange(rchCardInfo.Document.ContentEnd, rchCardInfo.Document.ContentEnd);
                tr.Text = Card.Race;
                tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);
            }
            if (Card.Power != -1)
            {
                tr = new TextRange(rchCardInfo.Document.ContentEnd, rchCardInfo.Document.ContentEnd);
                tr.Text = "\nPower: ";
                tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
                tr = new TextRange(rchCardInfo.Document.ContentEnd, rchCardInfo.Document.ContentEnd);
                tr.Text = Card.Power.ToString();
                tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);
            }
            tr = new TextRange(rchCardInfo.Document.ContentEnd, rchCardInfo.Document.ContentEnd);
            tr.Text = "\nText: ";
            tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
            if (Card.Text != null)
            {
                tr = new TextRange(rchCardInfo.Document.ContentEnd, rchCardInfo.Document.ContentEnd);
                tr.Text = Card.Text;
                tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);
            }
            rchCardInfo.ScrollToHome();
        }

        public void removeFromSelectedCards(SelectGUI_CardGUIModel card)
        {
            if (card.Card != null)
                loadCardInfo(card.Card);

            SelectedDefender = -1;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
        }
    }
}
