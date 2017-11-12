using System;
using System.Collections.Generic;
using System.IO;
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

namespace DM___Client.GUIPages
{
    /// <summary>
    /// Interaction logic for GUICollection.xaml
    /// </summary>
    public partial class GUICollection : Page
    {
        private GUIWindows.GUI parent;
        private Controllers.CollectionController ctrl;
        private DispatcherTimer checkServerResponse = new DispatcherTimer();
        private List<Image> GuiImages;
        private List<Button> GuiButtons;
        private Image tempImage;
        private bool isWindowLoaded;
        private List<Models.DeckGUIModel> DecksGUI;
        private List<Models.DeckItemGUIModel> DeckContentGUI;
        private int selectedDeckID;
        public ImageSource BackgroundImageSource { get { return backgroundImage.Source; } }

        public GUICollection(GUIWindows.GUI parent_, Communication com_, Models.CardCollection CardCollection_)
        {
            isWindowLoaded = false;
            InitializeComponent();
            isWindowLoaded = true;
            parent = parent_;
            DecksGUI = new List<Models.DeckGUIModel>();
            DeckContentGUI = new List<Models.DeckItemGUIModel>();
            ctrl = new Controllers.CollectionController(this, com_, CardCollection_);
            selectedDeckID = -1;
            tempImage = new Image();
            tempImage.Width = 222;
            tempImage.Height = 307;
            tempImage.VerticalAlignment = VerticalAlignment.Top;
            tempImage.HorizontalAlignment = HorizontalAlignment.Left;
            tempImage.Visibility = Visibility.Hidden;
            tempImage.Stretch = Stretch.Fill;
            cardGrid.Children.Add(tempImage);

            GuiImages = new List<Image>() { cardImage1, cardImage2, cardImage3, cardImage4, cardImage5, cardImage6, cardImage7, cardImage8, cardImage9, cardImage10 };
            GuiButtons = new List<Button>() { btnCard1, btnCard2, btnCard3, btnCard4, btnCard5, btnCard6, btnCard7, btnCard8, btnCard9, btnCard10 };

            loadCardsToGUI(ctrl.currentList, ctrl.currentIndex);
            checkServerResponse.Interval = new TimeSpan(0, 0, 0, 0, 250);
            checkServerResponse.Tick += checkServerResponse_Tick;
            ctrl.loadPageData();
            beginListening();
        }

        private void beginListening()
        {
            if (!ctrl.Listening)
                ctrl.beginListen();
            if (!checkServerResponse.IsEnabled)
                checkServerResponse.Start();
        }

        private void stopListening()
        {
            if (ctrl.Listening)
                ctrl.stopListen();
            if (checkServerResponse.IsEnabled)
                checkServerResponse.Stop();
        }

        private void checkServerResponse_Tick(object sender, EventArgs e)
        {
            if (ctrl.hasReceivedResponse())
                ctrl.messageProcessor(ctrl.getReceivedResponse());
        }

        public List<bool> getLoadedData()
        {
            return ctrl.getLoadedData();
        }

        public bool DoneLoading()
        {
            if (ctrl.getLoadedData().Contains(false))
                return false;
            return true;
        }

        public void disconnected(string message, int type)
        {
            stopListening();
            parent.changeStatus("Not connected!");
            GUIWindows.GUIDisconnected disconnectWindow = new GUIWindows.GUIDisconnected(message, type);
            disconnectWindow.ShowDialog();

            parent.loadLogIn();
        }

        private void loadCardsToGUI(List<Models.Card> Cards, int index)
        {
            for (int i = index; i < index + 10; i++)
            {
                if (i < Cards.Count)
                {
                    try
                    {
                        GuiImages[i - index].Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "/Images/Cards/" + Cards[i].Name + ".png", UriKind.Absolute));
                        if (GuiButtons[i - index].Visibility == Visibility.Hidden)
                            GuiButtons[i - index].Visibility = Visibility.Visible;
                    }
                    catch
                    {
                        GuiImages[i - index].Source = null;
                        GuiButtons[i - index].Visibility = Visibility.Hidden;
                    }
                }
                else
                {
                    GuiImages[i - index].Source = null;
                    GuiButtons[i - index].Visibility = Visibility.Hidden;
                }
            }
        }

        public void loadDecksToGUI()
        {
            foreach(Models.CollectionDeck deck in ctrl.Decks)
            {
                Models.DeckGUIModel dguim = new Models.DeckGUIModel(deck.DeckID, deck.DeckName, this);
                stackDecks.Children.Add(dguim.Border);
                DecksGUI.Add(dguim);
            }
        }

        private void cardImage_MouseRightButtonDown(object sender, MouseEventArgs e)
        {
            Image img = (Image)sender;
            int index = Int32.Parse(img.Name.Substring(9));
            try
            {
                tempImage.Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "/Images/Cards/" + ctrl.currentList[ctrl.currentIndex + index - 1].Name + ".png", UriKind.Absolute));
            }
            catch (Exception ex)
            {
                if (!Directory.Exists(@".\Logs"))
                    Directory.CreateDirectory(@".\Logs");

                StreamWriter file = new StreamWriter(@".\Logs\" + DateTime.Now.ToString("yyyy -dd-MM--HH-mm-ss") + Guid.NewGuid().ToString() + "_Crash_Log.txt");
                file.Write(ex.ToString());
                file.Close();
            }
            Point location = GuiImages[index - 1].TransformToAncestor(Application.Current.MainWindow).Transform(new Point(0, 0));
            tempImage.Margin = new Thickness(location.X + 127, location.Y, 0, 0);

            tempImage.Visibility = Visibility.Visible;
        }

        private void cardImage_MouseRightButtonUp(object sender, MouseEventArgs e)
        {
            tempImage.Visibility = Visibility.Hidden;
            tempImage.Source = null;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            int index = Int32.Parse(btn.Name.Substring(7));
            loadCardInfo(ctrl.currentList[ctrl.currentIndex + index - 1]);
        }

        public void loadCardInfo(Models.Card Card)
        {
            rchCardInfo.Document.Blocks.Clear();

            TextRange tr = new TextRange(rchCardInfo.Document.ContentEnd, rchCardInfo.Document.ContentEnd);
            tr.Text += "Name: ";
            tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
            tr = new TextRange(rchCardInfo.Document.ContentEnd, rchCardInfo.Document.ContentEnd);
            tr.Text += Card.Name;
            tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);
            tr = new TextRange(rchCardInfo.Document.ContentEnd, rchCardInfo.Document.ContentEnd);
            tr.Text = "\nType: ";
            tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
            tr = new TextRange(rchCardInfo.Document.ContentEnd, rchCardInfo.Document.ContentEnd);
            tr.Text = Card.Type;
            tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);
            tr = new TextRange(rchCardInfo.Document.ContentEnd, rchCardInfo.Document.ContentEnd);
            tr.Text = "\nCivilization: ";
            tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
            tr = new TextRange(rchCardInfo.Document.ContentEnd, rchCardInfo.Document.ContentEnd);
            tr.Text = Card.Civilization;
            tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);
            tr = new TextRange(rchCardInfo.Document.ContentEnd, rchCardInfo.Document.ContentEnd);
            tr.Text = "\nCost: ";
            tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
            tr = new TextRange(rchCardInfo.Document.ContentEnd, rchCardInfo.Document.ContentEnd);
            tr.Text = Card.Cost.ToString();
            tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);
            if(Card.Race!=null)
            {
                tr = new TextRange(rchCardInfo.Document.ContentEnd, rchCardInfo.Document.ContentEnd);
                tr.Text = "\nRace: ";
                tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
                tr = new TextRange(rchCardInfo.Document.ContentEnd, rchCardInfo.Document.ContentEnd);
                tr.Text = Card.Race;
                tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);
            }
            if(Card.Power!=-1)
            {
                tr = new TextRange(rchCardInfo.Document.ContentEnd, rchCardInfo.Document.ContentEnd);
                tr.Text = "\nPower: ";
                tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
                tr = new TextRange(rchCardInfo.Document.ContentEnd, rchCardInfo.Document.ContentEnd);
                tr.Text = Card.Power.ToString();
                tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);
            }
            tr = new TextRange(rchCardInfo.Document.ContentEnd, rchCardInfo.Document.ContentEnd);
            tr.Text = "\nMana Number: ";
            tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
            tr = new TextRange(rchCardInfo.Document.ContentEnd, rchCardInfo.Document.ContentEnd);
            tr.Text = Card.ManaNumber.ToString();
            tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);
            tr = new TextRange(rchCardInfo.Document.ContentEnd, rchCardInfo.Document.ContentEnd);
            tr.Text = "\nSet: ";
            tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
            tr = new TextRange(rchCardInfo.Document.ContentEnd, rchCardInfo.Document.ContentEnd);
            tr.Text = Card.Set;
            tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);
            tr = new TextRange(rchCardInfo.Document.ContentEnd, rchCardInfo.Document.ContentEnd);
            tr.Text = "\nText: ";
            tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
            if (Card.Text != null)
            {
                tr = new TextRange(rchCardInfo.Document.ContentEnd, rchCardInfo.Document.ContentEnd);
                tr.Text = Card.Text;
                tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);
            }
            rchCardInfo.ScrollToHome();
        }

        private void btnArrowRight_Click(object sender, RoutedEventArgs e)
        {
            if(ctrl.currentIndex+10 <= ctrl.currentList.Count-1)
            {
                ctrl.currentIndex += 10;
                loadCardsToGUI(ctrl.currentList, ctrl.currentIndex);
            }
        }

        private void btnArrowLeft_Click(object sender, RoutedEventArgs e)
        {
            if (ctrl.currentIndex - 10 >= 0)
            {
                ctrl.currentIndex -= 10;
                loadCardsToGUI(ctrl.currentList, ctrl.currentIndex);
            }
        }

        private void btnCreateNewDeck_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnBackToLobby_Click(object sender, RoutedEventArgs e)
        {
            ctrl.send(new Models.ClientMessage("JOINLOBBY"));
            stopListening();
            parent.loadGameLobby();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isWindowLoaded)
            {
                ctrl.filterCollection(((ComboBoxItem)cboFilterType.SelectedItem).Content.ToString(), ((ComboBoxItem)cboFilterCivilization.SelectedItem).Content.ToString(),
                    ((ComboBoxItem)cboFilterCost.SelectedItem).Content.ToString(), ((ComboBoxItem)cboFilterCostType.SelectedItem).Content.ToString(), ((ComboBoxItem)cboFilterRace.SelectedItem).Content.ToString(),
                    ((ComboBoxItem)cboFilterPower.SelectedItem).Content.ToString(), ((ComboBoxItem)cboFilterPowerType.SelectedItem).Content.ToString(), ((ComboBoxItem)cboFilterSet.SelectedItem).Content.ToString());
                loadCardsToGUI(ctrl.currentList, ctrl.currentIndex);
            }
        }

        public void SelectDeck(int ID)
        {
            selectedDeckID = ID;
            for(int i =0; i< DecksGUI.Count;i++)
                DecksGUI[i].deselect();
        }

        public void populateDeckContents(int id)
        {
            stackDeckContents.Children.Clear();
            Models.CollectionDeck deck = ctrl.getDeckByID(id);
            if(deck!= null)
            {
                foreach(Models.CollectionDeckItem cdi in deck.CardList)
                {
                    Models.DeckItemGUIModel cguim = new Models.DeckItemGUIModel(cdi, this);
                    stackDeckContents.Children.Add(cguim.Border);
                }
            }
        }

        private void insertCDIGUIdToStackPanel(Models.DeckItemGUIModel cguim)
        {
            
        }

        private void btnDeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            if (selectedDeckID == -1)
                MessageBox.Show("No deck selected.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            else
            {
                ctrl.send(new Models.ClientMessage("DELETEDECK", new List<string>() { selectedDeckID.ToString() }));
                stackDeckContents.Children.Clear();
                removeDeckFromGUI(selectedDeckID);
                ctrl.removeDeck(selectedDeckID);
                selectedDeckID = -1;
            }
        }

        private void removeDeckFromGUI(int ID)
        {
            for(int i =0;i<DecksGUI.Count;i++)
            {
                if(DecksGUI[i].ID == ID)
                {
                    stackDecks.Children.Remove(DecksGUI[i].Border);
                    DecksGUI.RemoveAt(i);
                    break;
                }
            }
        }

        private void btnEditSelected_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}