using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DM___Client.Models;

namespace DM___Client.Controllers
{
    public class GameRoomController : Controller
    {
        private GUIPages.GUIGameRoom parent;

        public int GameRoomID { get; set; }
        public int DeckID { get; set; }
        public List<CardWithGameProperties> InitialHand { get; set; }
        public CardCollection CardCollection { get; set; }

        public GameRoomController(GUIPages.GUIGameRoom _parent, Communication _com, int GameRoomID_, int DeckID_, CardCollection CardCollection_)
        {
            parent = _parent;
            com = _com;
            GameRoomID = GameRoomID_;
            DeckID = DeckID_;
            CardCollection = CardCollection_;
            InitialHand = new List<CardWithGameProperties>();
            loadedData = new List<bool>() { false, false, false };
        }

        public override void gameCommandProcessor(GameMessage message)
        {
            switch (message.Command)
            {
                default: break;
            }
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
                case "DECKSET":
                    loadedData[0] = true;
                    send(new Models.ClientMessage("GETHAND", new List<string>() { GameRoomID.ToString() }));
                    break;
                case "HANDRECEIVED":
                    InitialHand = argumentsToCards(message.Arguments);
                    loadedData[1] = true;
                    send(new Models.ClientMessage("READYTOSTART", new List<string>() { GameRoomID.ToString() }));
                    break;
                case "READYTOGO":
                    loadedData[2] = true;
                    break;
                case "YOURINITTURN":
                    parent.loadManaPhase();
                    break;
                case "OPPINITTURN":
                    parent.updateGameState(false, "Mana phase");
                    break;
                default:
                    break;
            }
        }

        public override void loadPageData()
        {
            send(new ClientMessage("SETDECK", new List<string>() { GameRoomID.ToString(), DeckID.ToString() }));
        }

        private List<CardWithGameProperties> argumentsToCards(List<string> arguments)
        {
            List<CardWithGameProperties> list = new List<CardWithGameProperties>();

            foreach(string argument in arguments)
                list.Add(new CardWithGameProperties(CardCollection.Cards[Int32.Parse(argument)-1]));

            return list;
        }

        public Models.CardWithGameProperties getCardFromHand()
        {
            if (InitialHand.Count > 0)
            {
                Models.CardWithGameProperties card = InitialHand[0];
                InitialHand.RemoveAt(0);
                return card;
            }
            return null;
        }
    }
}
