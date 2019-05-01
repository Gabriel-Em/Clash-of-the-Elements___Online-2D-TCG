using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace DM___Client.Animations
{
    public class RotateAnimation
    {
        private DispatcherTimer runAnimation;

        public Border border { get; set; }
        public int speed { get; set; }
        public bool engage { get; set; }

        public bool isFinished { get; private set; }
        public bool isRunning { get; private set; }

        private const int DEFAULTSPEED = 300;

        public RotateAnimation(bool engage_)
        {
            engage = engage_;
            speed = DEFAULTSPEED;
            isFinished = false;
            isRunning = false;
            border = null;

            runAnimation = new DispatcherTimer();
            runAnimation.Interval = new TimeSpan(0, 0, 0, 0, 0);
            runAnimation.Tick += RunAnimation_Tick;
        }

        private void RunAnimation_Tick(object sender, EventArgs e)
        {
            runAnimation.Stop();
            animate();
        }

        private void animate()
        {
            var ease = new PowerEase { EasingMode = EasingMode.EaseOut };
            DoubleAnimation myanimation;

            if (engage)
            {
                myanimation = new DoubleAnimation(0, 90, new Duration(TimeSpan.FromMilliseconds(speed)));
            }
            else
            {
                myanimation = new DoubleAnimation(90, 0, new Duration(TimeSpan.FromMilliseconds(speed)));
            }

            myanimation.Completed += Myanimation_Completed;

            //Adding Power ease to the animation
            myanimation.EasingFunction = ease;

            RotateTransform rt = new RotateTransform();

            border.RenderTransform = rt;
            border.RenderTransformOrigin = new Point(0.5, 0.5);
            rt.BeginAnimation(RotateTransform.AngleProperty, myanimation);
        }

        private void Myanimation_Completed(object sender, EventArgs e)
        {
            isFinished = true;
            isRunning = false;
        }

        public void startAnimation()
        {
            isRunning = true;
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
