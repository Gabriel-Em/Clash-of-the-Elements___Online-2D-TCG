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
    /// Interaction logic for GUIPeek.xaml
    /// </summary>
    public partial class GUIPeek : Window
    {
        private List<SelectGUI_CardGUIModel> ownCards;
        private List<SelectGUI_CardGUIModel> oppCards;

        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        public GUIPeek(List<CardGUIModel> ownCards, List<CardGUIModel> oppCards, string zone)
        {
            InitializeComponent();

            this.ownCards = new List<SelectGUI_CardGUIModel>();
            this.oppCards = new List<SelectGUI_CardGUIModel>();

            messageBlock.Text = zone;

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
                SelectGUI_CardGUIModel sCard = new SelectGUI_CardGUIModel(cardGUI.Card, this, margin, true);

                if (own)
                {
                    ownCards.Add(sCard);
                    grdSelectOwn.Children.Add(sCard.Border);
                }
                else
                {
                    oppCards.Add(sCard);
                    grdSelectOpp.Children.Add(sCard.Border);
                }
            }
        }

        public void deselectAll()
        {
            foreach(SelectGUI_CardGUIModel scg in ownCards)
            {
                scg.deselect();
            }

            foreach(SelectGUI_CardGUIModel scg in oppCards)
            {
                scg.deselect();
            }
        }

        public void loadCardInfo(Models.Card Card)
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

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
        }
    }
}