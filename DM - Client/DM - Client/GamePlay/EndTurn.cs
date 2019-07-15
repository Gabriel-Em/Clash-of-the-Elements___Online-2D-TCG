using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DM___Client.GUIPages
{
    public partial class GUIGameRoom : Page
    {
        // End Turn

        private void BtnEndTurn_Click(object sender, RoutedEventArgs e)
        {
            deselectAll();
            ableToSelect.Clear();

            // remove all spells we used during our turn from the battle ground
            for (int i = 0; i < listOwnBattleGround.Count; i++)
            {
                if (listOwnBattleGround[i].Card.Type == "Spell")
                {
                    animateBattleToGraveyard(i, OWN);
                    updateInfoBoard("grave", OWN, 1);
                }
            }

            // remove any shield trigger spells our opponent used from the battle ground
            for (int i = 0; i < listOppBattleGround.Count; i++)
            {
                if (listOppBattleGround[i].Card.Type == "Spell")
                {
                    animateBattleToGraveyard(i, OPP);
                    updateInfoBoard("grave", OPP, 1);
                }
            }

            disengageManaOPP();
            disengageBattleOPP();
            animateDrawCardOPP();
            updateGameState(false, "Mana phase");

            ctrl.send(new Models.GameMessage("ENDTURN", ctrl.GameRoomID));

            actionButtons.Children.Clear();
            updateInfoBoard("hand", OPP, 1);
            updateInfoBoard("deck", OPP, -1);
        }
    }
}
