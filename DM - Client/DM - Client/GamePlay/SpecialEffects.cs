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
                    switch (se.TargetFrom)
                    {
                        case "OppMana":
                            effectSendToFromOppMana(se);
                            break;
                        case "OwnMana":
                            effectSendToFromOwnMana(se);
                            break;
                        case "OppGround":
                            effectSendToFromOppGround(se);
                            break;
                        case "OwnGrave":
                            effectSendToFromOwnGrave(se);
                            break;
                        case "OwnGround":
                            effectSendToFromOwnGround(se);
                            break;
                    }
                    break;
                case "InstantSummon":
                    card.hasCompletelyBeenSummoned = true;
                    break;
                case "Draw":
                    ctrl.send(new GameMessage("DRAWCARD", ctrl.GameRoomID));
                    break;
            }
        }

        private void effectSendToFromOppMana(SpecialEffect se)
        {
            GUIWindows.GUISelect gUISelect;
            List<CardGUIModel> validSelections;
            string message;

            validSelections = new List<CardGUIModel>();

            validSelections.Clear();
            validSelections = listOppManaZone;

            if (validSelections.Count > 0)
            {
                gUISelect = null;
                List<int> selectedTargetIndexes;

                // if you must select a number of cards from opponent's mana zone
                if (se.Arguments[0] != ALL)
                {
                    message = string.Format("You must select a total of {0} card(s) from your opponent's mana zone.", Math.Min(se.Arguments[0], validSelections.Count));

                    gUISelect = new GUIWindows.GUISelect(validSelections, message, se.Arguments[0], null);
                    gUISelect.removeCancelButton();
                    gUISelect.ShowDialog();

                    selectedTargetIndexes = gUISelect.selected;
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
                        foreach (int index in selectedTargetIndexes)
                            animateManaToHandOpp(index);
                        
                        // notify the server that we triggered a SendTo effect
                        // note: if the opponent is the one that will have their cards sent from a zone to another we must send commands preceeded by "Own" because it's our opponent that will receive them
                        sendSendTo(selectedTargetIndexes, "OwnMana", "OwnHand");
                        break;
                }
            }
        }

        private void effectSendToFromOwnMana(SpecialEffect se)
        {
            GUIWindows.GUISelect gUISelect;
            List<CardGUIModel> validSelections;
            string message;

            validSelections = new List<CardGUIModel>();

            validSelections.Clear();
            validSelections = listOwnManaZone;

            if (validSelections.Count > 0)
            {
                gUISelect = null;
                List<int> selectedTargetIndexes;

                if (se.Arguments[0] != ALL)
                {
                    message = string.Format("You must select a total of {0} card(s) from your mana zone.", Math.Min(se.Arguments[0], validSelections.Count));

                    gUISelect = new GUIWindows.GUISelect(validSelections, message, se.Arguments[0], null);
                    gUISelect.removeCancelButton();
                    gUISelect.ShowDialog();

                    selectedTargetIndexes = gUISelect.selected;
                }
                else
                {
                    selectedTargetIndexes = new List<int>();

                    for (int i = 0; i < validSelections.Count; i++)
                        selectedTargetIndexes.Add(i);
                }

                switch (se.TargetTo)
                {
                    case "OwnHand":
                        foreach (int index in selectedTargetIndexes)
                            animateManaToHandOwn(index);
                        sendSendTo(selectedTargetIndexes, "OppMana", "OppHand");
                        break;
                }
            }
        }

        private void effectSendToFromOppGround(SpecialEffect se)
        {
            GUIWindows.GUISelect gUISelect;
            List<CardGUIModel> validSelections;
            string message;

            validSelections = new List<CardGUIModel>();

            validSelections.Clear();
            validSelections = listOppBattleGround;

            if (validSelections.Count > 0)
            {
                gUISelect = null;
                List<int> selectedTargetIndexes;

                if (se.Arguments[0] != ALL)
                {
                    message = string.Format("You must select a total of {0} card(s) from your opponent's battleground.", Math.Min(se.Arguments[0], validSelections.Count));

                    gUISelect = new GUIWindows.GUISelect(validSelections, message, se.Arguments[0], null);
                    gUISelect.removeCancelButton();
                    gUISelect.ShowDialog();

                    selectedTargetIndexes = gUISelect.selected;
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
                        foreach (int index in selectedTargetIndexes)
                            animateBattleToHandOpp(index);
                        sendSendTo(selectedTargetIndexes, "OwnGround", "OwnHand");
                        break;
                    case "OppGrave":
                        foreach (int index in selectedTargetIndexes)
                            animateBattleToGraveyard(index, false);
                        sendSendTo(selectedTargetIndexes, "OwnGround", "OwnGrave");
                        break;
                }
            }
        }

        private void effectSendToFromOwnGrave(SpecialEffect se)
        {
            GUIWindows.GUISelect gUISelect;
            List<CardGUIModel> validSelections;
            string message;

            validSelections = new List<CardGUIModel>();

            validSelections.Clear();
            validSelections = listOwnGraveyard;

            if (validSelections.Count > 0)
            {
                gUISelect = null;
                List<int> selectedTargetIndexes;

                if (se.Arguments[0] != ALL)
                {
                    message = string.Format("You must select a total of {0} card(s) from your own graveyard.", Math.Min(se.Arguments[0], validSelections.Count));

                    gUISelect = new GUIWindows.GUISelect(validSelections, message, se.Arguments[0], null);
                    gUISelect.removeCancelButton();
                    gUISelect.ShowDialog();

                    selectedTargetIndexes = gUISelect.selected;
                }
                else
                {
                    selectedTargetIndexes = new List<int>();

                    for (int i = 0; i < validSelections.Count; i++)
                        selectedTargetIndexes.Add(i);
                }

                switch (se.TargetTo)
                {
                    case "OwnGround":
                        foreach (int index in selectedTargetIndexes)
                        {
                            animateGraveyardToBattle(index, true);
                        }
                        sendSendTo(selectedTargetIndexes, "OppGrave", "OppGround");
                        break;
                    case "OwnHand":
                        foreach (int index in selectedTargetIndexes)
                        {
                            animateGraveyardToHandOwn(index);
                        }
                        sendSendTo(selectedTargetIndexes, "OppGrave", "OppHand");
                        break;
                }
            }
        }

        private void effectSendToFromOwnGround(SpecialEffect se)
        {
            GUIWindows.GUISelect gUISelect;
            List<CardGUIModel> validSelections;
            string message;

            validSelections = new List<CardGUIModel>();

            validSelections.Clear();
            validSelections = listOwnBattleGround;

            if (validSelections.Count > 0)
            {
                gUISelect = null;
                List<int> selectedTargetIndexes;

                if (se.Arguments[0] != ALL)
                {
                    message = string.Format("You must select a total of {0} card(s) from your own battleground.", Math.Min(se.Arguments[0], validSelections.Count));

                    gUISelect = new GUIWindows.GUISelect(validSelections, message, se.Arguments[0], null);
                    gUISelect.removeCancelButton();
                    gUISelect.ShowDialog();
                    selectedTargetIndexes = gUISelect.selected;
                }
                else
                {
                    selectedTargetIndexes = new List<int>();

                    for (int i = 0; i < validSelections.Count; i++)
                        selectedTargetIndexes.Add(i);
                }
                
                switch (se.TargetTo)
                {
                    case "OwnHand":
                        foreach (int index in selectedTargetIndexes)
                            animateBattleToHandOwn(index);
                        sendSendTo(selectedTargetIndexes, "OppGround", "OppHand");
                        break;
                }
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

        public void processSendTo(List<int> arguments, string from, string to)
        {
            switch(from)
            {
                case "OwnMana":
                    {
                        switch (to)
                        {
                            case "OwnHand":
                                foreach (int index in arguments)
                                    animateManaToHandOwn(index);
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
                                    animateManaToHandOpp(index);
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
                                    animateBattleToHandOwn(index);
                                break;
                            case "OwnGrave":
                                foreach (int index in arguments)
                                    animateBattleToGraveyard(index, true);
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
                                    animateGraveyardToBattle(index, false);
                                break;
                            case "OppHand":
                                foreach (int index in arguments)
                                    animateGraveyardToHandOpp(index);
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
                                    animateBattleToHandOpp(index);
                                break;
                        }
                    }
                    break;
            }
        }

        public void processOppDrew()
        {
            animateDrawCardOPP();
        }
    }
}
