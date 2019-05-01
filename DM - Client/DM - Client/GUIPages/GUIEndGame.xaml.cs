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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DM___Client.GUIPages
{
    /// <summary>
    /// Interaction logic for GUIEndGame.xaml
    /// </summary>
    public partial class GUIEndGame : Page
    {
        GUIWindows.GUI parent;

        public GUIEndGame(GUIWindows.GUI parent, bool youHaveWon)
        {
            InitializeComponent();

            this.parent = parent;
            if (youHaveWon)
                loadVictoryScreen();
            else
                loadDefeatScreen();
        }

        private void loadVictoryScreen()
        {
            verdictLabel.Content = "VICTORY";
        }

        private void loadDefeatScreen()
        {
            verdictLabel.Content = "DEFEAT";
        }

        private void btnBackToLobby_Click(object sender, RoutedEventArgs e)
        {
            parent.loadGameLobby();
        }
    }
}
