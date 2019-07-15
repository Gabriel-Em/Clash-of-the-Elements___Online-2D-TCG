using DM___Client.Models;
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
    /// Interaction logic for GUISafeguardOrder.xaml
    /// </summary>
    public partial class GUISafeguardOrder : Window
    {
        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private List<int> shieldNumbers;
        public List<int> selectedOrder;
        public GUISafeguardOrder(List<int> brokenGuards)
        {
            string shields;

            InitializeComponent();

            shieldNumbers = brokenGuards;
            shieldNumbers.Sort();

            shields = "";
            for (int i = 0; i < shieldNumbers.Count; i++)
            {
                if (i < shieldNumbers.Count - 1)
                    shields += string.Format("{0}, ", shieldNumbers[i]);
                else
                    shields += string.Format("{0}", shieldNumbers[i]);
            }
            messageBlock.Text = string.Format("Your opponent broke the following shields ({0}). Select the order in which you want them revealed. [numbers and spaces only!]", shields);
        }

        private bool isNumericSpecial(string text)
        {
            for (int i = 0; i < text.Length; i++)
                if ((text[i] < '0' || text[i] > '9') && text[i] != ' ')
                    return false;
            return true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            string[] selection;
            int shieldNumber;

            selectedOrder = new List<int>();

            if (!isNumericSpecial(txtSelected.Text))
            {
                MessageBox.Show("Invalid selection!", "Warning", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            selection = txtSelected.Text.Split(' ');

            foreach (string selected in selection)
            {
                if (selected.Length > 0)
                {
                    shieldNumber = Int32.Parse(selected);
                    if (!shieldNumbers.Contains(shieldNumber))
                    {
                        MessageBox.Show("Invalid selection!", "Warning", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    else
                        selectedOrder.Add(shieldNumber);
                }
            }

            if (selectedOrder.Count != shieldNumbers.Count)
            {
                MessageBox.Show("Invalid selection!", "Warning", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Close();
        }
    }
}
