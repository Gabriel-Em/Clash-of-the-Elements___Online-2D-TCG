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
    /// Interaction logic for GUISafeguardActive.xaml
    /// </summary>
    public partial class GUISafeguardActive : Window
    {
        private List<SelectGUI_CardGUIModel> ownDefenders;

        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        public int SelectedDefender { get; set; }
        public GUISafeguardActive(List<CardGUIModel> ownDefenders, Card attacker, Card target, string message, int count = 1)
        {

            InitializeComponent();

            //if (count == 999)
            //{
            //    txtYou.Visibility = Visibility.Visible;
            //}
            //else if (count != 1)
            //{
            //    txtTarget.Text = string.Format("Target (x{0})", count);
            //}

            //messageBlock.Text = message;

            //this.ownDefenders = new List<SelectGUI_CardGUIModel>();

            //cardsToGUI(ownDefenders, attacker, target);
        }

        private void cardsToGUI(List<CardGUIModel> cardList, Card attacker, Card target)
        {
            //Thickness margin;

            //foreach (CardGUIModel cardGUI in cardList)
            //{
            //    if (ownDefenders.Count == 0)
            //        margin = new Thickness(10, 0, 0, 0);
            //    else
            //    {
            //        margin = ownDefenders[ownDefenders.Count - 1].Border.Margin;
            //        margin.Left += 75;
            //    }
            //    SelectGUI_CardGUIModel sCard = new SelectGUI_CardGUIModel(cardGUI, this, margin);
            //    this.ownDefenders.Add(sCard);
            //    grdSelectOwn.Children.Add(sCard.Border);
            //}
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {

            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            SelectedDefender = -1;
            Close();
        }

        //public int addToSelectedCards(SelectGUI_CardGUIModel card)
        //{
        //    foreach (SelectGUI_CardGUIModel guiCard in ownDefenders)
        //    {
        //        if (guiCard != card)
        //            guiCard.deselect();
        //    }
        //    SelectedDefender = ownDefenders.IndexOf(card);
        //    return 0;
        //}

        public void removeFromSelectedCards(SelectGUI_CardGUIModel card)
        {
            SelectedDefender = -1;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
        }
    }
}
