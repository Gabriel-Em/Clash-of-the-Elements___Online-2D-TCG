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
            if (type_ == -1)
                button.Content = "Exit";

        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (type == -1)
                System.Windows.Application.Current.Shutdown();
            else
                Close();

        }
    }
}
