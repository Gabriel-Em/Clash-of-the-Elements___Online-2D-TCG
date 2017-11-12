using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace DM___Client.Models
{
    public class AnimatioUtility
    {
        private DispatcherTimer runAnimation = new DispatcherTimer();
        private GUIPages.GUIGameRoom parent;

        public Border origin { get; set; }
        public Border destination { get; set; }
        public int animationsLeft { get; set; }

        public AnimatioUtility(GUIPages.GUIGameRoom parent_)
        {
            parent = parent_;
            origin = null;
            destination = null;

            runAnimation.Interval = new TimeSpan(0, 0, 0, 0, 0);
            runAnimation.Tick += RunAnimation_Tick;
        }

        private void animate()
        {
            Point oldPoint = origin.TranslatePoint(new Point(0, 0), parent.grdParent);
            Point newPoint = destination.TranslatePoint(new Point(0, 0), parent.grdParent);

            var EndX = destination.Width / 2 + newPoint.X - oldPoint.X - (origin.Width / 2);
            var EndY = destination.Height / 2 + newPoint.Y - oldPoint.Y - (origin.Height / 2);

            TranslateTransform trans = new TranslateTransform();
            origin.RenderTransform = trans;
            DoubleAnimation anim1 = new DoubleAnimation(0, EndX, TimeSpan.FromSeconds(0.3));
            DoubleAnimation anim2 = new DoubleAnimation(0, EndY, TimeSpan.FromSeconds(0.3));
            trans.BeginAnimation(TranslateTransform.XProperty, anim1);
            trans.BeginAnimation(TranslateTransform.YProperty, anim2);
        }

        private void RunAnimation_Tick(object sender, EventArgs e)
        {
            runAnimation.Stop();
            animate();
        }

        public void startAnimation()
        {
            runAnimation.Start();
        }
    }
}
