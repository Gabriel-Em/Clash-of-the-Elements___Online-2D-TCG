using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DM___Client.Controllers
{
    public class RegisterController : Controller
    {
        private GUIWindows.GUIRegister parent;

        public RegisterController(GUIWindows.GUIRegister _parent, Communication _com)
        {
            parent = _parent;
            com = _com;
        }

        public override void commandProcessor(Models.ClientMessage message)
        {
            switch (message.Command)
            {
                case "REGISTERSUCCESSFULL":
                    parent.registerSuccessful();
                    break;
                case "REGISTERFAILED":
                    parent.registerFailed(message.Arguments);
                    break;
                case "DISCONNECTED":
                    parent.disconnected("Connection to server was lost and a log regarding the incident was created and deposited inside 'Logs' in apps home directory.", 0);
                    break;
                case "REMOTEDISCONNECT":
                    parent.disconnected("Your account was logged in from a different location.", -1);
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

            for (int i = 0; i < encription.Length; i++)
                hashedPassword.Append(encription[i].ToString("x2"));

            return hashedPassword.ToString();
        }
    }
}
