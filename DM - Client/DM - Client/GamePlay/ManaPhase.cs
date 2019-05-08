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
        // Mana Phase

        public void loadManaPhase()
        {
            updateGameState(true, "Mana phase");
            setAbleToSelect(1, listHand.ToList<Models.CardGUIModel>());
            loadManaPhaseButtons();
        }

        private void loadManaPhaseButtons()
        {
            actionButtons.Children.Clear();
            actionButtons.Children.Add(btnPlayAsMana);
            actionButtons.Children.Add(btnNextPhase);
            actionButtons.Children.Add(btnSurrender);
        }

        private void BtnPlayAsMana_Click(object sender, RoutedEventArgs e)
        {
            if (selectedCards.Count != ableToSelectLimit)
                MessageBox.Show("You must select " + ableToSelectLimit.ToString() + " card(s) to play as mana.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            else
            {
                int index;

                actionButtons.Children.Clear();

                ableToSelect.Clear();
                index = listHand.IndexOf(selectedCards[0]);
                selectedCards[0].deselect();

                // we notify the server that we played a card as mana so it can notify our opponent
                sendPlayAsMana(index);

                // we animate the action and we realign the cards in our hand
                animatePlayAsManaOWN(index);
                addAnimation(new Animations.AlignAnimation(listHand, AnimationConstants.handInitialPosition, AnimationConstants.handAlignPace));

                // we update the info board
                updateInfoBoard("hand", OWN, -1);
                updateInfoBoard("mana", OWN, 1);

                // we want to load the summon phase AFTER all the animations have played out so that's why we insert an animation object which is actually a notification and not an animation
                // which will trigger the loadSummonPhase method after all the animations have ended
                addLoadEvent(new Animation(loadSummonPhase));
            }
        }

        private void sendPlayAsMana(int index)
        {
            Models.GameMessage gameMessage = new Models.GameMessage();
            gameMessage.GameID = ctrl.GameRoomID;
            gameMessage.Command = "PLAYASMANA";
            gameMessage.intArguments.Add(listHand[index].Card.ID);
            ctrl.send(gameMessage);
        }
    }
}
