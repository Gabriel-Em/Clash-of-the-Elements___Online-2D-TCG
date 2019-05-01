using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DM___Client.Animations;

namespace DM___Client.Animations
{
    public class Animation
    {
        public MoveAnimation moveAnimation { get; set; }
        public RotateAnimation rotateAnimation { get; set; }
        public AlignAnimation alignAnimation { get; set; }

        public int type;

        public delegate void loadPhaseMethod();
        public loadPhaseMethod loadPhase;

        public Animation(loadPhaseMethod loadPhase)
        {
            this.loadPhase = loadPhase;
            moveAnimation = null;
            rotateAnimation = null;
            alignAnimation = null;

            type = AnimationConstants.TYPELOAD;
        }

        public Animation(AlignAnimation alignAnimation)
        {
            this.alignAnimation = alignAnimation;
            moveAnimation = null;
            rotateAnimation = null;
            loadPhase = null;

            type = AnimationConstants.TYPEALIGN;
        }

        public Animation(MoveAnimation moveAnimation)
        {
            this.moveAnimation = moveAnimation;
            rotateAnimation = null;
            alignAnimation = null;
            loadPhase = null;

            type = AnimationConstants.TYPEMOVE;
        }

        public Animation(RotateAnimation rotateAnimation)
        {
            this.rotateAnimation = rotateAnimation;
            moveAnimation = null;
            alignAnimation = null;
            loadPhase = null;

            type = AnimationConstants.TYPEROTATE;
        }
    }
}
