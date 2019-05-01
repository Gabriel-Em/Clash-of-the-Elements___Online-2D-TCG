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
        // Disengage functions

        private void disengageManaOWN()
        {
            animateDisengageManaOWN();
            foreach (Models.CardGUIModel cardGUI in listOwnManaZone)
                cardGUI.Card.isEngaged = false;
        }

        private void disengageManaOPP()
        {
            animateDisengageManaOPP();
            foreach (Models.CardGUIModel cardGUI in listOppManaZone)
                cardGUI.Card.isEngaged = false;
        }

        private void disengageBattleOWN()
        {
            animateDisengageBattleOWN();
            foreach (Models.CardGUIModel cardGUI in listOwnBattleGround)
                cardGUI.Card.isEngaged = false;
        }

        private void disengageBattleOPP()
        {
            animateDisengageBattleOPP();
            foreach (Models.CardGUIModel cardGUI in listOppBattleGround)
                cardGUI.Card.isEngaged = false;
        }
    }
}
