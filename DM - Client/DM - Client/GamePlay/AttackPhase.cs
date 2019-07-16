using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DM___Client.GUIWindows;
using DM___Client.Models;

namespace DM___Client.GUIPages
{
    public partial class GUIGameRoom : Page
    {
        // Attack Phase

        // load Phase

        public void loadAttackPhase()
        {
            List<CardGUIModel> selectableCards;

            deselectAll();
            updateGameState(true, "Attack phase");

            // notify the server that we entered Attack phase so it can notify our opponent
            ctrl.send(new Models.GameMessage("SETPHASE", ctrl.GameRoomID, new List<string>() { "Attack phase" }));

            selectableCards = new List<CardGUIModel>();
            foreach (CardGUIModel cardGUI in listOwnBattleGround)
            {
                if (cardGUI.Card.hasCompletelyBeenSummoned && !hasEffect(cardGUI.Card, "CannotAttack"))
                    selectableCards.Add(cardGUI);
            }

            loadAttackPhaseButtons(listOppSafeGuardZone.Count == 0);
            setAbleToSelect(1, selectableCards);
        }

        // attack buttons

        private void loadAttackPhaseButtons(bool loadBtnWin)
        {
            actionButtons.Children.Clear();
            if (loadBtnWin)
                actionButtons.Children.Add(btnWin);
            else
                actionButtons.Children.Add(btnAttackSafeguards);
            actionButtons.Children.Add(btnAttackCreatures);
            actionButtons.Children.Add(btnEndTurn);
        }

        private void BtnAttackSafeguards_Click(object sender, RoutedEventArgs e)
        {
            string message;
            int attackTargetCount;

            if (selectedCards.Count != ableToSelectLimit)
                MessageBox.Show("You must select " + ableToSelectLimit.ToString() + " card(s) to attack with.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            else
            {
                GUIWindows.GUISelect guiSelect;

                // find out how many safeguards can selected creature attack
                attackTargetCount = getAttackTargetCount();

                if (attackTargetCount == 0)
                {
                    MessageBox.Show("This creature cannot attack safeguards.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    List<int> selectedSafeguards;
                    int index;

                    // cannot break more safeguards than our opponent has
                    attackTargetCount = Math.Min(attackTargetCount, listOppSafeGuardZone.Count());

                    // create the gui that allows us to select the safeguards that we want to attack
                    message = string.Format("You must select a total of {0} safeguard(s).", attackTargetCount);
                    guiSelect = new GUIWindows.GUISelect(
                        new List<CardGUIModel>(),
                        listOppSafeGuardZone,
                        message,
                        "shield zone",
                        0,
                        attackTargetCount
                        );
                    guiSelect.ShowDialog();

                    if (!guiSelect.wasCanceled)
                    {
                        selectedSafeguards = guiSelect.oppSelected;

                        // notify server of our attack so it can notify our opponent
                        sendAttack(selectedSafeguards, true);

                        index = listOwnBattleGround.IndexOf(selectedCards[0]);
                        ableToSelect.Remove(selectedCards[0]);
                        selectedCards[0].deselect();
                        engageBattleOWN(index);
                    }
                }
            }
        }

        private void BtnAttackCreatures_Click(object sender, RoutedEventArgs e)
        {
            string message;

            if (selectedCards.Count != ableToSelectLimit)
                MessageBox.Show("You must select " + ableToSelectLimit.ToString() + " card(s) to attack with.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            else
            {
                GUIWindows.GUISelect guiSelect;
                List<CardGUIModel> validSelections;
                int index;

                // only creatures that are engaged can be attacked
                validSelections = new List<CardGUIModel>();
                foreach(CardGUIModel cardGUI in listOppBattleGround)
                {
                    if (cardGUI.Card.isEngaged)
                        validSelections.Add(cardGUI);
                }

                if (validSelections.Count == 0)
                    MessageBox.Show("There are no creatures you can attack.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                else
                {
                    message = ("You must select a creature to attack.");
                    guiSelect = new GUIWindows.GUISelect(
                        new List<CardGUIModel>(),
                        validSelections,
                        message,
                        "battle zone",
                        0,
                        1
                        );

                    guiSelect.ShowDialog();
                    if (!guiSelect.wasCanceled)
                    {
                        // translate selected card indexes to listOppBattleGround indexes
                        index = listOppBattleGround.IndexOf(validSelections[guiSelect.oppSelected[0]]);

                        // notify server of our attack so it can notify our opponent
                        sendAttack(new List<int>() { index }, false);

                        // deselect card and engage it
                        index = listOwnBattleGround.IndexOf(selectedCards[0]);
                        ableToSelect.Remove(selectedCards[0]);
                        selectedCards[0].deselect();
                        engageBattleOWN(index);
                    }
                }
            }
        }

        private void BtnWin_Click(object sender, RoutedEventArgs e)
        {
            int index;

            if (selectedCards.Count == 1)
            {
                index = listOwnBattleGround.IndexOf(selectedCards[0]);
                ableToSelect.Remove(selectedCards[0]);
                selectedCards[0].deselect();
                engageBattleOWN(index);

                ctrl.send(new GameMessage(
                    "ATTACKOPPONENT",
                    ctrl.GameRoomID,
                    new List<int>() { index }
                    ));
            }
            else
                MessageBox.Show("You must select a creature to attack with.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        // utility

        private int getAttackTargetCount()
        {
            Card card = selectedCards[0].Card;

            foreach(SpecialEffect se in card.SpecialEffects)
            {
                if (se.Effect == "SafeguardBreaker")
                    return 1 + se.Arguments[0];
            }
            return 1;
        }

        private void sendAttack(List<int> selectedCards, bool attackSafeguards)
        {
            GameMessage gameMessage = new GameMessage();

            if (attackSafeguards)
                gameMessage.Command = "ATTACKSAFEGUARDS";
            else
                gameMessage.Command = "ATTACKCREATURE";

            gameMessage.GameID = ctrl.GameRoomID;

            // the creature that attacks
            gameMessage.intArguments.Add(listOwnBattleGround.IndexOf(this.selectedCards[0]));

            // the target(s)
            foreach(int index in selectedCards)
            {
                gameMessage.intArguments.Add(index);
            }

            ctrl.send(gameMessage);
        }

        // safeguards are being attacked

        public void safeguardsUnderAttack(List<int> intArguments)
        {
            bool youBlocked;
            List<CardGUIModel> defenders = null;
            GUIWindows.GUIDefend gUISelect = null;

            /*
             * intArguments[0] - the attacker
             * intArguments[1..] - the safeguards that are being attacked
             */

            engageBattleOPP(intArguments[0]);

            youBlocked = false;
            if (canBeBlockedBlock(intArguments[0]))
            {
                string message;
                string shields;

                defenders = getOwnDefendersThatCanBlock();
                shields = "";

                for (int i = 1; i < intArguments.Count; i++)
                {
                    if (i < intArguments.Count - 1)
                        shields += string.Format("{0}, ", intArguments[i] + 1);
                    else
                        shields += string.Format("{0}", intArguments[i] + 1);
                }
                message = string.Format("Your opponent attacked the following shields ({0}). Select one defender to block the attack with, or don't block at all.", shields);

                gUISelect = new GUIWindows.GUIDefend(
                    defenders,
                    listOppBattleGround[intArguments[0]].Card,
                    null,
                    message,
                    intArguments.Count - 1
                    );
                gUISelect.ShowDialog();

                // you blocked the attack
                if (gUISelect.SelectedDefender != -1)
                {
                    youBlocked = true;
                }
            }

            if (youBlocked)
            {
                int index;

                index = listOwnBattleGround.IndexOf(defenders[gUISelect.SelectedDefender]);

                engageBattleOWN(index);

                // you modify the target of the attacker to the index of the defender
                intArguments[1] = index;
                sendBattle(intArguments);
                Battle(intArguments, false);
            }
            else
            {
                // you delete the attacker from the list of arguments
                intArguments.RemoveAt(0);
                sendBrokenSafeguards(intArguments);
            }
        }

        public void playerUnderAttack(List<int> intArguments)
        {
            bool youBlocked;
            List<CardGUIModel> defenders = null;
            GUIWindows.GUIDefend gUISelect = null;

            /*
             * intArguments[0] - the attacker
             */

            engageBattleOPP(intArguments[0]);

            youBlocked = false;
            if (canBeBlockedBlock(intArguments[0]))
            {
                string message;

                defenders = getOwnDefendersThatCanBlock();
                message = "Your opponent attacked you directly! Select one defender to block the attack with, or don't block at all.";

                gUISelect = new GUIWindows.GUIDefend(
                    defenders,
                    listOppBattleGround[intArguments[0]].Card,
                    null,
                    message,
                    999
                    );
                gUISelect.ShowDialog();

                // you blocked the attack
                if (gUISelect.SelectedDefender != -1)
                {
                    youBlocked = true;
                }
            }

            if (youBlocked)
            {
                int index;

                index = listOwnBattleGround.IndexOf(defenders[gUISelect.SelectedDefender]);

                engageBattleOWN(index);

                // you add the target of the attacker to the index of the defender
                intArguments.Add(index);
                sendBattle(intArguments);
                Battle(intArguments, false);
            }
            else
            {
                // you lost

                ctrl.send(new GameMessage(
                    "ISURRENDER",
                    ctrl.GameRoomID
                    ));

                loadEndGame(false);
            }
        }

        private bool canBeBlockedBlock(int creatureIndex)
        {
            Card card = listOppBattleGround[creatureIndex].Card;

            if (hasEffect(card, "Slippery"))
                return false;

            if (getOwnDefendersThatCanBlock().Count == 0)
                return false;

            return true;
        }

        private void sendBrokenSafeguards(List<int> intArgs)
        {
            GameMessage gm = new GameMessage();

            gm.Command = "BROKENSAFEGUARDS";
            gm.GameID = ctrl.GameRoomID;
            gm.intArguments = intArgs;

            ctrl.send(gm);
        }

        // whenever your safeguards are broken

        public void yourGuardsBroke(List<int> args)
        {
            int count;
            CardWithGameProperties card;
            GUISafeguardActive guiSafeguardActive;
            GUISafeguardOrder guiSafeguardOrder;
            bool activate;
            List<int> brokenGuardsNumbers;
            List<int> selectedOrder;
            Dictionary<int, int> guards;
            int index;

            /*
             * args[0] - number of safeguards that broke
             * args[1..args[0]] - the index of the broken safeguards
             * args[1 + args[0]..args[0] + args[0]] - cardIDs that were under those safeguards
             */

            count = args[0];
            guards = new Dictionary<int, int>();
            brokenGuardsNumbers = new List<int>();

            for (int i = 1; i <= count; i++)
            {
                brokenGuardsNumbers.Add(listOwnSafeGuardZone[args[i]].ShieldNumber);
                guards.Add(listOwnSafeGuardZone[args[i]].ShieldNumber, args[i + count]);
            }

            // choosing the order in which guards get broken
            if (count > 1)
            {
                guiSafeguardOrder = new GUISafeguardOrder(brokenGuardsNumbers);
                guiSafeguardOrder.ShowDialog();
                selectedOrder = guiSafeguardOrder.selectedOrder;
            }
            else
                selectedOrder = new List<int>() { brokenGuardsNumbers[0] };

            // revealing shields
            foreach(int shieldNumber in selectedOrder)
            {
                card = ctrl.getCardWithGamePropertiesByID(guards[shieldNumber]);
                activate = false;

                if (hasTrigger(card, "SafeguardActive"))
                {
                    guiSafeguardActive = new GUIWindows.GUISafeguardActive(card, shieldNumber);
                    guiSafeguardActive.ShowDialog();
                    activate = guiSafeguardActive.activate;
                }
                else
                {
                    guiSafeguardActive = new GUIWindows.GUISafeguardActive(card, shieldNumber, false);
                    guiSafeguardActive.ShowDialog();
                    activate = guiSafeguardActive.activate;
                }

                if (activate)
                {
                    index = getIndexOfOwnShieldWithNumber(shieldNumber);

                    // animate shield to battle zone
                    animateSafeguardToGroundOwn(index, guards[shieldNumber]);
                    sendSendTo(new List<int>() { index, guards[shieldNumber] }, "OppGuards", "OppGround");

                    // add all special effects to event queue
                    foreach (SpecialEffect se in card.SpecialEffects)
                    {
                        if (se.Trigger == "SafeguardActive")
                            addTriggerEvent(se, card);
                        wait();
                    }
                }
                else
                {
                    index = getIndexOfOwnShieldWithNumber(shieldNumber);

                    animateSafeguardBrokeOWN(index, guards[shieldNumber]);
                    updateInfoBoard("hand", OWN, 1);

                    // notify opponent that they broke this shield

                    GameMessage gm = new GameMessage(
                        "YOUBROKEGUARD",
                        ctrl.GameRoomID,
                        new List<int>() { index });
                    ctrl.send(gm);
                }
            }
        }



        private int getIndexOfOwnShieldWithNumber(int number)
        {
            for (int i = 0; i < listOwnSafeGuardZone.Count; i++)
                if (listOwnSafeGuardZone[i].ShieldNumber == number)
                    return i;
            return -1;
        }

        // whenever you break safeguards

        public void youBrokeGuard(List<int> args)
        {
            // if you're breaking the last shield opponent has
            if (listOppSafeGuardZone.Count == 1)
                loadAttackPhaseButtons(true);

            animateSafeguardBrokeOPP(args[0]);
            updateInfoBoard("hand", OPP, 1);
        }

        // creature is under attack

        public void creatureUnderAttack(List<int> intArguments)
        {
            bool youBlocked;
            List<CardGUIModel> defenders = null;
            GUIWindows.GUIDefend gUISelect = null;

            /*
             * intArguments[0] - the attacker
             * intArguments[1] - the creature that is being attacked
             */

            engageBattleOPP(intArguments[0]);

            youBlocked = false;
            if (canBeBlockedBlock(intArguments[0]))
            {
                string message;

                defenders = getOwnDefendersThatCanBlock();
                if (defenders.Count != 0)
                {
                    message = "Your opponent is attacking your creature. Select one defender to block the attack with, or don't block at all.";

                    gUISelect = new GUIWindows.GUIDefend(
                    defenders,
                    listOppBattleGround[intArguments[0]].Card,
                    listOwnBattleGround[intArguments[1]].Card,
                    message
                    );
                    gUISelect.ShowDialog();

                    // you blocked the attack
                    if (gUISelect.SelectedDefender != -1)
                    {
                        youBlocked = true;
                    }
                }
            }

            if (youBlocked)
            {
                int index;

                index = listOwnBattleGround.IndexOf(defenders[gUISelect.SelectedDefender]);

                engageBattleOWN(index);

                // you modify the target of the attacker to the index of the defender
                intArguments[1] = index;

                //notify the server that 2 creatures will battle
                sendBattle(intArguments);
                Battle(intArguments, false);
            }
            else
            {
                sendBattle(intArguments);
                Battle(intArguments, false);
            }
        }

        private void sendBattle(List<int> arguments)
        {
            GameMessage gameMessage = new GameMessage();

            gameMessage.Command = "BATTLE";
            gameMessage.GameID = ctrl.GameRoomID;
            gameMessage.intArguments = arguments;

            ctrl.send(gameMessage);
        }

        public void Battle(List<int> arguments, bool iAmTheInitiator)
        {
            int ownCreaturePower;
            int oppCreaturePower;

            int ownCreatureIndex;
            int oppCreatureIndex;

            CardWithGameProperties ownCreature;
            CardWithGameProperties oppCreature;

            /*
             * arguments[0] - index of attaker
             * arguments[1] - index of target
             */

            if (iAmTheInitiator)
            {
                ownCreatureIndex = arguments[0];
                oppCreatureIndex = arguments[1];

                ownCreature = listOwnBattleGround[ownCreatureIndex].Card;
                oppCreature = listOppBattleGround[oppCreatureIndex].Card;

                ownCreaturePower = getCreaturePowerWhileAttacking(ownCreature);
                oppCreaturePower = oppCreature.Power;

                if (!oppCreature.isEngaged)
                    engageBattleOPP(oppCreatureIndex);

                // if we win the battle
                if (ownCreaturePower > oppCreaturePower)
                {
                    killCreature(oppCreature, oppCreatureIndex, OPP);

                    // if his creature was poisonous
                    if (hasEffect(oppCreature, "Poisonous") || hasEffect(ownCreature, "OnePunch"))
                    {
                        killCreature(ownCreature, ownCreatureIndex, OWN);
                    }
                }
                else
                if (ownCreaturePower < oppCreaturePower)
                {
                    killCreature(ownCreature, ownCreatureIndex, OWN);
                    if (hasEffect(ownCreature, "Poisonous") || hasEffect(oppCreature, "OnePunch"))
                    {
                        killCreature(oppCreature, oppCreatureIndex, OPP);
                    }
                }
                else
                {
                    killCreature(ownCreature, ownCreatureIndex, OWN);
                    killCreature(oppCreature, oppCreatureIndex, OPP);
                }
            }
            else
            {
                ownCreatureIndex = arguments[1];
                oppCreatureIndex = arguments[0];

                ownCreature = listOwnBattleGround[ownCreatureIndex].Card;
                oppCreature = listOppBattleGround[oppCreatureIndex].Card;

                ownCreaturePower = ownCreature.Power;
                oppCreaturePower = getCreaturePowerWhileAttacking(oppCreature);
                
                if (oppCreaturePower > ownCreaturePower)
                {
                    killCreature(ownCreature, ownCreatureIndex, OWN);
                    if (hasEffect(ownCreature, "Poisonous") || hasEffect(oppCreature, "OnePunch"))
                    {
                        killCreature(oppCreature, oppCreatureIndex, OPP);
                    }
                }
                else
                if (oppCreaturePower < ownCreaturePower)
                {
                    killCreature(oppCreature, oppCreatureIndex, OPP);
                    if (hasEffect(oppCreature, "Poisonous") || hasEffect(ownCreature, "OnePunch"))
                    {
                        killCreature(ownCreature, ownCreatureIndex, OWN);
                    }
                }
                else
                {
                    killCreature(ownCreature, ownCreatureIndex, OWN);
                    killCreature(oppCreature, oppCreatureIndex, OPP);
                }
            }
        }

        private void killCreature(Card card, int cardIndex, bool own)
        {

            if (hasEffect(card, "Unkillable"))
            {
                if (own)
                    animateBattleToHandOwn(cardIndex);
                else
                    animateBattleToHandOpp(cardIndex);
                updateInfoBoard("hand", own, 1);
            }
            else
            {
                if (own)
                    animateBattleToGraveyard(cardIndex, own);
                else
                    animateBattleToGraveyard(cardIndex, own);
                updateInfoBoard("grave", own, 1);
            }
        }
    }
}
