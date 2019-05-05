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
        // Start Turn

        public void startTurn()
        {
            // remove all spells that our opponent has used from the battle ground
            for (int i = 0; i < listOppBattleGround.Count; i++)
            {
                if (listOppBattleGround[i].Card.Type == "Spell")
                {
                    animateBattleToGraveyard(i, OPP);
                    updateInfoBoard("grave", OPP, 1);
                }
            }

            disengageEverything();

            // set cards that have been summoned but were unable to be used to be able to be used
            foreach (Models.CardGUIModel cardGUI in listOwnBattleGround)
                if (!cardGUI.Card.hasCompletelyBeenSummoned)
                    cardGUI.Card.hasCompletelyBeenSummoned = true;

            // notify the server that our turn has started so it can reveal the cardID of the card we need to draw at the start of our turn
            ctrl.send(new Models.GameMessage("STARTTURN", ctrl.GameRoomID));
        }

        public void DrawCard(Models.CardWithGameProperties card)
        {
            animateDrawCardOWN(card);
            addAnimation(new Animations.AlignAnimation(listHand, AnimationConstants.handInitialPosition, AnimationConstants.handAlignPace));

            // if we drew a card during our own Summon phase we add it to the ableToSelect list
            if (Phase == "Summon phase" && itIsOwnTurn)
                ableToSelect.Add(listHand[listHand.Count - 1]);

            // we update the info board
            updateInfoBoard("hand", OWN, 1);
            updateInfoBoard("deck", OWN, -1);
        }

        private void disengageEverything()
        {
            disengageManaOWN();
            disengageBattleOWN();
        }
    }
}
