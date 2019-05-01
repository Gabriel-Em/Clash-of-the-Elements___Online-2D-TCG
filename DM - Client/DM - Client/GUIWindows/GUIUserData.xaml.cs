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
    /// Interaction logic for GUIUserData.xaml
    /// </summary>
    public partial class GUIUserData : Window
    {
        public GUIUserData(Models.UserData userData)
        {
            InitializeComponent();
            Username.Text = userData.Username;
            Nickname.Text = userData.NickName;
            Email.Text = userData.Email;
            GamesWonValue.Text = userData.GamesWon.ToString();
            GamesLostValue.Text = userData.GamesLost.ToString();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
