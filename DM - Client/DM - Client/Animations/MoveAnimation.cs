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

        public bool removeOrigin { get; set; }
        public bool startsWithHiddenOrigin { get; set; }

        public bool isFinished { get; private set; }
        public bool isRunning { get; private set; }

        public MoveAnimation(
            Grid originGrid, Grid destinationGrid, Grid parentGrid, 
            List<Models.CardGUIModel> originList, List<Models.CardGUIModel> destinationList,
            Models.CardGUIModel origin, 
            int type)
        {
            this.originGrid = originGrid;
            this.destinationGrid = destinationGrid;
            this.parentGrid = parentGrid;
            this.originList = originList;
            this.destinationList = destinationList;
            this.origin = origin;
            this.type = type;


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
                Point p = origin.Border.TranslatePoint(new Point(0, 0), destinationGrid);

                origin.Border.Margin = new Thickness(p.X, p.Y, 0, 0);
                origin.Border.RenderTransform = new TranslateTransform();
                parentGrid.Children.Remove(origin.Border);
                destinationGrid.Children.Add(origin.Border);
                origin.Border.Margin = cloneThickness(destination.Margin);
                origin.Border.RenderTransform = new TranslateTransform();
            }

            destinationGrid.Children.Remove(destination);
            if (removeOrigin)
                parentGrid.Children.Remove(origin.Border);

            if (originList != null)
                originList.Remove(origin);
            if (destinationList != null)
            {
                if (emptySpaceIndex == destinationList.Count)
                    destinationList.Add(origin);
                else
                    destinationList.Insert(emptySpaceIndex, origin);
            }

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

                origin.Border.Margin = new Thickness(p.X, p.Y, 0, 0);
                originGrid.Children.Remove(origin.Border);
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
                case AnimationConstants.DESTINATIONOWNHAND:
                    margin = cloneThickness(AnimationConstants.handInitialPosition);
                    foreach (Models.CardGUIModel cardGUI in destinationList)
                    {
                        if (cardGUI.Border.Margin != margin)
                            break;
                        margin.Left += AnimationConstants.handAlignPace;
                        emptySpaceIndex++;
                    }
                    break;
                case AnimationConstants.DESTINATIONOPPHAND:
                    margin = cloneThickness(AnimationConstants.oppHandLocation);
                    break;
                case AnimationConstants.DESTINATIONMANA:
                    margin = cloneThickness(AnimationConstants.manaInitialPosition);
                    foreach (Models.CardGUIModel cardGUI in destinationList)
                    {
                        if (cardGUI.Border.Margin != margin)
                            break;
                        margin.Left += AnimationConstants.manaAlignPace;
                        emptySpaceIndex++;
                    }
                    break;
                case AnimationConstants.DESTINATIONBATTLE:
                    margin = cloneThickness(AnimationConstants.battleGroundInitialPosition);
                    foreach (Models.CardGUIModel cardGUI in destinationList)
                    {
                        if (cardGUI.Border.Margin != margin)
                            break;
                        margin.Left += AnimationConstants.battleGroundAlignPace;
                        emptySpaceIndex++;
                    }
                    break;
                case AnimationConstants.DESTINATIONGRAVE:
                    margin = cloneThickness(AnimationConstants.graveInitialPosition);
                    break;
                case AnimationConstants.DESTINATIONSAFEGUARD:
                    margin = cloneThickness(AnimationConstants.safeguardInitialPosition);
                    foreach (Models.CardGUIModel cardGUI in destinationList)
                    {
                        if (cardGUI.Border.Margin != margin)
                            break;
                        margin.Left += AnimationConstants.safeguardAlignPace;
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
