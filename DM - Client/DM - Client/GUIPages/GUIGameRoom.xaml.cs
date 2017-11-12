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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DM___Client.GUIPages
{
    /// <summary>
    /// Interaction logic for GameRoom.xaml
    /// </summary>
    public partial class GUIGameRoom : Page
    {
        private GUIWindows.GUI parent;
        private Controllers.GameRoomController ctrl;
        private DispatcherTimer checkServerResponse = new DispatcherTimer();
        private List<Models.CardGUIModel> Hand;
        private List<Models.CardGUIModel> OwnManaZone;
        private List<Models.CardGUIModel> OwnBattleZone;
        private List<Models.CardGUIModel> OwnGraveyard;
        private List<Models.CardGUIModel> OwnShieldZone;
        private List<Models.CardGUIModel> OppManaZone;
        private List<Models.CardGUIModel> OppBattleZone;
        private List<Models.CardGUIModel> OppGraveyard;
        private List<Models.CardGUIModel> OppShieldZone;
        private Models.AnimatioUtility AU1;
        private Models.AnimatioUtility AU2;
        private DispatcherTimer chainOwnShieldAnimations = new DispatcherTimer();
        private DispatcherTimer chainOppShieldAnimations = new DispatcherTimer();
        private DispatcherTimer chainOwnHandAnimations = new DispatcherTimer();
        private DispatcherTimer PlayAsManaAnimation = new DispatcherTimer();
        private List<Models.CardGUIModel> selectedCards;
        private bool isYourTurn;
        private Button btnPlayAsMana;
        private Button btnNextPhase;
        private Button btnPlayCard;
        private Button btnEndTurn;

        public string Phase;
        public ImageSource BackgroundImageSource { get { return backgroundImage.Source; } }
        public List<Models.CardGUIModel> ableToSelect = new List<Models.CardGUIModel>();
        public int ableToSelectCount;

        public GUIGameRoom(GUIWindows.GUI parent_, Communication com_, int GameRoomID_, int DeckID_, Models.CardCollection CardCollection_)
        {
            InitializeComponent();
            parent = parent_;
            ctrl = new Controllers.GameRoomController(this, com_, GameRoomID_, DeckID_, CardCollection_);

            initButtons();
            initTimers();
            initLists();
            ctrl.loadPageData();
            beginListening();
        }

        private void PlayAsManaAnimation_Tick(object sender, EventArgs e)
        {
            PlayAsManaAnimation.Stop();
            AU1.destination.Visibility = Visibility.Visible;
            grdParent.Children.Remove(selectedCards[0].Border);
            Hand.Remove(selectedCards[0]);
            selectedCards.Clear();
        }

        private void initTimers()
        {
            checkServerResponse.Interval = new TimeSpan(0, 0, 0, 0, 100);
            checkServerResponse.Tick += checkServerResponse_Tick;
            chainOwnShieldAnimations.Interval = new TimeSpan(0, 0, 0, 0, 400);
            chainOwnShieldAnimations.Tick += ChainOwnShieldAnimations_Tick;
            chainOppShieldAnimations.Interval = new TimeSpan(0, 0, 0, 0, 400);
            chainOppShieldAnimations.Tick += ChainOppShieldAnimations_Tick;
            chainOwnHandAnimations.Interval = new TimeSpan(0, 0, 0, 0, 400);
            chainOwnHandAnimations.Tick += ChainOwnHandAnimations_Tick;
            PlayAsManaAnimation.Interval = new TimeSpan(0, 0, 0, 0, 400);
            PlayAsManaAnimation.Tick += PlayAsManaAnimation_Tick;
        }

        private void initLists()
        {
            Hand = new List<Models.CardGUIModel>();
            OwnManaZone = new List<Models.CardGUIModel>();
            OwnBattleZone = new List<Models.CardGUIModel>();
            OwnGraveyard = new List<Models.CardGUIModel>();
            OwnShieldZone = new List<Models.CardGUIModel>();
            OppManaZone = new List<Models.CardGUIModel>();
            OppBattleZone = new List<Models.CardGUIModel>();
            OppGraveyard = new List<Models.CardGUIModel>();
            OppShieldZone = new List<Models.CardGUIModel>();
            selectedCards = new List<Models.CardGUIModel>();
        }

        private void initButtons()
        {
            btnPlayAsMana = getActionButton();
            btnNextPhase = getActionButton();
            btnPlayCard = getActionButton();
            btnEndTurn = getActionButton();
            btnPlayAsMana.Content = "Play as Mana";
            btnPlayAsMana.Click += BtnPlayAsMana_Click;
            btnNextPhase.Content = "Next Phase";
            btnPlayCard.Content = "Play Card";
            btnEndTurn.Content = "End Turn";
        }

        private void BtnPlayAsMana_Click(object sender, RoutedEventArgs e)
        {
            if (selectedCards.Count != ableToSelectCount)
                MessageBox.Show("You must select a card first", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            else
            {
                Models.CardGUIModel cguim;

                selectedCards[0].Border.BorderBrush = Brushes.Transparent;
                Point newPoint = selectedCards[0].Border.TranslatePoint(new Point(0, 0), grdParent);
                grdHand.Children.Remove(selectedCards[0].Border);
                grdParent.Children.Add(selectedCards[0].Border);
                selectedCards[0].Border.VerticalAlignment = VerticalAlignment.Top;
                selectedCards[0].Border.Margin = new Thickness(newPoint.X, newPoint.Y, 0, 0);

                if (OwnManaZone.Count == 0)
                    cguim = new Models.CardGUIModel(selectedCards[0].Card, this, new Thickness(5, 0, 0, 0), Visibility.Hidden, 2);
                else
                {
                    Thickness margin = OwnManaZone[OwnManaZone.Count - 1].Border.Margin;
                    margin.Left += 80;
                    cguim = new Models.CardGUIModel(selectedCards[0].Card, this, margin, Visibility.Hidden, 2);
                }
                grdOwnMana.Children.Add(cguim.Border);
                OwnManaZone.Add(cguim);
                AU1 = new Models.AnimatioUtility(this);
                AU1.origin = selectedCards[0].Border;
                AU1.destination = cguim.Border;
                AU1.startAnimation();
                PlayAsManaAnimation.Start();
            }
        }

        private Button getActionButton()
        {
            Button btn = new Button();
            btn.Cursor = Cursors.Hand;
            btn.Opacity = 0.9;
            btn.Background = Brushes.Black;
            btn.Foreground = Brushes.White;
            btn.FontSize = 15;
            btn.Margin = new Thickness(0, 5, 0, 0);
            btn.Height = 30;
            btn.Width = 130;
            btn.HorizontalAlignment = HorizontalAlignment.Center;
            btn.Style = this.FindResource("buttonStyle") as Style;
            return btn;
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

        public void disconnected(string message, int type)
        {
            stopListening();
            parent.changeStatus("Not connected!");
            GUIWindows.GUIDisconnected disconnectWindow = new GUIWindows.GUIDisconnected(message, type);
            disconnectWindow.ShowDialog();

            parent.loadLogIn();
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

        public void startRolling()
        {
            AU1 = new Models.AnimatioUtility(this);
            AU1.animationsLeft = 5;
            chainOwnShieldAnimations.Start();
            AU2 = new Models.AnimatioUtility(this);
            AU2.animationsLeft = 5;
            chainOppShieldAnimations.Start();
        }

        private void addShieldToOwnZone()
        {
            Models.CardGUIModel cguim;
            if (OwnShieldZone.Count == 0)
                cguim = new Models.CardGUIModel(null, this, new Thickness(15, 0, 0, 0), Visibility.Hidden, 1);
            else
            {
                Thickness margin = OwnShieldZone[OwnShieldZone.Count - 1].Border.Margin;
                margin.Left += 100;
                cguim = new Models.CardGUIModel(null, this, margin, Visibility.Hidden, 1);
            }
            grdOwnShields.Children.Add(cguim.Border);
            OwnShieldZone.Add(cguim);
            cguim = new Models.CardGUIModel(null, this, new Thickness(983,463,0,149), Visibility.Visible, 0);
            grdParent.Children.Add(cguim.Border);

            AU1.origin = cguim.Border;
            AU1.destination = OwnShieldZone[OwnShieldZone.Count - 1].Border;
            AU1.startAnimation();
        }

        private void addShieldToOppZone()
        {
            Models.CardGUIModel cguim;
            if (OppShieldZone.Count == 0)
                cguim = new Models.CardGUIModel(null, this, new Thickness(15, 0, 0, 0), Visibility.Hidden, -1);
            else
            {
                Thickness margin = OppShieldZone[OppShieldZone.Count - 1].Border.Margin;
                margin.Left += 100;
                cguim = new Models.CardGUIModel(null, this, margin, Visibility.Hidden, -1);
            }
            grdOppShields.Children.Add(cguim.Border);
            OppShieldZone.Add(cguim);
            cguim = new Models.CardGUIModel(null, this, new Thickness(519, 133, 0, 479), Visibility.Visible, 0);
            grdParent.Children.Add(cguim.Border);

            AU2.origin = cguim.Border;
            AU2.destination = OppShieldZone[OppShieldZone.Count - 1].Border;
            AU2.startAnimation();
        }

        private void drawCard(Models.CardWithGameProperties card)
        {
            Models.CardGUIModel cguim;
            if (Hand.Count == 0)
                cguim = new Models.CardGUIModel(card, this, new Thickness(5, 0, 0, 0), Visibility.Hidden, 5);
            else
            {
                Thickness margin = Hand[Hand.Count - 1].Border.Margin;
                margin.Left += 25;
                cguim = new Models.CardGUIModel(card, this, margin, Visibility.Hidden, 5);
            }
            grdHand.Children.Add(cguim.Border);
            Hand.Add(cguim);
            cguim = new Models.CardGUIModel(null, this, new Thickness(983, 463, 0, 149), Visibility.Visible, 0);
            grdParent.Children.Add(cguim.Border);
            Grid.SetRow(cguim.Border, 4);
            Grid.SetColumn(cguim.Border, 8);

            AU1.origin = cguim.Border;
            AU1.destination = Hand[Hand.Count - 1].Border;
            AU1.startAnimation();
        }

        private void ChainOwnShieldAnimations_Tick(object sender, EventArgs e)
        {
            if (AU1.origin != null)
                grdParent.Children.Remove(AU1.origin);
            if (AU1.destination != null)
                AU1.destination.Visibility = Visibility.Visible;

            if (AU1.animationsLeft != 0)
            {
                addShieldToOwnZone();
                AU1.animationsLeft--;
            }
            else
            {
                chainOwnShieldAnimations.Stop();
                AU1 = new Models.AnimatioUtility(this);
                AU1.animationsLeft = 5;
                chainOwnHandAnimations.Start();
            }
        }

        private void ChainOppShieldAnimations_Tick(object sender, EventArgs e)
        {
            if (AU2.origin != null)
                grdParent.Children.Remove(AU2.origin);
            if (AU2.destination != null)
                AU2.destination.Visibility = Visibility.Visible;

            if (AU2.animationsLeft != 0)
            {
                addShieldToOppZone();
                AU2.animationsLeft--;
            }
            else
            {
                chainOppShieldAnimations.Stop();
                AU2 = null;
            }
        }

        private void ChainOwnHandAnimations_Tick(object sender, EventArgs e)
        {
            if (AU1.origin != null)
                grdParent.Children.Remove(AU1.origin);
            if (AU1.destination != null)
                AU1.destination.Visibility = Visibility.Visible;

            if (AU1.animationsLeft != 0)
            {
                drawCard(ctrl.getCardFromHand());
                AU1.animationsLeft--;
            }
            else
            {
                chainOwnHandAnimations.Stop();
                AU1 = null;
                ctrl.send(new Models.ClientMessage("GETFIRSTGAMESTATE", new List<string>() { ctrl.GameRoomID.ToString() }));
            }
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

        public void updateGameState(bool isYourTurn_, string phase)
        {
            Phase = phase;
            if (isYourTurn_)
                lblTurn.Content = "Your turn";
            else
                lblTurn.Content = "Opponent's turn";
            isYourTurn = isYourTurn_;
            lblPhase.Content = phase;
        }

        public void deselectAll()
        {
            while (selectedCards.Count > 0)
                selectedCards[0].deselect();
        }

        public void addToSelectedCards(Models.CardGUIModel card)
        {
            selectedCards.Add(card);
        }

        public void removeFromSelectedCards(Models.CardGUIModel card)
        {
            selectedCards.Remove(card);
        }

        public void setAbleToSelect(int count, List<Models.CardGUIModel> cards)
        {
            ableToSelect.Clear();
            ableToSelectCount = count;
            ableToSelect = cards;
        }

        public bool isAbleToSelect(Models.CardGUIModel card)
        {
            if (ableToSelect.IndexOf(card) != -1)
                return true;
            return false;
        }

        public void startTurn()
        {

        }

        public void endTurn()
        {


        }
        public void loadManaPhase()
        {
            updateGameState(true, "Mana phase");
            List<Models.CardGUIModel> cards = new List<Models.CardGUIModel>();
            foreach (Models.CardGUIModel card in Hand)
                cards.Add(card);
            setAbleToSelect(1, cards);
            actionButtons.Children.Clear();
            actionButtons.Children.Add(btnPlayAsMana);
            actionButtons.Children.Add(btnNextPhase);
        }

        public void loadSummonPhase()
        {

        }

        public void loadAttackPhase()
        {

        }
    }
}
