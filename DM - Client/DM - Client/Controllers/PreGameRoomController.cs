using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DM___Client.Models;

namespace DM___Client.Controllers
{
    public class PreGameRoomController : Controller
    {
        private GUIPages.GUIPreGameRoom parent;
        public List<PreGameDeckListElement> Decks;

        public PreGameRoomController(GUIPages.GUIPreGameRoom _parent, Communication _com)
        {
            parent = _parent;
            com = _com;
            loadedData = new List<bool>() { false };
            Decks = new List<PreGameDeckListElement>();
        }

        public override void commandProcessor(ClientMessage message)
        {
            switch (message.Command)
            {
                case "DISCONNECTED":
                    parent.disconnected("Connection to server was lost and a log regarding the incident was created and deposited inside 'Logs' in apps home directory.", 0);
                    break;
                case "REMOTEDISCONNECT":
                    parent.disconnected("Your account was logged in from a different location.", -1);
                    break;
                case "DECKLISTDELIVERED":
                    commandArgumentsToDecks(message.Arguments);
                    parent.loadDecksToGUI();
                    loadedData[0] = true;
                    break;
                default: break;
            }
        }

        private void commandArgumentsToDecks(List<string> commandArguments)
        {
            int id;
            string deckName;

            foreach (string argument in commandArguments)
            {
                string[] splitData = argument.Split(',');
                id = Int32.Parse(splitData[0]);
                deckName = splitData[1];
                Decks.Add(new PreGameDeckListElement(id, deckName));
            }
        }

        public override void loadPageData()
        {
            send(new Models.ClientMessage("GETDECKLIST"));
        }
    }
}