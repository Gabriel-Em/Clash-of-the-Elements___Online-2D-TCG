using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using FontAwesome.WPF;
namespace DM___Client.GUIPages
{
    /// <summary>
    /// Interaction logic for GUILoading.xaml
    /// </summary>
    public partial class GUILoading : Page
    {
        private List<ImageAwesome> images;
        private const int BORDERHEIGHT = 136;
        private const int MESSAGEHEIGHT = 41;
        private const string MESSAGEBULLETPOINT = "+ ";

        public GUILoading(ImageSource source, List<string> loadedDataChecklistItems, List<bool> loadedDataChecklist)
        {
            InitializeComponent();
            backgroundImage.Source = source;
            surroundingBorder.Height = BORDERHEIGHT + loadedDataChecklistItems.Count * MESSAGEHEIGHT;
            interiorBorder.Height = BORDERHEIGHT + loadedDataChecklistItems.Count * MESSAGEHEIGHT;
            populateWindow(loadedDataChecklistItems, loadedDataChecklist);
        }

        private void populateWindow(List<string> loadedDataChecklistItems, List<bool> loadedDataChecklist)
        {
            images = new List<ImageAwesome>();
            for (int i = 0; i < loadedDataChecklistItems.Count; i++)
            {
                Label label = new Label();
                label.FontSize = 23;
                label.Foreground = Brushes.Black;
                label.Content = MESSAGEBULLETPOINT + loadedDataChecklistItems[i];
                label.HorizontalAlignment = HorizontalAlignment.Left;
                label.FontWeight = FontWeights.Bold;

                ImageAwesome image = new ImageAwesome();
                image.Width = 25;
                image.HorizontalAlignment = HorizontalAlignment.Right;
                image.Margin = new Thickness(0, 0, 30, 0);
                if (loadedDataChecklist[i])
                    image.Icon = FontAwesomeIcon.Check;
                else
                {
                    image.Icon = FontAwesomeIcon.Spinner;
                    image.SpinDuration = 5;
                    image.Spin = true;
                }
                images.Add(image);

                DockPanel dp = new DockPanel();
                dp.Children.Add(label);
                dp.Children.Add(image);

                checkListPanel.Children.Add(dp);
            }
        }

        public void updateMessages(List<bool> loadedDataCheckList)
        {
            for(int i = 0; i< loadedDataCheckList.Count;i++)
            {
                if (loadedDataCheckList[i] && images[i].Icon == FontAwesomeIcon.Spinner)
                {
                    images[i].Spin = false;
                    images[i].Icon = FontAwesomeIcon.Check;
                }
            }
        }
    }
}
