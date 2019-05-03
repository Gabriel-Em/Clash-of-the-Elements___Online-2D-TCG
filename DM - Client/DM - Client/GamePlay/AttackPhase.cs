using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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

            loadAttackPhaseButtons(listOppSafeguardZone.Count == 0);
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
                    attackTargetCount = Math.Min(attackTargetCount, listOppSafeguardZone.Count());

                    // create the gui that allows us to select the safeguards that we want to attack
                    message = string.Format("You must select a total of {0} safeguard(s).", attackTargetCount);
                    guiSelect = new GUIWindows.GUISelect(listOppSafeguardZone, message, attackTargetCount, null);
                    guiSelect.ShowDialog();

                    if (!guiSelect.wasCanceled)
                    {
                        selectedSafeguards = guiSelect.selected;

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
                    guiSelect = new GUIWindows.GUISelect(validSelections, message, 1, null);

                    guiSelect.ShowDialog();
                    if (!guiSelect.wasCanceled)
                    {
                        // translate selected card indexes to listOppBattleGround indexes
                        index = listOppBattleGround.IndexOf(validSelections[guiSelect.selected[0]]);

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
            GUIWindows.GUISelect gUISelect = null;

            /*
             * intArguments[0] - the attacker
             * intArguments[1..] - the safeguards that are being attacked
             */

            engageBattleOPP(intArguments[0]);

            youBlocked = false;
            if (canBlock(intArguments[0]))
            {
                string message;

                defenders = getOwnDefendersThatCanBlock();
                message = "Select one defender to block the attack with, or don't block at all.";

                gUISelect = new GUIWindows.GUISelect(defenders, message, 1, null);
                gUISelect.replaceCancelButtonMessage("Don't defend");
                gUISelect.ShowDialog();

                // you blocked the attack
                if (!gUISelect.wasCanceled)
                {
                    youBlocked = true;
                }
            }

            if (youBlocked)
            {
                int index;

                index = listOwnBattleGround.IndexOf(defenders[gUISelect.selected[0]]);

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
            GUIWindows.GUISelect gUISelect = null;

            /*
             * intArguments[0] - the attacker
             */

            engageBattleOPP(intArguments[0]);

            youBlocked = false;
            if (canBlock(intArguments[0]))
            {
                string message;

                defenders = getOwnDefendersThatCanBlock();
                message = "Select one defender to block the attack with, or don't block at all.";

                gUISelect = new GUIWindows.GUISelect(defenders, message, 1, null);
                gUISelect.replaceCancelButtonMessage("Don't defend");
                gUISelect.ShowDialog();

                // you blocked the attack
                if (!gUISelect.wasCanceled)
                {
                    youBlocked = true;
                }
            }

            if (youBlocked)
            {
                int index;

                index = listOwnBattleGround.IndexOf(defenders[gUISelect.selected[0]]);

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

        private bool canBlock(int cardIndex)
        {
            Card card = listOppBattleGround[cardIndex].Card;

            foreach(SpecialEffect se in card.SpecialEffects)
            {
                if (se.Effect == "Slippery")
                    return false;
            }

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

            /*
             * args[0] - number of safeguards that broke
             * args[1..args[0]] - the index of the broken safeguards
             * args[1 + args[0]..args[0] + args[0]] - cardIDs that were under those safeguards
             */

            count = args[0];
            for (int i = 1; i <= count; i++)
                animateSafeguardBrokeOWN(args[i], args[i + count]);

            txtOwnHand.Text = (Int32.Parse(txtOwnHand.Text) + count).ToString();
        }

        // whenever you break safeguards

        public void youBrokeGuards(List<int> args)
        {
            if (listOppSafeguardZone.Count - args.Count == 0)
                loadAttackPhaseButtons(true);

            foreach (int index in args)
                animateSafeguardBrokeOPP(index);

            txtOppHand.Text = (Int32.Parse(txtOppHand.Text) + args.Count).ToString();
        }

        // creature is under attack

        public void creatureUnderAttack(List<int> intArguments)
        {
            bool youBlocked;
            List<CardGUIModel> defenders = null;
            GUIWindows.GUISelect gUISelect = null;

            /*
             * intArguments[0] - the attacker
             * intArguments[1] - the creature that is being attacked
             */

            engageBattleOPP(intArguments[0]);

            youBlocked = false;
            if (canBlock(intArguments[0]))
            {
                string message;

                defenders = getOwnDefendersThatCanBlock();
                if (defenders.Count != 0)
                {
                    message = "Select one defender to block the attack with, or don't block at all.";

                    gUISelect = new GUIWindows.GUISelect(defenders, message, 1, null);
                    gUISelect.replaceCancelButtonMessage("Don't defend");
                    gUISelect.ShowDialog();

                    // you blocked the attack
                    if (!gUISelect.wasCanceled)
                    {
                        youBlocked = true;
                    }
                }
            }

            if (youBlocked)
            {
                int index;

                index = listOwnBattleGround.IndexOf(defenders[gUISelect.selected[0]]);

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
                    // animate sending opponent's creature to his graveyard
                    animateBattleToGraveyard(oppCreatureIndex, false);

                    // update info board
                    txtOppGrave.Text = (Int32.Parse(txtOppGrave.Text) + 1).ToString();

                    // if his creature was poisonous
                    if (hasEffect(oppCreature, "Poisonous"))
                    {
                        animateBattleToGraveyard(ownCreatureIndex, true);
                        txtOwnGrave.Text = (Int32.Parse(txtOwnGrave.Text) + 1).ToString();
                    }
                }
                else
                if (ownCreaturePower < oppCreaturePower)
                {
                    animateBattleToGraveyard(ownCreatureIndex, true);
                    txtOwnGrave.Text = (Int32.Parse(txtOwnGrave.Text) + 1).ToString();
                    if (hasEffect(ownCreature, "Poisonous"))
                    {
                        animateBattleToGraveyard(oppCreatureIndex, false);
                        txtOppGrave.Text = (Int32.Parse(txtOppGrave.Text) + 1).ToString();
                    }
                }
                else
                {
                    animateBattleToGraveyard(oppCreatureIndex, false);
                    animateBattleToGraveyard(ownCreatureIndex, true);
                    txtOppGrave.Text = (Int32.Parse(txtOppGrave.Text) + 1).ToString();
                    txtOwnGrave.Text = (Int32.Parse(txtOwnGrave.Text) + 1).ToString();
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
                    animateBattleToGraveyard(ownCreatureIndex, true);
                    txtOwnGrave.Text = (Int32.Parse(txtOwnGrave.Text) + 1).ToString();
                    if (hasEffect(ownCreature, "Poisonous"))
                    {
                        animateBattleToGraveyard(oppCreatureIndex, false);
                        txtOppGrave.Text = (Int32.Parse(txtOppGrave.Text) + 1).ToString();
                    }
                }
                else
                if (oppCreaturePower < ownCreaturePower)
                {
                    animateBattleToGraveyard(oppCreatureIndex, false);
                    txtOppGrave.Text = (Int32.Parse(txtOppGrave.Text) + 1).ToString();
                    if (hasEffect(oppCreature, "Poisonous"))
                    {
                        animateBattleToGraveyard(ownCreatureIndex, true);
                        txtOwnGrave.Text = (Int32.Parse(txtOwnGrave.Text) + 1).ToString();
                    }
                }
                else
                {
                    animateBattleToGraveyard(ownCreatureIndex, true);
                    animateBattleToGraveyard(oppCreatureIndex, false);
                    txtOwnGrave.Text = (Int32.Parse(txtOwnGrave.Text) + 1).ToString();
                    txtOppGrave.Text = (Int32.Parse(txtOppGrave.Text) + 1).ToString();
                }
            }
        }
    }
}
