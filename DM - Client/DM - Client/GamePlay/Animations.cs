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

        private void animateDrawCardOWN(Models.CardWithGameProperties card)
        {
            Models.CardGUIModel drawnCard;
            Animations.MoveAnimation animation;

            drawnCard = new Models.CardGUIModel(card, this, AnimationAndEventsConstants.ownDeckLocation, Visibility.Hidden);
            
            // add cards to grids
            grdParent.Children.Add(drawnCard.Border);

            if ((Phase == "Mana phase" || Phase == "Summon phase") && itIsOwnTurn)
                ableToSelect.Add(drawnCard);

            animation = new Animations.MoveAnimation(
                grdParent, 
                grdHand, 
                grdParent, 
                null, 
                listHand, 
                drawnCard, 
                AnimationAndEventsConstants.DESTINATIONOWNHAND);

            animation.startsWithHiddenOrigin = true;
            addAnimation(animation);
        }

        private void animateDrawCardOPP()
        {
            Models.CardGUIModel drawnCard;
            Animations.MoveAnimation animation;

            // set origin and destination
            drawnCard = new Models.CardGUIModel(null, this, AnimationAndEventsConstants.oppDeckLocation, Visibility.Hidden);

            // add cards to grids
            grdParent.Children.Add(drawnCard.Border);

            animation = new Animations.MoveAnimation(
                grdParent, 
                grdParent, 
                grdParent, 
                null, 
                null, 
                drawnCard,
                AnimationAndEventsConstants.DESTINATIONOPPHAND);

            animation.removeOrigin = true;
            animation.startsWithHiddenOrigin = true;
            addAnimation(animation);
        }

        // Mana Phase

        private void animatePlayAsManaOWN(int cardIndex)
        {
            Models.CardGUIModel card;
            Animations.MoveAnimation moveAnimation;
            Animations.RotateAnimation rotateAnimation;

            // select origin and create destination
            card = listHand[cardIndex];

            moveAnimation = new Animations.MoveAnimation(
                grdHand, 
                grdOwnMana, 
                grdParent, 
                listHand, 
                listOwnManaZone, 
                card,
                AnimationAndEventsConstants.DESTINATIONMANA);
            rotateAnimation = new RotateAnimation(true, 180);
            rotateAnimation.border = card.Border;

            addAnimation(moveAnimation);
            //addAnimation(rotateAnimation);
        }

        public void animatePlayAsManaOPP(int cardID)
        {
            Models.CardGUIModel card;
            Animations.MoveAnimation moveAnimation;
            Animations.RotateAnimation rotateAnimation;

            // create the origin and destination cards

            card = new Models.CardGUIModel(ctrl.getCardWithGamePropertiesByID(cardID), this, AnimationAndEventsConstants.oppHandLocation, Visibility.Visible);

            // add cards to grids
            grdParent.Children.Add(card.Border);

            moveAnimation = new Animations.MoveAnimation(
                grdParent, 
                grdOppMana,
                grdParent, 
                null, 
                listOppManaZone,
                card,
                AnimationAndEventsConstants.DESTINATIONMANA);
            rotateAnimation = new RotateAnimation(true, 180);
            rotateAnimation.border = card.Border;
            addAnimation(moveAnimation);
            //addAnimation(rotateAnimation);
        }

        // Summon Phase

        private void animateSummonOWN(int index)
        {
            Models.CardGUIModel card;
            Animations.MoveAnimation animation;

            // select origin and create destination
            card = listHand[index];

            animation = new Animations.MoveAnimation(
                grdHand, 
                grdOwnBattle, 
                grdParent, 
                listHand, 
                listOwnBattleGround, 
                card,
                AnimationAndEventsConstants.DESTINATIONBATTLE);
            addAnimation(animation);
        }

        private Models.CardGUIModel animateSummonOPP(int cardID)
        {
            Models.CardGUIModel card;
            Animations.MoveAnimation animation;

            // create origin and destination

            card = new Models.CardGUIModel(ctrl.getCardWithGamePropertiesByID(cardID), this, AnimationAndEventsConstants.oppHandLocation, Visibility.Visible);

            // add cards to grids
            grdParent.Children.Add(card.Border);

            animation = new Animations.MoveAnimation(
                grdParent,
                grdOppBattle,
                grdParent,
                null,
                listOppBattleGround, 
                card,
                AnimationAndEventsConstants.DESTINATIONBATTLE);
            addAnimation(animation);

            return card;
        }

        // Attack Phase

        private void animateSafeguardBrokeOWN(int index, int cardID)
        {
            Models.CardGUIModel safeGuard;
            Animations.MoveAnimation animation;

            // select origin and create destination

            safeGuard = listOwnSafeGuardZone[index];
          
            // reveal the safeguard
            safeGuard.setCard(ctrl.getCardWithGamePropertiesByID(cardID));

            animation = new Animations.MoveAnimation(
                grdOwnSafeguards, 
                grdHand, 
                grdParent, 
                listOwnSafeGuardZone,
                listHand, 
                safeGuard,
                AnimationAndEventsConstants.DESTINATIONOWNHAND,
                true);
            addAnimation(animation);
        }

        private void animateSafeguardToGroundOwn(int index, int cardID)
        {
            Models.CardGUIModel safeGuard;
            Animations.MoveAnimation animation;

            // select origin and create destination

            safeGuard = listOwnSafeGuardZone[index];

            // reveal the safeguard
            safeGuard.setCard(ctrl.getCardWithGamePropertiesByID(cardID));

            animation = new Animations.MoveAnimation(
                grdOwnSafeguards,
                grdOwnBattle,
                grdParent,
                listOwnSafeGuardZone,
                listOwnBattleGround,
                safeGuard,
                AnimationAndEventsConstants.DESTINATIONBATTLE,
                true);
            addAnimation(animation);
        }

        private void animateSafeguardToGroundOpp(int index, int cardID)
        {
            Models.CardGUIModel safeGuard;
            Animations.MoveAnimation animation;

            // select origin and create destination

            safeGuard = listOppSafeGuardZone[index];

            // reveal the safeguard
            safeGuard.setCard(ctrl.getCardWithGamePropertiesByID(cardID));

            animation = new Animations.MoveAnimation(
                grdOppSafeguards,
                grdOppBattle,
                grdParent,
                listOppSafeGuardZone,
                listOppBattleGround,
                safeGuard,
                AnimationAndEventsConstants.DESTINATIONBATTLE,
                true);
            addAnimation(animation);
        }

        private void animateSafeguardBrokeOPP(int index)
        {
            Models.CardGUIModel safeGuard;
            Animations.MoveAnimation animation;

            // select origin and create destination

            safeGuard = listOppSafeGuardZone[index];

            animation = new Animations.MoveAnimation(
                grdOppSafeguards, 
                grdParent, 
                grdParent, 
                listOppSafeGuardZone, 
                null, 
                safeGuard,
                AnimationAndEventsConstants.DESTINATIONOPPHAND);
            animation.removeOrigin = true;
            addAnimation(animation);
        }

        private void animateBattleToGraveyard(int cardIndex, bool own)
        {
            Models.CardGUIModel card;
            Animations.MoveAnimation moveAnimation;
            Animations.RotateAnimation rotateAnimation;

            // select origin and create destination

            if (own)
            {
                card = listOwnBattleGround[cardIndex];
                moveAnimation = new Animations.MoveAnimation(
                    grdOwnBattle,
                    grdOwnGrave,
                    grdParent,
                    listOwnBattleGround,
                    listOwnGraveyard,
                    card,
                    AnimationAndEventsConstants.DESTINATIONGRAVE);
            }
            else
            {
                card = listOppBattleGround[cardIndex];
                moveAnimation = new Animations.MoveAnimation(
                    grdOppBattle,
                    grdOppGrave,
                    grdParent,
                    listOppBattleGround,
                    listOppGraveyard,
                    card,
                    AnimationAndEventsConstants.DESTINATIONGRAVE);
            }

            if (card.Card.isEngaged)
            {
                rotateAnimation = new Animations.RotateAnimation(false);
                rotateAnimation.border = card.Border;
                addAnimation(rotateAnimation);
            }

            card.Card.resetProperties();

            addAnimation(moveAnimation);
        }

        // Engage

        private void animateEngageManaOWN(List<int> selectedMana)
        {
            Animations.RotateAnimation rotateAnimation;

            foreach (int index in selectedMana)
            {
                rotateAnimation = new Animations.RotateAnimation(true);
                rotateAnimation.border = listOwnManaZone[index].Border;
                addAnimation(rotateAnimation);
            }
        }

        private void animateEngageManaOPP(List<int> selectedMana)
        {
            Animations.RotateAnimation rotateAnimation;

            foreach (int index in selectedMana)
            {
                rotateAnimation = new Animations.RotateAnimation(true);
                rotateAnimation.border = listOppManaZone[index].Border;
                addAnimation(rotateAnimation);
            }
        }

        private void animateEngageBattleOWN(int cardIndex)
        {
            Animations.RotateAnimation rotateAnimation = new Animations.RotateAnimation(true);

            rotateAnimation.border = listOwnBattleGround[cardIndex].Border;
            addAnimation(rotateAnimation);
        }

        private void animateEngageBattleOPP(Models.CardGUIModel cardGUI)
        {
            Animations.RotateAnimation rotateAnimation = new Animations.RotateAnimation(true);

            rotateAnimation.border = cardGUI.Border;
            addAnimation(rotateAnimation);
        }

        // Disengage

        private void animateDisengageManaOWN()
        {
            int count;
            Animations.RotateAnimation rotateAnimation;

            count = 0;

            foreach (Models.CardGUIModel cardGUI in listOwnManaZone)
            {
                rotateAnimation = new Animations.RotateAnimation(false);
                if (cardGUI.Card.isEngaged)
                {
                    rotateAnimation.border = cardGUI.Border;
                    rotateAnimation.setSpeed(100);
                    addAnimation(rotateAnimation);
                    count++;
                }
            }
        }

        private void animateDisengageManaOPP()
        {
            int count;
            Animations.RotateAnimation rotateAnimation;

            count = 0;

            foreach (Models.CardGUIModel cardGUI in listOppManaZone)
            {
                if (cardGUI.Card.isEngaged)
                {
                    rotateAnimation = new Animations.RotateAnimation(false);
                    rotateAnimation.border = cardGUI.Border;
                    rotateAnimation.setSpeed(100);
                    addAnimation(rotateAnimation);
                    count++;
                }
            }
        }

        private void animateDisengageBattleOWN()
        {
            int count;
            Animations.RotateAnimation rotateAnimation;

            count = 0;

            foreach (Models.CardGUIModel cardGUI in listOwnBattleGround)
            {
                if (cardGUI.Card.isEngaged)
                {
                    rotateAnimation = new Animations.RotateAnimation(false);
                    rotateAnimation.border = cardGUI.Border;
                    rotateAnimation.setSpeed(100);
                    addAnimation(rotateAnimation);
                    count++;
                }
            }
        }

        private void animateDisengageBattleOPP()
        {
            int count;
            Animations.RotateAnimation rotateAnimation;

            count = 0;

            foreach (Models.CardGUIModel cardGUI in listOppBattleGround)
            {
                if (cardGUI.Card.isEngaged)
                {
                    rotateAnimation = new Animations.RotateAnimation(false);
                    rotateAnimation.border = cardGUI.Border;
                    rotateAnimation.setSpeed(100);
                    addAnimation(rotateAnimation);
                    count++;
                }
            }
        }

        // Special Effects

        private void animateManaToHandOpp(int cardIndex)
        {
            Models.CardGUIModel card;
            Animations.MoveAnimation animation;

            // select origin

            card = listOppManaZone[cardIndex];

            if (card.Card.isEngaged)
            {
                Animations.RotateAnimation rotateAnimation = new Animations.RotateAnimation(false);
                rotateAnimation.border = card.Border;
                addAnimation(rotateAnimation);
            }

            animation = new Animations.MoveAnimation(
                grdOppMana,
                grdParent,
                grdParent,
                listOppManaZone, 
                null, 
                card,
                AnimationAndEventsConstants.DESTINATIONOPPHAND);
            animation.removeOrigin = true;
            addAnimation(animation);
        }

        private void animateManaToGraveOpp(int cardIndex)
        {
            Models.CardGUIModel card;
            Animations.MoveAnimation animation;

            // select origin

            card = listOppManaZone[cardIndex];

            if (card.Card.isEngaged)
            {
                Animations.RotateAnimation rotateAnimation = new Animations.RotateAnimation(false);
                rotateAnimation.border = card.Border;
                addAnimation(rotateAnimation);
            }

            animation = new Animations.MoveAnimation(
                grdOppMana,
                grdOppGrave,
                grdParent,
                listOppManaZone,
                listOppGraveyard,
                card,
                AnimationAndEventsConstants.DESTINATIONGRAVE);
            addAnimation(animation);
        }

        private void animateManaToGraveOwn(int cardIndex)
        {
            Models.CardGUIModel card;
            Animations.MoveAnimation animation;

            // select origin

            card = listOwnManaZone[cardIndex];

            if (card.Card.isEngaged)
            {
                Animations.RotateAnimation rotateAnimation = new Animations.RotateAnimation(false);
                rotateAnimation.border = card.Border;
                addAnimation(rotateAnimation);
            }

            animation = new Animations.MoveAnimation(
                grdOwnMana,
                grdOwnGrave,
                grdParent,
                listOwnManaZone,
                listOwnGraveyard,
                card,
                AnimationAndEventsConstants.DESTINATIONGRAVE);
            addAnimation(animation);
        }

        private void animateManaToHandOwn(int cardIndex)
        {
            Models.CardGUIModel card;
            Animations.MoveAnimation animation;

            // select origin and create destination

            card = listOwnManaZone[cardIndex];
           
            if (card.Card.isEngaged)
            {
                Animations.RotateAnimation rotateAnimation = new Animations.RotateAnimation(false);
                rotateAnimation.border = card.Border;
                addAnimation(rotateAnimation);
            }

            card.Card.resetProperties();

            if ((Phase == "Mana phase" || Phase == "Summon phase") && itIsOwnTurn)
                ableToSelect.Add(card);

            animation = new Animations.MoveAnimation(
                grdOwnMana, 
                grdHand,
                grdParent, 
                listOwnManaZone,
                listHand,
                card,
                AnimationAndEventsConstants.DESTINATIONOWNHAND);
            addAnimation(animation);
        }

        private void animateGraveyardToBattle(int cardIndex, bool own)
        {
            Models.CardGUIModel card;
            Animations.MoveAnimation moveAnimation;

            // select origin

            if (own)
            {
                card = listOwnGraveyard[cardIndex];
                moveAnimation = new Animations.MoveAnimation(
                    grdOwnGrave,
                    grdOwnBattle,
                    grdParent,
                    listOwnGraveyard,
                    listOwnBattleGround,
                    card,
                    AnimationAndEventsConstants.DESTINATIONBATTLE);
            }
            else
            {
                card = listOppGraveyard[cardIndex];
                moveAnimation = new Animations.MoveAnimation(
                    grdOppGrave,
                    grdOppBattle,
                    grdParent,
                    listOppGraveyard,
                    listOppBattleGround,
                    card,
                    AnimationAndEventsConstants.DESTINATIONBATTLE);
            }

            addAnimation(moveAnimation);
        }

        private void animateGraveyardToHandOwn(int cardIndex)
        {
            Models.CardGUIModel card;
            Animations.MoveAnimation animation;

            // select origins

            card = listOwnGraveyard[cardIndex];

            if ((Phase == "Mana phase" || Phase == "Summon phase") && itIsOwnTurn)
                ableToSelect.Add(card);

            animation = new Animations.MoveAnimation(grdOwnGrave, 
                grdHand,
                grdParent,
                listOwnGraveyard, 
                listHand,
                card,
                AnimationAndEventsConstants.DESTINATIONOWNHAND);
            addAnimation(animation);
        }

        private void animateGraveyardToHandOpp(int cardIndex)
        {
            Models.CardGUIModel card;
            Animations.MoveAnimation animation;

            // select origin and create destination

            card = listOppGraveyard[cardIndex];

            animation = new Animations.MoveAnimation(
                grdOppGrave, 
                grdParent, 
                grdParent,
                listOppGraveyard,
                null,
                card,
                AnimationAndEventsConstants.DESTINATIONOPPHAND);
            animation.removeOrigin = true;

            addAnimation(animation);
        }

        private void animateBattleToHandOwn(int cardIndex)
        {
            Models.CardGUIModel card;
            Animations.MoveAnimation animation;

            // select origin and create destination

            card = listOwnBattleGround[cardIndex];

            if ((Phase == "Mana phase" || Phase == "Summon phase") && itIsOwnTurn)
                ableToSelect.Add(card);

            if (card.Card.isEngaged)
            {
                Animations.RotateAnimation rotateAnimation = new Animations.RotateAnimation(false);
                rotateAnimation.border = card.Border;
                addAnimation(rotateAnimation);
            }

            card.Card.resetProperties();

            animation = new Animations.MoveAnimation(grdOwnBattle,
                grdHand, 
                grdParent, 
                listOwnBattleGround, 
                listHand, 
                card,
                AnimationAndEventsConstants.DESTINATIONOWNHAND);
            addAnimation(animation);
        }

        private void animateBattleToHandOpp(int cardIndex)
        {
            Models.CardGUIModel card;
            Animations.MoveAnimation animation;

            // select origin and create destination

            card = listOppBattleGround[cardIndex];

            if (card.Card.isEngaged)
            {
                Animations.RotateAnimation rotateAnimation = new Animations.RotateAnimation(false);
                rotateAnimation.border = card.Border;
                addAnimation(rotateAnimation);
            }

            animation = new Animations.MoveAnimation(grdOppBattle,
                grdParent,
                grdParent,
                listOppBattleGround, 
                null, 
                card,
                AnimationAndEventsConstants.DESTINATIONOPPHAND);
            animation.removeOrigin = true;
            addAnimation(animation);
        }
    }
}
