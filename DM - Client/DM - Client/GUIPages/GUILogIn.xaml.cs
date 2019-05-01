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
        // page controller
        private Controllers.LogInController ctrl;

        // the window that applied this page
        private GUIWindows.GUI parent;

        // timer that updates the title of the window with current server status
        private DispatcherTimer statusTimer = new DispatcherTimer();

        // timer that checks if the server has responded to our erquest to connect
        private DispatcherTimer checkServerResponse = new DispatcherTimer();

        // server communication class
        private Communication com;                                              

        public GUILogIn(GUIWindows.GUI parent_, Communication com_)
        {
            InitializeComponent();

            // attach controller
            ctrl = new Controllers.LogInController(this, com_);

            // attach Window that applied the Page
            parent = parent_;

            // attach Server communication class
            com = com_;                                                         

            // initailize timers
            statusTimer.Interval = new TimeSpan(0, 0, 0, 0, 250);
            statusTimer.Tick += status_Timer_Tick;
            checkServerResponse.Interval = new TimeSpan(0, 0, 0, 0, 250);
            checkServerResponse.Tick += checkServerResponse_Tick;
        }

        // timer that updates the status
        private void status_Timer_Tick(object sender, EventArgs e)
        {
            // update the status from the main Window
            string status = ctrl.getStatus();   
            parent.changeStatus(status);

            if (status == "Connected!" || status.Contains("Server unreachable!"))
            {
                // if we managed to connect, permit the user to use the LogIn Box and start listening to the server
                if (status == "Connected!")
                {
                    spanelLogIn.IsEnabled = true;
                    beginListening();
                }
                else
                {
                    // if the server is unreachable display the "Retry" button
                    btnRetry.Visibility = Visibility.Visible;
                }

                // status "Connected!" or status "Server unreachable!" don't need further status checking
                statusTimer.Stop();
            }
        }

        // timer that checks servers response to our request to connect
        private void checkServerResponse_Tick(object sender, EventArgs e)
        {
            if (ctrl.hasReceivedResponse())
                ctrl.messageProcessor(ctrl.getReceivedResponse());
        }

        // start listening to the server and check its response
        private void beginListening()       
        {
            if (!ctrl.Listening)
                ctrl.beginListen();
            if (!checkServerResponse.IsEnabled)
                checkServerResponse.Start();
        }

        // stop listening to the server and stop checking its response
        private void stopListening()        
        {
            if (ctrl.Listening)
                ctrl.stopListen();
            if (checkServerResponse.IsEnabled)
                checkServerResponse.Stop();
        }

        // start checking the status of the connection and start the tryConnect procedure
        public void tryConnect()            
        {
            statusTimer.Start();
            ctrl.tryConnect();
        }

        // the retry button resets the status and restarts the tryConnect procedure
        private void btnRetry_Click(object sender, RoutedEventArgs e)
        {
            parent.changeStatus("Not connected!");
            tryConnect();
            btnRetry.Visibility = Visibility.Hidden;
        }
        
        // functionalities of the log in button
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

        // fucntionalities of the register button
        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            stopListening();
            GUIWindows.GUIRegister reg = new GUIWindows.GUIRegister(this, com);
            reg.ShowDialog();

            // when coming back from the register window check if you were disconnected during it
            if (reg.wasDisconnected)
                weGotDisconnected(reg.disconnectMessage, reg.disconnectType);
            else
                beginListening();
        }

        public void logInDenied()
        {
            MessageBox.Show("The username or password that you provided were invalid!", "Log In failed...", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            spanelLogIn.IsEnabled = true;
        }

        public void logInPermitted()
        {
            stopListening();

            // switch to the GameLobby page
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

        public void weGotDisconnected(string message, int type)
        {
            stopListening();
            spanelLogIn.IsEnabled = false;
            txtUsername.Clear();
            passwordBox.Clear();
            parent.changeStatus("Not connected!");
            GUIWindows.GUIDisconnected disconnectWindow = new GUIWindows.GUIDisconnected(message, type);
            disconnectWindow.ShowDialog();
            tryConnect();
        }
    }
}