using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DM___Client.GUIWindows
{
    /// <summary>
    /// Interaction logic for GUICreateDeck.xaml
    /// </summary>
    public partial class GUICreateDeck : Window
    {
        public string deckName { get; set; }
        public GUICreateDeck()
        {
            InitializeComponent();
            deckName = null;
        }

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            if (txtDeckName.Text.Length == 0 || txtDeckName.Text.Length > 15)
                MessageBox.Show("The name must be between 1 and 15 characters long.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            else
            if (txtDeckName.Text[0] == ' ')
                MessageBox.Show("The name cannot begin with space.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            else
            if (!isAlphaNumericSpecial(txtDeckName.Text))
                MessageBox.Show("The name can only contain letters, digits and spaces.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            else
            {
                deckName = txtDeckName.Text;
                Close();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private bool isAlphaNumericSpecial(string text)
        {
            for (int i = 0; i < text.Length; i++)
                if ((text[i] < '0' || text[i] > '9' && text[i] < 'A' || text[i] > 'Z' && text[i] < 'a' || text[i] > 'z') && text[i] != ' ')
                    return false;
            return true;
        }
    }
}
