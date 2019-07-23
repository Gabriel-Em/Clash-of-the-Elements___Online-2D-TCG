using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DM___Client.Animations;
using DM___Client.Models;

namespace DM___Client.Animations
{
    public class Event
    {
        public MoveAnimation moveAnimation { get; set; }
        public RotateAnimation rotateAnimation { get; set; }
        public AlignAnimation alignAnimation { get; set; }

        public int type;

        public delegate void runMethodEvent();
        public delegate void triggerEffectMethod(SpecialEffect se, CardWithGameProperties card);
        public delegate void processShieldMethod(int cardID, int shieldNumber);

        public runMethodEvent runMethod;
        private triggerEffectMethod triggerEffect_;
        private processShieldMethod processShield_;

        private SpecialEffect se;
        private CardWithGameProperties card;
        private int cardID;
        private int shieldNumber;

        public Event(runMethodEvent runMethod)
        {
            this.runMethod = runMethod;
            type = AnimationAndEventsConstants.TYPERUNMETHOD;
        }

        public Event(triggerEffectMethod triggerEffect, SpecialEffect se, CardWithGameProperties card)
        {
            triggerEffect_ = triggerEffect;
            this.se = se;
            this.card = card;

            type = AnimationAndEventsConstants.TYPETRIGGER;
        }

        public Event(processShieldMethod processShield, int cardID, int shieldNumber)
        {
            processShield_ = processShield;
            this.cardID = cardID;
            this.shieldNumber = shieldNumber;

            type = AnimationAndEventsConstants.TYPEPROCESSSHIELD;
        }

        public void triggerEffect()
        {
            triggerEffect_(se, card);
        }

        public void triggerProcessShield()
        {
            processShield_(cardID, shieldNumber);
        }

        public Event(AlignAnimation alignAnimation)
        {
            this.alignAnimation = alignAnimation;
            type = AnimationAndEventsConstants.TYPEALIGN;
        }

        public Event(MoveAnimation moveAnimation)
        {
            this.moveAnimation = moveAnimation;
            type = AnimationAndEventsConstants.TYPEMOVE;
        }

        public Event(RotateAnimation rotateAnimation)
        {
            this.rotateAnimation = rotateAnimation;
            type = AnimationAndEventsConstants.TYPEROTATE;
        }

        public Event(int type)
        {
            this.type = type;
        }
    }
}
