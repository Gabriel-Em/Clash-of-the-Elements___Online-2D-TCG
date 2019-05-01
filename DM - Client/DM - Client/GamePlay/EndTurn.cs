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
            disengageManaOPP();
            disengageBattleOPP();
            animateDrawCardOPP();
            updateGameState(false, "Mana Phase");

            for (int i = 0; i < listOwnBattleGround.Count; i++)
            {
                if (listOwnBattleGround[i].Card.Type == "Spell")
                    animateBattleToGraveyard(i, true);
            }

            ctrl.send(new Models.GameMessage("ENDTURN", ctrl.GameRoomID));

            actionButtons.Children.Clear();
            txtOppDeck.Text = (Int32.Parse(txtOppDeck.Text) - 1).ToString();
            txtOppHand.Text = (Int32.Parse(txtOppHand.Text) + 1).ToString();
        }
    }
}
