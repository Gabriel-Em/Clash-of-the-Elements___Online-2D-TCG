using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using DM___Client.Models;

namespace DM___Client.GUIPages
{
    public partial class GUIGameRoom : Page
    {
        private const int ALL = 23;

        private List<CardGUIModel> getOwnDefendersThatCanBlock()
        {
            List<CardGUIModel> defenders = new List<CardGUIModel>();

            foreach (CardGUIModel cardGUI in listOwnBattleGround)
            {
                if (!cardGUI.Card.isEngaged && hasEffect(cardGUI.Card, "Defender"))
                {
                    defenders.Add(cardGUI);
                }
            }

            return defenders;
        }

        private bool hasEffect(Card card, string effect)
        {
            foreach (SpecialEffect se in card.SpecialEffects)
                if (se.Effect == effect)
                    return true;
            return false;
        }

        private int getCreaturePowerWhileAttacking(Card card)
        {
            foreach (SpecialEffect se in card.SpecialEffects)
            {
                if (se.Effect == "BloodThirst")
                    return (se.Arguments[0]);
            }
            return card.Power;
        }

        private bool hasTrigger(Card card, string trigger)
        {
            foreach (SpecialEffect se in card.SpecialEffects)
            {
                if (se.Trigger == trigger)
                    return true;
            }
            return false;
        }

        private void triggerEffect(SpecialEffect se, CardWithGameProperties card)
        {
            switch (se.Effect)
            {
                case "SendTo":
                    triggerOwnSendTo(se);
                    break;
                case "InstantSummon":
                    card.hasCompletelyBeenSummoned = true;
                    break;
                case "Draw":
                    ctrl.send(new GameMessage("DRAWCARD", ctrl.GameRoomID));
                    break;
            }
        }

        private void triggerOwnSendTo(SpecialEffect se)
        {
            GUIWindows.GUISelect gUISelect;
            List<CardGUIModel> validSelections;
            string selectMessage, friendlyFrom;

            validSelections = new List<CardGUIModel>();

            validSelections.Clear();

            getValidSelections(se, out validSelections, out friendlyFrom);

            if (validSelections.Count > 0)
            {
                gUISelect = null;
                List<int> selectedTargetIndexes;

                // if you must select a number of cards from opponent's mana zone
                if (se.Arguments[0] != ALL)
                {
                    selectMessage = string.Format("You must select a total of {0} card(s) from your {1}.", Math.Min(se.Arguments[0], validSelections.Count), friendlyFrom);

                    gUISelect = new GUIWindows.GUISelect(
                        new List<CardGUIModel>(),
                        validSelections, 
                        selectMessage,
                        "mana zone",
                        0,
                        se.Arguments[0]);
                    gUISelect.removeCancelButton();
                    gUISelect.ShowDialog();

                    selectedTargetIndexes = gUISelect.oppSelected;
                }
                else
                {
                    selectedTargetIndexes = new List<int>();

                    for (int i = 0; i < validSelections.Count; i++)
                        selectedTargetIndexes.Add(i);
                }

                switch (se.TargetTo)
                {
                    case "OppHand":
                        sendToOppHand(se, selectedTargetIndexes);
                        break;
                    case "OwnHand":
                        sendToOwnHand(se, selectedTargetIndexes);
                        break;
                    case "OppGrave":
                        sendToOppGrave(se, selectedTargetIndexes);
                        break;
                    case "OwnGround":
                        sendToOwnGround(se, selectedTargetIndexes);
                        break;
                }
            }
        }

        private void sendToOppHand(SpecialEffect se, List<int> selectedTargetIndexes)
        {
            switch (se.TargetFrom)
            {
                case "OppMana":
                    // notify the server that we triggered a SendTo effect
                    // note: if the opponent is the one that will have their cards sent from a zone to another we must send commands preceeded by "Own" because it's our opponent that will receive them
                    sendSendTo(selectedTargetIndexes, "OwnMana", "OwnHand");

                    foreach (int index in selectedTargetIndexes)
                    {
                        animateManaToHandOpp(index);
                        updateInfoBoard("mana", OPP, -1);
                        updateInfoBoard("hand", OPP, 1);
                    }
                    break;
                case "OppGround":
                    sendSendTo(selectedTargetIndexes, "OwnGround", "OwnHand");
                    foreach (int index in selectedTargetIndexes)
                    {
                        animateBattleToHandOpp(index);
                        updateInfoBoard("hand", OPP, 1);
                    }
                    break;

            }
        }

        private void sendToOwnHand(SpecialEffect se, List<int> selectedTargetIndexes)
        {
            switch (se.TargetFrom)
            {
                case "OwnMana":
                    sendSendTo(selectedTargetIndexes, "OppMana", "OppHand");
                    foreach (int index in selectedTargetIndexes)
                    {
                        animateManaToHandOwn(index);
                        updateInfoBoard("mana", OWN, -1);
                        updateInfoBoard("hand", OWN, 1);
                    }
                    break;
                case "OwnGrave":
                    sendSendTo(selectedTargetIndexes, "OppGrave", "OppHand");
                    foreach (int index in selectedTargetIndexes)
                    {
                        animateGraveyardToHandOwn(index);
                        updateInfoBoard("grave", OWN, -1);
                        updateInfoBoard("hand", OWN, 1);
                    }
                    break;
                case "OwnGround":
                    sendSendTo(selectedTargetIndexes, "OppGround", "OppHand");
                    foreach (int index in selectedTargetIndexes)
                    {
                        animateBattleToHandOwn(index);
                        updateInfoBoard("hand", OWN, 1);
                    }
                    break;
            }
        }

        private void sendToOppGrave(SpecialEffect se, List<int> selectedTargetIndexes)
        {
            switch (se.TargetFrom)
            {
                case "OppGround":
                    sendSendTo(selectedTargetIndexes, "OwnGround", "OwnGrave");
                    foreach (int index in selectedTargetIndexes)
                    {
                        animateBattleToGraveyard(index, OPP);
                        updateInfoBoard("grave", OPP, 1);
                    }
                    break;
            }
        }

        private void sendToOwnGround(SpecialEffect se, List<int> selectedTargetIndexes)
        {
            switch (se.TargetFrom)
            {
                case "OwnGrave":
                    sendSendTo(selectedTargetIndexes, "OppGrave", "OppGround");
                    foreach (int index in selectedTargetIndexes)
                    {
                        animateGraveyardToBattle(index, OWN);
                        updateInfoBoard("grave", OWN, -1);
                    }
                    break;
            }
        }

        private void getValidSelections(SpecialEffect se, out List<CardGUIModel> validSelections, out string friendlyFrom)
        {
            switch (se.TargetFrom)
            {
                case "OwnMana":
                    validSelections = listOwnManaZone;
                    friendlyFrom = "mana zone";
                    break;
                case "OppMana":
                    validSelections = listOppManaZone;
                    friendlyFrom = "opponent's mana zone";
                    break;
                case "OwnGrave":
                    validSelections = listOwnGraveyard;
                    friendlyFrom = "gaveyard";
                    break;
                case "OppGrave":
                    validSelections = listOppGraveyard;
                    friendlyFrom = "opponent's graveyard";
                    break;
                case "OwnGround":
                    validSelections = listOwnBattleGround;
                    friendlyFrom = "battleground";
                    break;
                case "OppGround":
                    validSelections = listOppBattleGround;
                    friendlyFrom = "opponent's battleground";
                    break;
                default:
                    validSelections = new List<CardGUIModel>();
                    friendlyFrom = "";
                    break;
            }
        }

        private void sendSendTo(List<int> arguments, string from, string to)
        {
            GameMessage gameMessage = new GameMessage();

            gameMessage.Command = "SENDTO";
            gameMessage.intArguments = arguments;
            gameMessage.stringArguments.Add(from);
            gameMessage.stringArguments.Add(to);
            gameMessage.GameID = ctrl.GameRoomID;

            ctrl.send(gameMessage);
        }

        public void processOppSendTo(List<int> arguments, string from, string to)
        {
            switch(from)
            {
                case "OwnMana":
                    {
                        switch (to)
                        {
                            case "OwnHand":
                                foreach (int index in arguments)
                                {
                                    updateInfoBoard("mana", OWN, -1);
                                    updateInfoBoard("hand", OWN, 1);
                                    animateManaToHandOwn(index);
                                }
                                break;
                        }
                        break;
                    }
                case "OppMana":
                    {
                        switch (to)
                        {
                            case "OppHand":
                                foreach (int index in arguments)
                                {
                                    updateInfoBoard("mana", OPP, -1);
                                    updateInfoBoard("hand", OPP, 1);
                                    animateManaToHandOpp(index);
                                }
                                break;
                        }
                    }
                    break;
                case "OwnGround":
                    {
                        switch (to)
                        {
                            case "OwnHand":
                                foreach (int index in arguments)
                                {
                                    updateInfoBoard("hand", OWN, 1);
                                    animateBattleToHandOwn(index);
                                }
                                break;
                            case "OwnGrave":
                                foreach (int index in arguments)
                                {
                                    updateInfoBoard("grave", OWN, 1);
                                    animateBattleToGraveyard(index, OWN);
                                }
                                break;
                        }
                    }
                    break;
                case "OppGrave":
                    {
                        switch (to)
                        {
                            case "OppGround":
                                foreach (int index in arguments)
                                {
                                    updateInfoBoard("grave", OPP, -1);
                                    animateGraveyardToBattle(index, OPP);
                                }
                                break;
                            case "OppHand":
                                foreach (int index in arguments)
                                {
                                    updateInfoBoard("grave", OPP, -1);
                                    updateInfoBoard("hand", OPP, 1);
                                    animateGraveyardToHandOpp(index);
                                }
                                break;
                        }
                    }
                    break;
                case "OppGround":
                    {
                        switch (to)
                        {
                            case "OppHand":
                                foreach (int index in arguments)
                                {
                                    updateInfoBoard("hand", OPP, 1);
                                    animateBattleToHandOpp(index);
                                }
                                break;
                        }
                    }
                    break;
            }
        }

        public void processOppDrew()
        {
            updateInfoBoard("hand", OPP, 1);
            animateDrawCardOPP();
        }
    }
}

/*

    Working
Baraq
Cataclysm				            
Cyclonius				            
Ejzif					            
Qroax 					           

    Not working
Flux					            must code
Volta					            must code
Decadence of Life			        must code
Siorvys, Protector of the Forest	can't break 2 shields
Terrane					            must code
Lydia Von Stein				        bring only creatures

    Not tested
Araneidae
Black Plague
Jugguro, The Protector
Murriephan, The Puny
Rockoz
Elemental Burn
Eliondrox
Gulgurd
Hulor


*/
