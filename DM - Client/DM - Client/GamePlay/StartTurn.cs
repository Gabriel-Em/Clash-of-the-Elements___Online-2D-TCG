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
            disengageEverything();

            foreach (Models.CardGUIModel cardGUI in listOwnBattleGround)
                if (!cardGUI.Card.hasCompletelyBeenSummoned)
                    cardGUI.Card.hasCompletelyBeenSummoned = true;

            for (int i = 0; i < listOppBattleGround.Count; i++)
            {
                if (listOppBattleGround[i].Card.Type == "Spell")
                    animateBattleToGraveyard(i, false);
            }

            ctrl.send(new Models.GameMessage("STARTTURN", ctrl.GameRoomID));
        }

        public void DrawCard(Models.CardWithGameProperties card)
        {
            animateDrawCardOWN(card);
            addAnimation(new Animations.AlignAnimation(listHand, AnimationConstants.handInitialPosition, AnimationConstants.handAlignPace));
            ableToSelect.Add(listHand[listHand.Count - 1]);
            txtOwnDeck.Text = (Int32.Parse(txtOwnDeck.Text) - 1).ToString();
            txtOwnHand.Text = (Int32.Parse(txtOwnHand.Text) + 1).ToString();
        }

        private void disengageEverything()
        {
            disengageManaOWN();
            disengageBattleOWN();
        }
    }
}
