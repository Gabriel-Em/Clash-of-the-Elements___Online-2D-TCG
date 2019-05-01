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
    /// Interaction logic for GUIDisconnected.xaml
    /// </summary>
    public partial class GUIDisconnected : Window
    {
        private int type;

        public GUIDisconnected(string message, int type_)
        {
            InitializeComponent();
            messageBlock.Text = "Reason: " + message;
            type = type_;

            // if this was a remote disconnect (which means that this account was logged in somewhere else)
            if (type_ == -1)
                btnReconnect.Content = "Exit";

        }

        private void btnReconnect_Click(object sender, RoutedEventArgs e)
        {
            if (type == -1)
                System.Windows.Application.Current.Shutdown();
            else
                Close();
        }
    }
}
