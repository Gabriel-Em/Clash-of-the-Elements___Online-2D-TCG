using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DM___Client.Animations;
using DM___Client.Models;

namespace DM___Client.Animations
{
    public class Animation
    {
        public MoveAnimation moveAnimation { get; set; }
        public RotateAnimation rotateAnimation { get; set; }
        public AlignAnimation alignAnimation { get; set; }

        public int type;

        public delegate void runMethodEvent();
        public delegate void triggerEffectMethod(SpecialEffect se, CardWithGameProperties card);

        public runMethodEvent runMethod;
        private triggerEffectMethod triggerEffect_;

        private SpecialEffect se;
        private CardWithGameProperties card;

        public Animation(runMethodEvent runMethod)
        {
            this.runMethod = runMethod;
            type = AnimationConstants.TYPERUNMETHOD;
        }

        public Animation(triggerEffectMethod triggerEffect, SpecialEffect se, CardWithGameProperties card)
        {
            this.triggerEffect_ = triggerEffect;
            this.se = se;
            this.card = card;

            type = AnimationConstants.TYPETRIGGER;
        }

        public void triggerEffect()
        {
            triggerEffect_(se, card);
        }

        public Animation(AlignAnimation alignAnimation)
        {
            this.alignAnimation = alignAnimation;
            type = AnimationConstants.TYPEALIGN;
        }

        public Animation(MoveAnimation moveAnimation)
        {
            this.moveAnimation = moveAnimation;
            type = AnimationConstants.TYPEMOVE;
        }

        public Animation(RotateAnimation rotateAnimation)
        {
            this.rotateAnimation = rotateAnimation;
            type = AnimationConstants.TYPEROTATE;
        }
    }
}
