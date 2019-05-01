using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DM___Client.Animations;

namespace DM___Client.GUIPages
{
    public partial class GUIGameRoom : Page
    {
        // Summon Phase

        public void loadSummonPhase()
        {
            updateGameState(true, "Summon phase");
            ctrl.send(new Models.GameMessage("SETPHASE", ctrl.GameRoomID, new List<string>() { "Summon phase" }));
            setAbleToSelect(1, listHand.ToList<Models.CardGUIModel>());
            loadSummonPhaseButtons();
        }

        private void loadSummonPhaseButtons()
        {
            actionButtons.Children.Clear();
            actionButtons.Children.Add(btnPlayCard);
            actionButtons.Children.Add(btnNextPhase);
        }

        private void BtnPlayCard_Click(object sender, RoutedEventArgs e)
        {
            string message;

            if (selectedCards.Count != ableToSelectLimit)
                MessageBox.Show("You must select " + ableToSelectLimit.ToString() + " card(s) to summon.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            else
            {
                GUIWindows.GUISelect guiSelect;
                List<Models.CardGUIModel> validSelections;

                validSelections = new List<Models.CardGUIModel>();

                // you can select mana that's not engaged already
                foreach (Models.CardGUIModel cardGUI in listOwnManaZone)
                {
                    if (!cardGUI.Card.isEngaged)
                        validSelections.Add(cardGUI);
                }

                if (selectedCards[0].Card.Cost > validSelections.Count)
                {
                    message = string.Format("You don't have enough mana to summon this {0}.", selectedCards[0].Card.Type);
                    MessageBox.Show(message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    int index;

                    message = string.Format("You must select a total of {0} mana, out of which at least one must be of {1} type.", selectedCards[0].Card.Cost, selectedCards[0].Card.Element);
                    guiSelect = new GUIWindows.GUISelect(validSelections, message, selectedCards[0].Card.Cost, selectedCards[0].Card.Element);

                    guiSelect.ShowDialog();

                    if (!guiSelect.wasCanceled)
                    {
                        List<int> selectedMana;

                        selectedMana = new List<int>();

                        // translate the selected indexes from the validSelections to indexes from the listOwnManaZone
                        foreach (int selectedIndex in guiSelect.selected)
                            selectedMana.Add(listOwnManaZone.IndexOf(validSelections[selectedIndex]));
                        
                        index = listHand.IndexOf(selectedCards[0]);
                        if (hasManaToSummon(selectedMana, index))
                        {
                            ableToSelect.Remove(selectedCards[0]);
                            selectedCards[0].deselect();

                            summonOWN(selectedMana, index);
                        }
                    }
                }
            }
        }

        private bool hasManaToSummon(List<int> selectedMana, int selectedCardIndex)
        {
            Models.Card card;
            string errorMessage;

            card = listHand[selectedCardIndex].Card;
            errorMessage = string.Format("You must select a total of {0} mana, out of which at least one must be a {1} type.", card.Cost.ToString(), card.Element);

            if (selectedMana.Count == card.Cost)
            {
                foreach (int index in selectedMana)
                {
                    if (listOwnManaZone[index].Card.Element == card.Element)
                        return true;
                }
            }

            MessageBox.Show(errorMessage, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        private void summonOWN(List<int> selectedMana, int selectedCardIndex)
        {
            Models.CardGUIModel cardGUI;

            // get a reference to summoned card

            cardGUI = listHand[selectedCardIndex];

            // summon

            sendSummon(selectedMana, selectedCardIndex);

            engageManaOWN(selectedMana);
            animateSummonOWN(selectedCardIndex);

            addAnimation(new Animations.AlignAnimation(listHand, AnimationConstants.handInitialPosition, AnimationConstants.handAlignPace));

            txtOwnHand.Text = (Int32.Parse(txtOwnHand.Text) - 1).ToString();

            // check summon triggers

            if (hasTrigger(cardGUI.Card, "Summon"))
            {
                foreach (Models.SpecialEffect se in cardGUI.Card.SpecialEffects)
                {
                    if (se.Trigger == "Summon")
                        triggerEffect(se, cardGUI.Card);
                }
            }
        }

        public void summonOPP(List<int> cardIDs)
        {
            int cardID;
            Models.CardGUIModel cardGUI;

            cardID = cardIDs[0];
            cardIDs.RemoveAt(0);
            engageManaOPP(cardIDs);
            cardGUI = animateSummonOPP(cardID);

            txtOppHand.Text = (Int32.Parse(txtOppHand.Text) - 1).ToString();
        }

        private void sendSummon(List<int> selectedMana, int selectedCardIndex)
        {
            Models.GameMessage gameMessage = new Models.GameMessage();

            gameMessage.Command = "SUMMON";
            gameMessage.GameID = ctrl.GameRoomID;
            gameMessage.intArguments.Add(listHand[selectedCardIndex].Card.ID);
            foreach (int index in selectedMana)
                gameMessage.intArguments.Add(index);

            ctrl.send(gameMessage);
        }
    }
}
