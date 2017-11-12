using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DM___Client.GUIWindows
{
    /// <summary>
    /// Interaction logic for GUIRegister.xaml
    /// </summary>
    /// 
    public partial class GUIRegister : Window
    {
        private GUIPages.GUILogIn parent;
        private Controllers.RegisterController ctrl;
        private DispatcherTimer checkServerResponse = new DispatcherTimer();
        public bool wasDisconnected { get; private set; }
        public string disconnectMessage { get; private set; }
        public int disconnectType { get; private set; }

        public GUIRegister(GUIPages.GUILogIn parent_, Communication com_)
        {
            InitializeComponent();

            wasDisconnected = false;
            parent = parent_;
            ctrl = new Controllers.RegisterController(this, com_);
            checkServerResponse.Interval = new TimeSpan(0, 0, 0, 0, 500);
            checkServerResponse.Tick += checkServerResponse_Tick;
            beginListening();
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

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            if(validateRegisterInfo())
            {
                spanelRegister.IsEnabled = false;
                List<string> commandArguments = new List<string>() { txtUsername.Text, txtNickName.Text, ctrl.stringToMD5Hash(passwordBox.Password), txtEmail.Text };
                ctrl.send(new Models.ClientMessage("REGISTERREQUEST", commandArguments));
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            stopListening();
            this.Close();
        }

        private bool validateRegisterInfo()
        {
            string errorMessage = string.Empty;

            if (txtUsername.Text == string.Empty || txtNickName.Text == string.Empty || txtEmail.Text == string.Empty || passwordBox.Password == string.Empty || confirmPasswordBox.Password == string.Empty)
                errorMessage += "All fields are required!\n";
            if (!isAlphaNumericSpecial(txtUsername.Text))
                errorMessage += "+ Username can only contain letters digits and '_';\n";
            if (txtUsername.Text.Length < 5 || txtUsername.Text.Length > 30)
                errorMessage += "+ Username must be between 5 and 30 characters;\n";
            if (!isAlphaNumericSpecial(txtNickName.Text))
                errorMessage += "+ NickName can only contain letters digits and '_';\n";
            if (txtNickName.Text.Length < 5 || txtNickName.Text.Length > 30)
                errorMessage += "+ NickName must be between 5 and 30 characters;\n";
            if (passwordBox.Password.Length < 8 || passwordBox.Password.Length > 30)
                errorMessage += "+ Password must be between 8 and 30 characters;\n";
            if (passwordBox.Password != confirmPasswordBox.Password)
                errorMessage += "+ The two passwords do not match;\n";
            if (passwordBox.Password.Contains(";") || passwordBox.Password.Contains("|"))
                errorMessage += "+ Password connot contain ';' or '|'\n";
            if (txtEmail.Text.Length > 100)
                errorMessage += "+ Email must be shorter than 100 characters;\n";
            if (txtEmail.Text.Contains(";") || txtEmail.Text.Contains("|") || !isValidEmail(txtEmail.Text))
                errorMessage += "+ Email is not in valid form;\n";

            if (errorMessage != string.Empty)
            {
                MessageBox.Show("Invalid entries:\n\n" + errorMessage, "Warning", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            return true;
        }

        private bool isAlphaNumericSpecial(string text)
        {
            for (int i = 0; i < text.Length; i++)
                if ((text[i] < '0' || text[i] > '9' && text[i] < 'A' || text[i] > 'Z' && text[i] < 'a' || text[i] > 'z') && text[i] != '_')
                    return false;
            return true;
        }

        private bool isValidEmail(string email)
        {
            return Regex.IsMatch(email, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase);
        }

        private void checkServerResponse_Tick(object sender, EventArgs e)
        {
            if (ctrl.hasReceivedResponse())
                ctrl.messageProcessor(ctrl.getReceivedResponse());
        }

        public void registerSuccessful()
        {
            stopListening();
            MessageBox.Show("Register Successfull!", "Success", MessageBoxButton.OK, MessageBoxImage.None);
            this.Close();
        }

        public void registerFailed(List<string> commandArguments)
        {
            string errorMessage = "Invalid entries:\n\n";
            foreach (string invalidEntry in commandArguments)
                errorMessage += "+" + invalidEntry + " is already in use\n";
            MessageBox.Show("Register Failed!\n------------------------------------\n" + errorMessage, "Failed", MessageBoxButton.OK, MessageBoxImage.Information);
            spanelRegister.IsEnabled = true;
        }

        public void disconnected(string message, int type)
        {
            stopListening();
            wasDisconnected = true;
            disconnectMessage = message;
            disconnectType = type;
            this.Close();
        }
    }
}
