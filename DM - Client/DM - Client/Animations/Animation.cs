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

        public delegate void loadPhaseMethod();
        public delegate void triggerEffectMethod(SpecialEffect se, CardWithGameProperties card);

        public loadPhaseMethod loadPhase_;
        public triggerEffectMethod triggerEffect_;

        private SpecialEffect se;
        private CardWithGameProperties card;

        public Animation(loadPhaseMethod loadPhase)
        {
            this.loadPhase_ = loadPhase;
            moveAnimation = null;
            rotateAnimation = null;
            alignAnimation = null;
            triggerEffect_ = null;

            type = AnimationConstants.TYPELOAD;
        }

        public Animation(triggerEffectMethod triggerEffect, SpecialEffect se, CardWithGameProperties card)
        {
            this.triggerEffect_ = triggerEffect;
            moveAnimation = null;
            rotateAnimation = null;
            alignAnimation = null;
            loadPhase_ = null;

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
            moveAnimation = null;
            rotateAnimation = null;
            loadPhase_ = null;
            triggerEffect_ = null;

            type = AnimationConstants.TYPEALIGN;
        }

        public Animation(MoveAnimation moveAnimation)
        {
            this.moveAnimation = moveAnimation;
            rotateAnimation = null;
            alignAnimation = null;
            loadPhase_ = null;
            triggerEffect_ = null;

            type = AnimationConstants.TYPEMOVE;
        }

        public Animation(RotateAnimation rotateAnimation)
        {
            this.rotateAnimation = rotateAnimation;
            moveAnimation = null;
            alignAnimation = null;
            loadPhase_ = null;
            triggerEffect_ = null;

            type = AnimationConstants.TYPEROTATE;
        }
    }
}
