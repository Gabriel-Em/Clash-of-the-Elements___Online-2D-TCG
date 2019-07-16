using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DM___Client.Animations
{
    public static class AnimationConstants
    {
        public static Thickness safeguardInitialPosition { get { return new Thickness(25, 0, 0, 0); } }
        public static Thickness handInitialPosition { get { return new Thickness(5, 0, 0, 0); } }
        public static Thickness manaInitialPosition { get { return new Thickness(5, 0, 0, 0); } }
        public static Thickness graveInitialPosition { get { return new Thickness(5, 0, 0, 0); } }
        public static Thickness battleGroundInitialPosition { get { return new Thickness(25, 0, 0, 0); } }

        public static Thickness ownDeckLocation { get { return new Thickness(983, 465, 0, 0); } }
        public static Thickness oppDeckLocation { get { return new Thickness(475, 133, 0, 0); } }
        public static Thickness oppHandLocation { get { return new Thickness(751, -90, 0, 0); } }

        public const int handAlignPace = 23;
        public const int manaAlignPace = 75;
        public const int battleGroundAlignPace = 75;
        public const int safeguardAlignPace = 100;

        // MoveAnimation
        public const int DESTINATIONOWNHAND = 1;
        public const int DESTINATIONOPPHAND = 2;
        public const int DESTINATIONMANA = 3;
        public const int DESTINATIONBATTLE = 4;
        public const int DESTINATIONGRAVE = 5;
        public const int DESTINATIONSAFEGUARD = 6;

        // Animation
        public const int TYPEMOVE = 7;
        public const int TYPEROTATE = 8;
        public const int TYPEALIGN = 9;
        public const int TYPERUNMETHOD = 10;
        public const int TYPETRIGGER = 12;
    }
}
