using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace DM___Client.Controllers
{
    public class LogInController : Controllers.Controller
    {
        private GUIPages.GUILogIn parent;           // the Page that uses this Controller
        
        public LogInController(GUIPages.GUILogIn _parent, Communication _com)
        {
            parent = _parent;
            com = _com;
        }

        public override void clientCommandProcessor(Models.ClientMessage message)
        {
            switch(message.Command)
            {
                case "LOGINPERMITTED":
                    parent.logInPermitted();
                    break;
                case "LOGINDENIED":
                    parent.logInDenied();
                    break;
                case "DISCONNECTED":
                    parent.weGotDisconnected("Connection to server was lost and a log regarding the incident was created and deposited inside 'Logs' in apps home directory.", 0);
                    break;
                case "REMOTEDISCONNECT":
                    parent.weGotDisconnected("Your account was logged in from a different location.", -1);
                    break;
                case "ALREADYLOGGEDIN":
                    parent.alreadyLoggedIn();
                    break;
                default: break;
            }
        }

        public string stringToMD5Hash(string password)
        {
            MD5CryptoServiceProvider md5csp = new MD5CryptoServiceProvider();
            md5csp.ComputeHash(Encoding.ASCII.GetBytes(password));

            byte[] encription = md5csp.Hash;
            StringBuilder hashedPassword = new StringBuilder();

            for(int i=0;i<encription.Length;i++)
                hashedPassword.Append(encription[i].ToString("x2"));

            return hashedPassword.ToString();
        }
    }
}