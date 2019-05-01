using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DM___Client.GUIPages
{
    public partial class GUIGameRoom : Page
    {
        // Engage functions

        private void engageManaOWN(List<int> selectedMana)
        {
            foreach (int index in selectedMana)
            {
                listOwnManaZone[index].Card.isEngaged = true;
            }
            animateEngageManaOWN(selectedMana);
        }

        private void engageManaOPP(List<int> selectedMana)
        {
            foreach (int index in selectedMana)
            {
                listOppManaZone[index].Card.isEngaged = true;
            }
            animateEngageManaOPP(selectedMana);
        }

        private void engageBattleOWN(int cardIndex)
        {
            animateEngageBattleOWN(cardIndex);
            listOwnBattleGround[cardIndex].Card.isEngaged = true;
        }

        private void engageBattleOPP(int cardIndex)
        {
            animateEngageBattleOPP(listOppBattleGround[cardIndex]);
            listOppBattleGround[cardIndex].Card.isEngaged = true;
        }
    }
}
