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
    public class AlignAnimation
    {
        private List<Models.CardGUIModel> cardsToAlign;
        private Thickness initialPosition;
        private int alignPace;

        private DispatcherTimer runAnimation;

        public int speed { get; set; }

        public bool isFinished { get; private set; }
        public bool isRunning { get; private set; }

        private const int DEFAULTSPEED = 100;

        public AlignAnimation(List<Models.CardGUIModel> cardsToAlign, Thickness initialPosition, int alignPace)
        {
            this.cardsToAlign = cardsToAlign;
            this.initialPosition = initialPosition;
            this.alignPace = alignPace;

            speed = DEFAULTSPEED;
            isFinished = false;
            isRunning = false;

            runAnimation = new DispatcherTimer();
            runAnimation.Interval = new TimeSpan(0, 0, 0, 0, 0);
            runAnimation.Tick += RunAnimation_Tick;
        }

        private void RunAnimation_Tick(object sender, EventArgs e)
        {
            Thickness margin;
            bool alreadyAligned;

            alreadyAligned = true;
            runAnimation.Stop();

            margin = new Thickness(initialPosition.Left, initialPosition.Top, initialPosition.Right, initialPosition.Bottom);

            foreach (Models.CardGUIModel cardGUI in cardsToAlign)
            {
                if (cardGUI.Border.Margin != margin)
                {
                    animate(cardGUI.Border);
                    alreadyAligned = false;
                }
                margin.Left += alignPace;
            }

            if (alreadyAligned)
                isFinished = true;
        }

        private void animate(Border border)
        {
            TranslateTransform trans = new TranslateTransform();
            border.RenderTransform = trans;

            DoubleAnimation anim1 = new DoubleAnimation(0, -alignPace, TimeSpan.FromMilliseconds(speed));
            anim1.Completed += Anim1_Completed;
            trans.BeginAnimation(TranslateTransform.XProperty, anim1);
        }

        private void Anim1_Completed(object sender, EventArgs e)
        {
            Thickness margin;

            margin = new Thickness(initialPosition.Left, initialPosition.Top, initialPosition.Right, initialPosition.Bottom);

            foreach (Models.CardGUIModel cardGUI in cardsToAlign)
            {
                if (cardGUI.Border.Margin != margin)
                {
                    cardGUI.Border.Margin = margin;
                    cardGUI.Border.RenderTransform = new TranslateTransform();
                }
                margin.Left += alignPace;
            }

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
