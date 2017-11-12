using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace DM___Client.GUIPages
{
    /// <summary>
    /// Interaction logic for GUILogIn.xaml
    /// </summary>
    
    public partial class GUILogIn : Page
    {
        private Controllers.LogInController ctrl;
        private GUIWindows.GUI parent;
        private DispatcherTimer statusTimer = new DispatcherTimer();
        private DispatcherTimer checkServerResponse = new DispatcherTimer();
        private Communication com;

        public GUILogIn(GUIWindows.GUI parent_, Communication com_)
        {
            InitializeComponent();
            ctrl = new Controllers.LogInController(this, com_);
            parent = parent_;
            com = com_;
            statusTimer.Interval = new TimeSpan(0, 0, 0, 0, 250);
            statusTimer.Tick += status_Timer_Tick;
            checkServerResponse.Interval = new TimeSpan(0, 0, 0, 0, 250);
            checkServerResponse.Tick += checkServerResponse_Tick;
        }

        private void status_Timer_Tick(object sender, EventArgs e)
        {
            string status = ctrl.getStatus();
            parent.changeStatus(status);

            if (status == "Connected!" || status.Contains("Server unreachable!"))
            {
                if (status == "Connected!")
                {
                    spanelLogIn.IsEnabled = true;
                    beginListening();
                }
                else
                    btnRetry.Visibility = Visibility.Visible;

                statusTimer.Stop();
            }
        }

        private void beginListening()
        {
            if (!ctrl.Listening)
                ctrl.beginListen();
            if (!checkServerResponse.IsEnabled)
                checkServerResponse.Start();
        }

        private void stopListening()
        {
            if (ctrl.Listening)
                ctrl.stopListen();
            if (checkServerResponse.IsEnabled)
                checkServerResponse.Stop();
        }

        public void tryConnect()
        {
            statusTimer.Start();
            ctrl.tryConnect();
        }

        private void btnRetry_Click(object sender, RoutedEventArgs e)
        {
            parent.changeStatus("Not connected!");
            tryConnect();
            btnRetry.Visibility = Visibility.Hidden;
        }

        private void btnLogIn_Click(object sender, RoutedEventArgs e)
        {
            if(txtUsername.Text == string.Empty || passwordBox.Password == string.Empty)
            {
                MessageBox.Show("Username and Password can't be empty!","Warning",MessageBoxButton.OK,MessageBoxImage.Warning);
            }
            else
            {
                List<string> commandArguments = new List<string>() { txtUsername.Text, ctrl.stringToMD5Hash(passwordBox.Password) };
                ctrl.send(new Models.ClientMessage("LOGINREQUEST", commandArguments));
                spanelLogIn.IsEnabled = false;
            }
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            stopListening();
            GUIWindows.GUIRegister reg = new GUIWindows.GUIRegister(this, com);
            reg.ShowDialog();

            if (reg.wasDisconnected)
                disconnected(reg.disconnectMessage, reg.disconnectType);
            else
                beginListening();
        }                

        private void checkServerResponse_Tick(object sender, EventArgs e)
        {
            if (ctrl.hasReceivedResponse())
                ctrl.messageProcessor(ctrl.getReceivedResponse());
        }

        public void logInDenied()
        {
            MessageBox.Show("The username or password that you provided was invalid!", "Log In failed...", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            spanelLogIn.IsEnabled = true;
        }

        public void logInPermitted()
        {
            stopListening();
            parent.loadGameLobby();
        }

        public void alreadyLoggedIn()
        {
            if(MessageBox.Show("The account is already logged in somewhere else. Do you wish to log out the account at that location and log in here?", "Log In failed...", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
            {
                List<string> commandArguments = new List<string>() { txtUsername.Text };
                ctrl.send(new Models.ClientMessage("FORCELOGOUTREQUEST", commandArguments));
                Thread.Sleep(1000);
                commandArguments.Clear();
                commandArguments.Add(txtUsername.Text);
                commandArguments.Add(ctrl.stringToMD5Hash(passwordBox.Password));
                ctrl.send(new Models.ClientMessage("LOGINREQUEST", commandArguments));
            }
            else
                spanelLogIn.IsEnabled = true;
        }

        public void disconnected(string message, int type)
        {
            stopListening();
            spanelLogIn.IsEnabled = false;
            txtUsername.Clear();
            passwordBox.Clear();
            parent.changeStatus("Not connected!");
            btnRetry.Visibility = Visibility.Visible;
            GUIWindows.GUIDisconnected disconnectWindow = new GUIWindows.GUIDisconnected(message, type);
            disconnectWindow.ShowDialog();
        }
    }
}