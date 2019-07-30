using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using DM___Client.Animations;
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
            int power = card.Power;
            foreach (SpecialEffect se in card.SpecialEffects)
            {
                switch (se.Effect)
                {
                    case "BloodThirst":
                        power += se.Arguments[0];
                        break;
                    case "BloodThirstRevenge":
                        if (se.Condition == "OwnGraveyardCivilization")
                        {
                            int bonus;
                            string civilization;

                            bonus = se.Arguments[0];
                            civilization = codeToCivilization(se.Arguments[1]);

                            foreach(CardGUIModel graveyardCard in listOwnGraveyard)
                            {
                                if (graveyardCard.Card.Element == civilization)
                                    power += bonus;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            return power;
        }

        private string codeToCivilization(int code)
        {
            switch(code)
            {
                case 1:
                    return "Fire";
                case 2:
                    return "Water";
                case 3:
                    return "Nature";
                case 4:
                    return "Darkness";
                case 5:
                    return "Light";
            }
            return "";
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
                    ctrl.send(new GameMessage("DRAWCARD", ctrl.GameRoomID, se.Arguments));
                    break;
            }
        }

        private void triggerOwnSendTo(SpecialEffect se)
        {
            GUIWindows.GUISelect gUISelect;
            Tuple<List<CardGUIModel>, List<CardGUIModel>> tuple;
            string selectMessage, friendlyFrom;
            bool treatCountAsOne;

            tuple = getValidSelections(se, out friendlyFrom);
            treatCountAsOne = se.TargetFrom.Contains("Any") ? true : false;

            if (tuple.Item1.Count > 0 || tuple.Item2.Count > 0)
            {
                gUISelect = null;
                List<int> selectedTargetIndexesOwn, selectedTargetIndexesOpp;

                // if you must select a number of cards from opponent's mana zone
                if (se.Arguments[0] != ALL)
                {
                    selectMessage = "";
                    if (tuple.Item1.Count > 0)
                    {
                        if (tuple.Item2.Count > 0)
                        {
                            selectMessage = string.Format("You must select a total of {0} cards from your {3} {1} {2} cards from your opponent's {3}.",
                                treatCountAsOne ? Math.Min(tuple.Item1.Count + tuple.Item1.Count, se.Arguments[0]) : Math.Min(tuple.Item1.Count, se.Arguments[0]),
                                treatCountAsOne ? "or" : "and",
                                treatCountAsOne ? Math.Min(tuple.Item1.Count + tuple.Item1.Count, se.Arguments[0]) : Math.Min(tuple.Item2.Count, se.Arguments[0]),
                                friendlyFrom);
                        }
                        else
                        {
                            selectMessage = string.Format("You must select a total of {0} card(s) from your {1}.",
                                Math.Min(se.Arguments[0], tuple.Item1.Count),
                                friendlyFrom);
                        }
                    }
                    else
                    {
                        if (tuple.Item2.Count > 0)
                        {
                            selectMessage = string.Format("You must select a total of {0} card(s) from your opponent's {1}.",
                                Math.Min(se.Arguments[0], tuple.Item2.Count),
                                friendlyFrom);
                        }
                    }
                    gUISelect = new GUIWindows.GUISelect(
                        tuple.Item1,
                        tuple.Item2, 
                        selectMessage,
                        friendlyFrom,
                        tuple.Item1.Count == 0 ? 0 : Math.Min(tuple.Item1.Count, se.Arguments[0]),
                        tuple.Item2.Count == 0 ? 0 : Math.Min(tuple.Item2.Count, se.Arguments[0]),
                        se.TargetFrom.Contains("Any") ? true : false);
                    gUISelect.removeCancelButton();
                    gUISelect.ShowDialog();

                    selectedTargetIndexesOwn = gUISelect.ownSelected;
                    selectedTargetIndexesOpp = gUISelect.oppSelected;
                }
                else
                {
                    selectedTargetIndexesOwn = new List<int>();
                    selectedTargetIndexesOpp = new List<int>();

                    for (int i = 0; i < tuple.Item1.Count; i++)
                        selectedTargetIndexesOwn.Add(i);
                    for (int i = 0; i < tuple.Item2.Count; i++)
                        selectedTargetIndexesOpp.Add(i);
                }

                switch (se.TargetTo)
                {
                    case "OppHand":
                        sendToOppHand(se, selectedTargetIndexesOwn, selectedTargetIndexesOpp);
                        break;
                    case "OwnHand":
                        sendToOwnHand(se, selectedTargetIndexesOwn, selectedTargetIndexesOpp);
                        break;
                    case "OwnGrave":
                        sendToOwnGrave(se, selectedTargetIndexesOwn, selectedTargetIndexesOpp);
                        break;
                    case "OppGrave":
                        sendToOppGrave(se, selectedTargetIndexesOwn, selectedTargetIndexesOpp);
                        break;
                    case "OwnGround":
                        sendToOwnGround(se, selectedTargetIndexesOwn, selectedTargetIndexesOpp);
                        break;
                    case "OwnersHand":
                        sendToOwnersHand(se, selectedTargetIndexesOwn, selectedTargetIndexesOpp);
                        break;
                }
            }
        }

        private void sendToOwnersHand(SpecialEffect se, List<int> selectedTargetIndexesOwn, List<int> selectedTargetIndexesOpp)
        {
            switch (se.TargetFrom)
            {
                case "AnyGround":
                    // notify the server that we triggered a SendTo effect
                    // note: if the opponent is the one that will have their cards sent from a zone to another we must send commands preceeded by "Own" because it's our opponent that will receive them

                    if (selectedTargetIndexesOwn.Count > 0)
                    {
                        sendSendTo(selectedTargetIndexesOwn, "OppGround", "OppHand");

                        foreach (int index in selectedTargetIndexesOwn)
                        {
                            animateBattleToHandOwn(index);
                            updateInfoBoard("ground", OWN, -1);
                            updateInfoBoard("hand", OWN, 1);
                        }
                    }

                    if (selectedTargetIndexesOpp.Count > 0)
                    {
                        sendSendTo(selectedTargetIndexesOpp, "OwnGround", "OwnHand");

                        foreach (int index in selectedTargetIndexesOpp)
                        {
                            animateBattleToHandOpp(index);
                            updateInfoBoard("ground", OPP, -1);
                            updateInfoBoard("hand", OPP, 1);
                        }
                    }
                    break;
            }
        }

        private void sendToOwnGrave(SpecialEffect se, List<int> selectedTargetIndexesOwn, List<int> selectedTargetIndexesOpp)
        {
            switch (se.TargetFrom)
            {
                case "OwnMana":
                    // notify the server that we triggered a SendTo effect
                    // note: if the opponent is the one that will have their cards sent from a zone to another we must send commands preceeded by "Own" because it's our opponent that will receive them

                    if (selectedTargetIndexesOwn.Count > 0)
                    {
                        sendSendTo(selectedTargetIndexesOwn, "OppMana", "OppGrave");

                        foreach (int index in selectedTargetIndexesOwn)
                        {
                            animateManaToGraveOwn(index);
                            updateInfoBoard("mana", OWN, -1);
                            updateInfoBoard("grave", OWN, 1);
                        }
                    }
                    break;
                case "OwnGround":
                    // notify the server that we triggered a SendTo effect
                    // note: if the opponent is the one that will have their cards sent from a zone to another we must send commands preceeded by "Own" because it's our opponent that will receive them

                    if (selectedTargetIndexesOwn.Count > 0)
                    {
                        sendSendTo(selectedTargetIndexesOwn, "OppGround", "OppGrave");

                        foreach (int index in selectedTargetIndexesOwn)
                        {
                            animateBattleToGraveyard(index, true);
                            updateInfoBoard("grave", OWN, 1);
                        }
                    }
                    break;
            }
        }

        private void sendToOppHand(SpecialEffect se, List<int> selectedTargetIndexesOwn, List<int> selectedTargetIndexesOpp)
        {
            switch (se.TargetFrom)
            {
                case "OppMana":
                    // notify the server that we triggered a SendTo effect
                    // note: if the opponent is the one that will have their cards sent from a zone to another we must send commands preceeded by "Own" because it's our opponent that will receive them
                    sendSendTo(selectedTargetIndexesOpp, "OwnMana", "OwnHand");

                    foreach (int index in selectedTargetIndexesOpp)
                    {
                        animateManaToHandOpp(index);
                        updateInfoBoard("mana", OPP, -1);
                        updateInfoBoard("hand", OPP, 1);
                    }
                    break;
                case "OppGround":
                    sendSendTo(selectedTargetIndexesOpp, "OwnGround", "OwnHand");
                    foreach (int index in selectedTargetIndexesOpp)
                    {
                        animateBattleToHandOpp(index);
                        updateInfoBoard("hand", OPP, 1);
                    }
                    break;

            }
        }

        private void sendToOwnHand(SpecialEffect se, List<int> selectedTargetIndexesOwn, List<int> selectedTargetIndexesOpp)
        {
            switch (se.TargetFrom)
            {
                case "OwnMana":
                    sendSendTo(selectedTargetIndexesOwn, "OppMana", "OppHand");
                    foreach (int index in selectedTargetIndexesOwn)
                    {
                        animateManaToHandOwn(index);
                        updateInfoBoard("mana", OWN, -1);
                        updateInfoBoard("hand", OWN, 1);
                    }
                    break;
                case "OwnGrave":
                    sendSendTo(selectedTargetIndexesOwn, "OppGrave", "OppHand");
                    foreach (int index in selectedTargetIndexesOwn)
                    {
                        animateGraveyardToHandOwn(index);
                        updateInfoBoard("grave", OWN, -1);
                        updateInfoBoard("hand", OWN, 1);
                    }
                    break;
                case "OwnGround":
                    sendSendTo(selectedTargetIndexesOwn, "OppGround", "OppHand");
                    foreach (int index in selectedTargetIndexesOwn)
                    {
                        animateBattleToHandOwn(index);
                        updateInfoBoard("hand", OWN, 1);
                    }
                    break;
            }
        }

        private void sendToOppGrave(SpecialEffect se, List<int> selectedTargetIndexesOwn, List<int> selectedTargetIndexesOpp)
        {
            switch (se.TargetFrom)
            {
                case "OppGround":
                    sendSendTo(selectedTargetIndexesOpp, "OwnGround", "OwnGrave");
                    foreach (int index in selectedTargetIndexesOpp)
                    {
                        animateBattleToGraveyard(index, OPP);
                        updateInfoBoard("grave", OPP, 1);
                    }
                    break;
            }
        }

        private void sendToOwnGround(SpecialEffect se, List<int> selectedTargetIndexesOwn, List<int> selectedTargetIndexesOpp)
        {
            switch (se.TargetFrom)
            {
                case "OwnGrave":
                    sendSendTo(selectedTargetIndexesOwn, "OppGrave", "OppGround");
                    foreach (int index in selectedTargetIndexesOwn)
                    {
                        animateGraveyardToBattle(index, OWN);
                        updateInfoBoard("grave", OWN, -1);
                    }
                    break;
            }
        }

        private Tuple<List<CardGUIModel>, List<CardGUIModel>> getValidSelections(SpecialEffect se, out string friendlyFrom)
        {
            Tuple<List<CardGUIModel>, List<CardGUIModel>> tuple;

            switch (se.TargetFrom)
            {
                case "OwnMana":
                    tuple = new Tuple<List<CardGUIModel>, List<CardGUIModel>>(listOwnManaZone, new List<CardGUIModel>());
                    friendlyFrom = "mana zone";
                    break;
                case "OppMana":
                    tuple = new Tuple<List<CardGUIModel>, List<CardGUIModel>>(new List<CardGUIModel>(), listOppManaZone);
                    friendlyFrom = "mana zone";
                    break;
                case "OwnGrave":
                    tuple = new Tuple<List<CardGUIModel>, List<CardGUIModel>>(listOwnGraveyard, new List<CardGUIModel>());
                    friendlyFrom = "gaveyard";
                    break;
                case "OppGrave":
                    tuple = new Tuple<List<CardGUIModel>, List<CardGUIModel>>(new List<CardGUIModel>(), listOppGraveyard);
                    friendlyFrom = "graveyard";
                    break;
                case "OwnGround":
                    tuple = new Tuple<List<CardGUIModel>, List<CardGUIModel>>(listOwnBattleGround, new List<CardGUIModel>());
                    friendlyFrom = "battleground";
                    break;
                case "OppGround":
                    tuple = new Tuple<List<CardGUIModel>, List<CardGUIModel>>(new List<CardGUIModel>(), listOppBattleGround);
                    friendlyFrom = "battleground";
                    break;
                case "AnyGround":
                    tuple = new Tuple<List<CardGUIModel>, List<CardGUIModel>>(listOwnBattleGround, listOppBattleGround);
                    friendlyFrom = "battleground";
                    break;
                default:
                    tuple = new Tuple<List<CardGUIModel>, List<CardGUIModel>>(new List<CardGUIModel>(), new List<CardGUIModel>());
                    friendlyFrom = "";
                    break;
            }
            return tuple;
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
                            case "OppGrave":
                                foreach (int index in arguments)
                                {
                                    updateInfoBoard("mana", OPP, -1);
                                    updateInfoBoard("grave", OPP, 1);
                                    animateManaToGraveOpp(index);
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
                            case "OppGrave":
                                foreach (int index in arguments)
                                {
                                    updateInfoBoard("grave", OPP, 1);
                                    animateBattleToGraveyard(index, false);
                                }
                                break;
                        }
                    }
                    break;
                case "OppGuards":
                    {
                        switch(to)
                        {
                            case "OppGround":
                                animateSafeguardToGroundOpp(arguments[0], arguments[1]);
                                break;
                        }
                    }
                    break;
            }
        }

        public void DrawCards(List<int> arguments)
        {
            CardWithGameProperties card;
            //List<Event> events;

            //events = new List<Event>();

            for (int i = 0; i < arguments.Count; i++)
            {
                card = new CardWithGameProperties(ctrl.getCardWithGamePropertiesByID(arguments[i]));

                animateDrawCardOWN(card);
                //events.Add(new Event(new Animations.AlignAnimation(listHand, AnimationAndEventsConstants.handInitialPosition, AnimationAndEventsConstants.handAlignPace)));

                // if we drew a card during our own Summon phase we add it to the ableToSelect list
                if (Phase == "Summon phase" && itIsOwnTurn)
                    ableToSelect.Add(listHand[listHand.Count - 1]);

                // we update the info board
                updateInfoBoard("hand", OWN, 1);
                updateInfoBoard("deck", OWN, -1);
            }
            //addEvents(events);
        }

        public void processOppDrew(GameMessage message)
        {
            for (int i = 0; i < message.intArguments[0]; i++)
            {
                updateInfoBoard("hand", OPP, 1);
                updateInfoBoard("deck", OPP, -1);
                animateDrawCardOPP();
            }
        }
    }
}