using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Collections.Generic;
using DM___Client.Animations;
using System.Threading;

namespace DM___Client.Animations
{
    public class MoveAnimation
    {
        private DispatcherTimer runAnimation;

        private Grid parentGrid;
        private Grid originGrid;
        private Grid destinationGrid;

        private Models.CardGUIModel origin;
        private Border destination;

        private List<Models.CardGUIModel> originList;
        private List<Models.CardGUIModel> destinationList;

        private int speed;
        private int type;
        private int emptySpaceIndex;

        private const int DEFAULTSPEED = 350;

        private TranslateTransform trans;
        private double displacementX;
        private double displacementY;
        private bool wasSafeguard;

        public bool removeOrigin { get; set; }
        public bool startsWithHiddenOrigin { get; set; }

        public bool isFinished { get; private set; }
        public bool isRunning { get; private set; }

        public MoveAnimation(
            Grid originGrid, Grid destinationGrid, Grid parentGrid, 
            List<Models.CardGUIModel> originList, List<Models.CardGUIModel> destinationList,
            Models.CardGUIModel origin, 
            int type,
            bool wasSafeguard=false)
        {
            this.originGrid = originGrid;
            this.destinationGrid = destinationGrid;
            this.parentGrid = parentGrid;
            this.originList = originList;
            this.destinationList = destinationList;
            this.origin = origin;
            this.type = type;
            this.wasSafeguard = wasSafeguard;

            destination = new Border();
            removeOrigin = false;
            isFinished = false;
            isRunning = false;
            startsWithHiddenOrigin = false;
            speed = DEFAULTSPEED;

            runAnimation = new DispatcherTimer();
            runAnimation.Interval = new TimeSpan(0, 0, 0, 0, 0);
            runAnimation.Tick += RunAnimation_Tick;
        }

        private void animate()
        {
            Point destinationPoint = destination.TranslatePoint(new Point(0, 0), parentGrid);

            displacementX = destinationPoint.X - origin.Border.Margin.Left;
            displacementY = destinationPoint.Y - origin.Border.Margin.Top;

            trans = new TranslateTransform();
            origin.Border.RenderTransform = trans;

            DoubleAnimation animX = new DoubleAnimation(0, displacementX, TimeSpan.FromMilliseconds(speed));
            DoubleAnimation animY = new DoubleAnimation(0, displacementY, TimeSpan.FromMilliseconds(speed));
            animX.Completed += AnimX_Completed;
            trans.BeginAnimation(TranslateTransform.XProperty, animX);
            trans.BeginAnimation(TranslateTransform.YProperty, animY);
        }

        private void AnimX_Completed(object sender, EventArgs e)
        {
            if (destinationGrid != parentGrid)
            {
                parentGrid.Children.Remove(origin.Border);
                origin.Border.Margin = cloneThickness(destination.Margin);
                destinationGrid.Children.Add(origin.Border);
            }

            origin.Border.RenderTransform = new TranslateTransform();
            destinationGrid.Children.Remove(destination);
            if (removeOrigin)
                destinationGrid.Children.Remove(origin.Border);

            if (originList != null)
                originList.Remove(origin);
            if (destinationList != null)
            {
                if (emptySpaceIndex == destinationList.Count)
                    destinationList.Add(origin);
                else
                    destinationList.Insert(emptySpaceIndex, origin);
            }

            if (wasSafeguard)
                origin.removeTextBlock();

            isFinished = true;
            isRunning = false;
        }

        private void RunAnimation_Tick(object sender, EventArgs e)
        {
            runAnimation.Stop();

            if (startsWithHiddenOrigin)
                origin.turnVisibilityON();

            // move origin to parent grid if not already there
            if (originGrid != parentGrid)
            {
                Point p = origin.Border.TranslatePoint(new Point(0, 0), parentGrid);

                originGrid.Children.Remove(origin.Border);
                origin.Border.Margin = new Thickness(p.X, p.Y, 0, 0);
                parentGrid.Children.Add(origin.Border);
            }

            animate();
        }

        private Thickness workOutDestinationCoordinates()
        {
            Thickness margin;

            emptySpaceIndex = 0;

            switch (type)
            {
                case AnimationAndEventsConstants.DESTINATIONOWNHAND:
                    margin = cloneThickness(AnimationAndEventsConstants.handInitialPosition);
                    foreach (Models.CardGUIModel cardGUI in destinationList)
                    {
                        if (cardGUI.Border.Margin != margin)
                            break;
                        margin.Left += AnimationAndEventsConstants.handAlignPace;
                        emptySpaceIndex++;
                    }
                    break;
                case AnimationAndEventsConstants.DESTINATIONOPPHAND:
                    margin = cloneThickness(AnimationAndEventsConstants.oppHandLocation);
                    break;
                case AnimationAndEventsConstants.DESTINATIONMANA:
                    margin = cloneThickness(AnimationAndEventsConstants.manaInitialPosition);
                    foreach (Models.CardGUIModel cardGUI in destinationList)
                    {
                        if (cardGUI.Border.Margin != margin)
                            break;
                        margin.Left += AnimationAndEventsConstants.manaAlignPace;
                        emptySpaceIndex++;
                    }
                    break;
                case AnimationAndEventsConstants.DESTINATIONBATTLE:
                    margin = cloneThickness(AnimationAndEventsConstants.battleGroundInitialPosition);
                    foreach (Models.CardGUIModel cardGUI in destinationList)
                    {
                        if (cardGUI.Border.Margin != margin)
                            break;
                        margin.Left += AnimationAndEventsConstants.battleGroundAlignPace;
                        emptySpaceIndex++;
                    }
                    break;
                case AnimationAndEventsConstants.DESTINATIONGRAVE:
                    margin = cloneThickness(AnimationAndEventsConstants.graveInitialPosition);
                    break;
                case AnimationAndEventsConstants.DESTINATIONSAFEGUARD:
                    margin = cloneThickness(AnimationAndEventsConstants.safeguardInitialPosition);
                    foreach (Models.CardGUIModel cardGUI in destinationList)
                    {
                        if (cardGUI.Border.Margin != margin)
                            break;
                        margin.Left += AnimationAndEventsConstants.safeguardAlignPace;
                        emptySpaceIndex++;
                    }
                    break;
                default:
                    margin = new Thickness();
                    break;
            }

            return margin;
        }

        private Thickness cloneThickness(Thickness margin)
        {
            return new Thickness(margin.Left, margin.Top, margin.Right, margin.Bottom);
        }

        public void startAnimation()
        {
            isRunning = true;

            // work out destination coordinates
            destination.Margin = workOutDestinationCoordinates();

            // add destination to destinationGrid
            destinationGrid.Children.Add(destination);

            runAnimation.Start();
        }

        public void setDelay(int milliseconds)
        {
            runAnimation.Interval = new TimeSpan(0, 0, 0, 0, milliseconds);
        }

        public void setSpeed(int milliseconds)
        {
            speed = milliseconds;
        }
    }
}
