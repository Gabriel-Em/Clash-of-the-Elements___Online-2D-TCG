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
            loadedDataChecklist = new List<bool>() { false, false, false };
        }

        public override void loadPageData()
        {
            send(new ClientMessage("FETCHDECK", new List<string>() { GameRoomID.ToString(), DeckID.ToString() }));
        }

        public override void gameCommandProcessor(GameMessage message)
        {
            switch (message.Command)
            {
                case "PLAYEDASMANA":
                    processPlayedAsMana(message);
                    break;
                case "SETPHASE":
                    parent.updateGameState(false, message.stringArguments[0]);
                    break;
                case "YOURTURN":
                    parent.startTurn();
                    break;
                case "ROLLON":
                    parent.DrawCards(message.intArguments);
                    parent.addRunMethodEvent(new Animations.Animation(parent.loadManaPhase));
                    break;
                case "YOURECEIVEDCARD":
                    processReceivedCard(message);
                    break;
                case "OPPRECEIVEDCARD":
                    parent.processOppDrew(message);
                    break;
                case "SUMMON":
                    processSummon(message);
                    break;
                case "ATTACKSAFEGUARDS":
                    processAttackSafeguards(message);
                    break;
                case "YOURGUARDSBROKE":
                    parent.yourGuardsBroke(message.intArguments);
                    break;
                case "YOUBROKEGUARD":
                    parent.youBrokeGuard(message.intArguments);
                    break;
                case "ATTACKCREATURE":
                    processAttackCreature(message);
                    break;
                case "ATTACKPLAYER":
                    processAttackPlayer(message);
                    break;
                case "BATTLE":
                    processBattle(message);
                    break;
                case "SENDTO":
                    processSendTo(message);
                    break;
                case "OPPSURRENDERED":
                    send(new ClientMessage("CLOSEROOM", new List<string>() { GameRoomID.ToString() }));
                    parent.loadEndGame(true);
                    break;
                default: break;
            }
        }

        public override void clientCommandProcessor(ClientMessage message)
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
                    loadedDataChecklist[0] = true;
                    send(new Models.ClientMessage("GETHAND", new List<string>() { GameRoomID.ToString() }));
                    break;
                case "HANDRECEIVED":
                    InitialHand = argumentsToCards(message.stringArguments);
                    loadedDataChecklist[1] = true;
                    send(new Models.ClientMessage("READYTOSTART", new List<string>() { GameRoomID.ToString() }));
                    break;
                case "READYTOGO":
                    loadedDataChecklist[2] = true;
                    break;
                case "YOURTURN":
                    parent.updateGameState(true, "Mana phase");
                    break;
                case "OPPTURN":
                    parent.updateGameState(false, "Mana phase");
                    break;
                default:
                    break;
            }
        }

        private List<CardWithGameProperties> argumentsToCards(List<string> arguments)
        {
            List<CardWithGameProperties> list = new List<CardWithGameProperties>();

            foreach(string argument in arguments)
                list.Add(new CardWithGameProperties(CardCollection.Cards[Int32.Parse(argument) - 1]));

            return list;
        }

        public Models.CardWithGameProperties getCardFromInitialHand()
        {
            if (InitialHand.Count > 0)
            {
                Models.CardWithGameProperties card = InitialHand[0];
                InitialHand.RemoveAt(0);
                return card;
            }
            return null;
        }

        public Models.CardWithGameProperties getCardWithGamePropertiesByID(int cardID)
        {
            return new Models.CardWithGameProperties(CardCollection.getCardById(cardID));
        }

        private void processPlayedAsMana(GameMessage message)
        {
            parent.txtOppHand.Text = (Int32.Parse(parent.txtOppHand.Text) - 1).ToString();
            parent.txtOppMana.Text = (Int32.Parse(parent.txtOppMana.Text) + 1).ToString();
            parent.animatePlayAsManaOPP(message.intArguments[0]);
        }

        private void processReceivedCard(GameMessage message)
        {
            parent.DrawCards(message.intArguments);
        }

        private void processSummon(GameMessage message)
        {
            parent.summonOPP(message.intArguments);
        }

        private void processAttackSafeguards(GameMessage message)
        {
            parent.safeguardsUnderAttack(message.intArguments);
        }

        private void processAttackPlayer(GameMessage message)
        {
            parent.playerUnderAttack(message.intArguments);
        }

        private void processAttackCreature(GameMessage message)
        {
            parent.creatureUnderAttack(message.intArguments);
        }

        private void processBattle(GameMessage message)
        {
            parent.Battle(message.intArguments, true);
        }

        private void processSendTo(GameMessage message)
        {
            parent.processOppSendTo(message.intArguments, message.stringArguments[0], message.stringArguments[1]);
        }
    }
}
