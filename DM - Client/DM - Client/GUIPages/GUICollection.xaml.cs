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

        private List<Image> listGuiImages;
        private List<Button> listGuiCardButtons;
        private List<Models.DeckGUIModel> listDecksGUI;
        private List<Models.DeckItemGUIModel> listDeckContentGUI;
        private Models.CollectionDeck deck;

        private Image zoomedImage;
        private int selectedDeckID;

        private const int COMPARELESSTHANOREQUAL = 8;
        private const int COMPAREGREATERTHANOREQUAL = 9;
        private const int COMPAREEQUAL = 10;

        private bool editModeON;

        private Log.Logger logger;

        public ImageSource BackgroundImageSource { get { return backgroundImage.Source; } }

        public GUICollection(GUIWindows.GUI parent_, Communication com_, Models.CardCollection CardCollection_)
        {
            InitializeComponent();

            logger = new Log.Logger();

            // attach parent
            parent = parent_;

            // attach controller
            ctrl = new Controllers.CollectionController(this, com_, CardCollection_);

            // init uninitialized data

            listDecksGUI = new List<Models.DeckGUIModel>();
            listDeckContentGUI = new List<Models.DeckItemGUIModel>();
            selectedDeckID = -1;
            editModeON = false;

            // create an invisibile image that will become the zoomed in image whenever auser right clicks a card from the collection
            zoomedImage = new Image();
            zoomedImage.Width = 250;
            zoomedImage.Height = 346;
            zoomedImage.VerticalAlignment = VerticalAlignment.Top;
            zoomedImage.HorizontalAlignment = HorizontalAlignment.Left;
            zoomedImage.Visibility = Visibility.Hidden;
            zoomedImage.Stretch = Stretch.Fill;
            grdParent.Children.Add(zoomedImage);

            // add all card images and attached buttons to two lists for easy access
            listGuiImages = new List<Image>() { cardImage1, cardImage2, cardImage3, cardImage4, cardImage5, cardImage6, cardImage7, cardImage8, cardImage9, cardImage10 };
            listGuiCardButtons = new List<Button>() { btnCard1, btnCard2, btnCard3, btnCard4, btnCard5, btnCard6, btnCard7, btnCard8, btnCard9, btnCard10 };

            // load the first 10 cards in the collection to GUI
            loadCardsToGUI(ctrl.filteredCollection, ctrl.currentIndex);

            // init server response timer
            checkServerResponse.Interval = new TimeSpan(0, 0, 0, 0, 250);
            checkServerResponse.Tick += checkServerResponse_Tick;

            // start loading page data and start listening
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

        public List<bool> getLoadedDataChecklist()
        {
            return ctrl.getLoadedDataChecklist();
        }

        public bool DoneLoading()
        {
            if (ctrl.getLoadedDataChecklist().Contains(false))
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

        // load cards to GUI

        private void loadCardsToGUI(List<Models.Card> Cards, int index)
        {
            for (int i = index; i < index + 10; i++)
            {
                if (i < Cards.Count)
                {
                    try
                    {
                        listGuiImages[i - index].Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "/Images/Cards/" + Cards[i].Name + ".png", UriKind.Absolute));
                        if (listGuiCardButtons[i - index].Visibility == Visibility.Hidden)
                            listGuiCardButtons[i - index].Visibility = Visibility.Visible;
                    }
                    catch
                    {
                        listGuiImages[i - index].Source = null;
                        listGuiCardButtons[i - index].Visibility = Visibility.Hidden;
                    }
                }
                else
                {
                    listGuiImages[i - index].Source = null;
                    listGuiCardButtons[i - index].Visibility = Visibility.Hidden;
                }
            }
        }

        // load decks to GUI

        public void loadDecksToGUI()
        {
            foreach (Models.CollectionDeck deck in ctrl.Decks)
            {
                Models.DeckGUIModel dguim = new Models.DeckGUIModel(deck.DeckID, deck.DeckName, this);
                stackDecks.Children.Add(dguim.Border);
                listDecksGUI.Add(dguim);
            }
        }

        // zoomed in image

        private void cardImage_MouseRightButtonDown(object sender, MouseEventArgs e)
        {
            Image img = (Image)sender;
            int index = Int32.Parse(img.Name.Substring(9));
            try
            {
                zoomedImage.Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "/Images/Cards/" + ctrl.filteredCollection[ctrl.currentIndex + index - 1].Name + ".png", UriKind.Absolute));
            }
            catch (Exception ex)
            {
                logger.Log(ex.ToString());
            }
            Point location = listGuiImages[index - 1].TransformToAncestor(Application.Current.MainWindow).Transform(new Point(0, 0));
            zoomedImage.Margin = new Thickness(location.X + 127, location.Y, 0, 0);

            zoomedImage.Visibility = Visibility.Visible;
        }

        public void displayZoomedInImage(string cardName, Border border)
        {
            try
            {
                zoomedImage.Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "/Images/Cards/" + cardName + ".png", UriKind.Absolute));
            }
            catch (Exception ex)
            {
                logger.Log(ex.ToString());
            }
            Point location = border.TranslatePoint(new Point(0,0), grdParent);

            if (location.Y + zoomedImage.Height > parent.Height - 50)
                location.Y -= location.Y + zoomedImage.Height - parent.Height + 50;

            zoomedImage.Margin = new Thickness(location.X - 260, location.Y, 0, 0);
            zoomedImage.Visibility = Visibility.Visible;
        }

        private void cardImage_MouseRightButtonUp(object sender, MouseEventArgs e)
        {
            hideZoomedInImage();
        }

        public void hideZoomedInImage()
        {
            zoomedImage.Visibility = Visibility.Hidden;
            zoomedImage.Source = null;
        }

        // load card info

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
            tr.Text = "\nElement: ";
            tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
            tr = new TextRange(rchCardInfo.Document.ContentEnd, rchCardInfo.Document.ContentEnd);
            tr.Text = Card.Element;
            tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);
            tr = new TextRange(rchCardInfo.Document.ContentEnd, rchCardInfo.Document.ContentEnd);
            tr.Text = "\nCost: ";
            tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
            tr = new TextRange(rchCardInfo.Document.ContentEnd, rchCardInfo.Document.ContentEnd);
            tr.Text = Card.Cost.ToString();
            tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);
            if (Card.Race != null)
            {
                tr = new TextRange(rchCardInfo.Document.ContentEnd, rchCardInfo.Document.ContentEnd);
                tr.Text = "\nRace: ";
                tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
                tr = new TextRange(rchCardInfo.Document.ContentEnd, rchCardInfo.Document.ContentEnd);
                tr.Text = Card.Race;
                tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);
            }
            if (Card.Power != -1)
            {
                tr = new TextRange(rchCardInfo.Document.ContentEnd, rchCardInfo.Document.ContentEnd);
                tr.Text = "\nPower: ";
                tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
                tr = new TextRange(rchCardInfo.Document.ContentEnd, rchCardInfo.Document.ContentEnd);
                tr.Text = Card.Power.ToString();
                tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);
            }
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

        // move right inside collection

        private void btnArrowRight_Click(object sender, RoutedEventArgs e)
        {
            if (ctrl.currentIndex + 10 <= ctrl.filteredCollection.Count - 1)
            {
                ctrl.currentIndex += 10;
                loadCardsToGUI(ctrl.filteredCollection, ctrl.currentIndex);
            }
        }

        // move left inside collection

        private void btnArrowLeft_Click(object sender, RoutedEventArgs e)
        {
            if (ctrl.currentIndex - 10 >= 0)
            {
                ctrl.currentIndex -= 10;
                loadCardsToGUI(ctrl.filteredCollection, ctrl.currentIndex);
            }
        }

        // menu buttons

        private void btnCreateNewDeck_Click(object sender, RoutedEventArgs e)
        {
            string deckName;
            GUIWindows.GUICreateDeck gUICreateDeck;

            gUICreateDeck = new GUIWindows.GUICreateDeck();
            gUICreateDeck.ShowDialog();

            deckName = gUICreateDeck.deckName;

            if (deckName != null)
            {
                ctrl.send(new Models.ClientMessage("CREATEDECK", new List<string>() { deckName }));
            }
        }

        private void btnEditSelected_Click(object sender, RoutedEventArgs e)
        {
            if (selectedDeckID != -1)
            {
                if (editModeON)
                {
                    editModeON = false;
                    btnEditSelected.Content = "Edit deck";
                    ctrl.updateDeck(deck);
                    sendUpdateDeck();
                }
                else
                {
                    editModeON = true;
                    btnEditSelected.Content = "Save deck";

                }
            }
            else
                MessageBox.Show("No deck selected.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void sendUpdateDeck()
        {
            Models.ClientMessage clientMessage = new Models.ClientMessage();
            clientMessage.Command = "UPDATEDECK";
            clientMessage.intArguments = new List<int>() { (deck.DeckID) };
            clientMessage.stringArguments = new List<string>() { ctrl.collectionDeckToStringArgument(deck) };

            ctrl.send(clientMessage);
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

        private void btnBackToLobby_Click(object sender, RoutedEventArgs e)
        {
            stopListening();
            ctrl.send(new Models.ClientMessage("JOINLOBBY"));
            parent.loadGameLobby();
        }

        // filter collection

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string comboBoxContent;
            string cardType;
            string cardElement;
            int cardCost;
            int cardCostComparator;
            int cardPower;
            int cardPowerComparator;

            if (e.RemovedItems.Count != 0)
            {
                // cardType

                cardType = ((ComboBoxItem)cboFilterCardType.SelectedItem).Content.ToString();

                // cardElement

                cardElement = ((ComboBoxItem)cboFilterElement.SelectedItem).Content.ToString();

                // cardCost

                comboBoxContent = ((ComboBoxItem)cboFilterCost.SelectedItem).Content.ToString();
                if (comboBoxContent == "Any")
                    cardCost = -1;
                else
                    cardCost = Int32.Parse(comboBoxContent);

                // cardCost comparator

                comboBoxContent = ((ComboBoxItem)cboFilterCostComparator.SelectedItem).Content.ToString();

                if (comboBoxContent == ">=")
                    cardCostComparator = COMPAREGREATERTHANOREQUAL;
                else
                if (comboBoxContent == "<=")
                    cardCostComparator = COMPARELESSTHANOREQUAL;
                else
                    cardCostComparator = COMPAREEQUAL;

                // cardPower

                comboBoxContent = ((ComboBoxItem)cboFilterPower.SelectedItem).Content.ToString();

                if (comboBoxContent == "Any")
                    cardPower = -1;
                else
                    cardPower = Int32.Parse(comboBoxContent);

                // cardPowerComparator

                comboBoxContent = ((ComboBoxItem)cboFilterPowerType.SelectedItem).Content.ToString();

                if (comboBoxContent == ">=")
                    cardPowerComparator = COMPAREGREATERTHANOREQUAL;
                else
                if (comboBoxContent == "<=")
                    cardPowerComparator = COMPARELESSTHANOREQUAL;
                else
                    cardPowerComparator = COMPAREEQUAL;

                ctrl.filterCollection(cardType, cardElement, cardCost, cardCostComparator, cardPower, cardPowerComparator);
                loadCardsToGUI(ctrl.filteredCollection, ctrl.currentIndex);
            }
        }

        // Select deck

        public void SelectDeck(int ID)
        {
            selectedDeckID = ID;
            populateDeckContents(ID);
        }

        // Deselect deck

        public void DeselectDeck()
        {
            selectedDeckID = -1;
            stackDeckContents.Children.Clear();
        }

        public void deselectAll()
        {
            foreach (Models.DeckGUIModel deck in listDecksGUI)
                deck.deselect();
        }

        // whenever you select a deck

        private void populateDeckContents(int id)
        {
            Models.CollectionDeck deck;

            stackDeckContents.Children.Clear();
            listDeckContentGUI.Clear();
            deck = ctrl.getDeckByID(id);

            this.deck = deck.Clone();

            if (deck != null)
            {
                foreach (Models.CollectionDeckItem cdi in deck.CardList)
                {
                    Models.DeckItemGUIModel cguim = new Models.DeckItemGUIModel(cdi, this);
                    listDeckContentGUI.Add(cguim);
                    stackDeckContents.Children.Add(cguim.Border);
                }
            }
        }

        // creates a new Deck item

        public void createNewDeckGUI(int deckID, string deckName)
        {
            Models.DeckGUIModel dguim = new Models.DeckGUIModel(deckID, deckName, this);
            stackDecks.Children.Add(dguim.Border);
            listDecksGUI.Add(dguim);
        }

        // removes a deck item

        private void removeDeckFromGUI(int ID)
        {
            for (int i = 0; i < listDecksGUI.Count; i++)
            {
                if (listDecksGUI[i].ID == ID)
                {
                    stackDecks.Children.Remove(listDecksGUI[i].Border);
                    listDecksGUI.RemoveAt(i);
                    break;
                }
            }
        }

        // click on a card in collection

        private void cardButton_Click(object sender, RoutedEventArgs e)
        {
            Models.Card card;

            Button btn = (Button)sender;
            int index = Int32.Parse(btn.Name.Substring(7));

            card = ctrl.filteredCollection[ctrl.currentIndex + index - 1];
            loadCardInfo(card);

            if (editModeON)
            {
                addCardToDeck(card);
            }
        }

        // add a new card to deck while in Edit mode

        private void addCardToDeck(Models.Card card)
        {
            bool found;

            if (deck.Count == 40)
            {
                MessageBox.Show("This deck contains 40 cards. You cannot add any more", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (deck.addCard(card))
            {
                found = false;
                foreach (Models.DeckItemGUIModel dguim in listDeckContentGUI)
                {
                    if (dguim.Card.ID == card.ID)
                    {
                        dguim.increaseCount();
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    Models.CollectionDeckItem collectionDeckItem = deck.getCollectionDeckItemByID(card.ID);
                    Models.DeckItemGUIModel cguim = new Models.DeckItemGUIModel(collectionDeckItem, this);
                    listDeckContentGUI.Add(cguim);
                    stackDeckContents.Children.Add(cguim.Border);
                }
            }
            else
                MessageBox.Show("You can only add a maximum of 4 copies of any card.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        // click on card in deck

        public void deckItem_Clicked(Models.Card card)
        {
            loadCardInfo(card);

            if (editModeON)
            {
                removeCardFromDeck(card);
            }
        }

        private void removeCardFromDeck(Models.Card card)
        {
            Models.DeckItemGUIModel deckItemGUIModel;

            deckItemGUIModel = null;

            foreach (Models.DeckItemGUIModel dguim in listDeckContentGUI)
                if (dguim.Card.ID == card.ID)
                {
                    deckItemGUIModel = dguim;
                    break;
                }

            if (deck.removeCard(card))
            {
                deckItemGUIModel.decreaseCount();
            }
            else
            {
                listDeckContentGUI.Remove(deckItemGUIModel);
                stackDeckContents.Children.Remove(deckItemGUIModel.Border);
            }
        }
    }
}