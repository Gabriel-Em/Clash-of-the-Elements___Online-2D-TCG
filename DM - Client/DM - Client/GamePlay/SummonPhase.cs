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

            // we notify the server that we entered Summon phase so it can notify our opponent of this action
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

                // if there is not enough unused mana or none of the mana is of card's element then we can't summon
                if (!checkCanSummon(selectedCards[0]))
                {
                    message = string.Format("You don't have the right mana to summon this {0}.", selectedCards[0].Card.Type);
                    MessageBox.Show(message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                validSelections = new List<Models.CardGUIModel>();

                // add all mana that's not engaged to the validSelections list (you can only use mana that's not engaged already)
                foreach (Models.CardGUIModel cardGUI in listOwnManaZone)
                {
                    if (!cardGUI.Card.isEngaged)
                        validSelections.Add(cardGUI);
                }

                int index;

                // create a GUI that will allow us to select the mana that we want to engage to pay the cost of the card we want to summon
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
                    ableToSelect.Remove(selectedCards[0]);
                    selectedCards[0].deselect();

                    // proceed to summon the card
                    summonOWN(selectedMana, index);
                }
            }
        }

        private bool checkCanSummon(Models.CardGUIModel card)
        {
            int count;
            bool found_element;

            count = 0;
            found_element = false;

            foreach(Models.CardGUIModel cardGUI in listOwnManaZone)
            {
                if (!cardGUI.Card.isEngaged)
                    count += 1;
                if (cardGUI.Card.Element == card.Card.Element)
                    found_element = true;
            }

            if (card.Card.Cost <= count && found_element)
                return true;
            return false;
        }

        private void summonOWN(List<int> selectedMana, int selectedCardIndex)
        {
            Models.CardGUIModel cardGUI;

            // get a reference to summoned card

            cardGUI = listHand[selectedCardIndex];

            // notify the server that we summoned a card so it can notify our opponent

            sendSummon(selectedMana, selectedCardIndex);

            // animate the engaging of the selected mana and the summoning of the card and then we align the remaining cards in our hand
            engageManaOWN(selectedMana);
            animateSummonOWN(selectedCardIndex);
            addAnimation(new Animations.AlignAnimation(listHand, AnimationConstants.handInitialPosition, AnimationConstants.handAlignPace));

            // update the info board
            updateInfoBoard("hand", true, -1);

            // check if any special effects have to trigger after the summon

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

            // animate the summoning of our opponent's card (the first card in list is the card our opponent has summoned and the rest are the mana he used)
            cardID = cardIDs[0];
            cardIDs.RemoveAt(0);
            engageManaOPP(cardIDs);
            cardGUI = animateSummonOPP(cardID);

            // update the info board
            updateInfoBoard("hand", false, -1);
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
