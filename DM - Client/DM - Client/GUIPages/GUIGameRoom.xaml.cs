using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using DM___Client.Animations;
using DM___Client.Models;

namespace DM___Client.GUIPages
{
    /// <summary>
    /// Interaction logic for GameRoom.xaml
    /// </summary>
    public partial class GUIGameRoom : Page
    {
        #region COMMUNICATION
        private GUIWindows.GUI parent;
        private Controllers.GameRoomController ctrl;
        private DispatcherTimer checkServerResponse;
        #endregion

        #region LISTS
        private List<Models.CardGUIModel> listHand;
        private List<Models.CardGUIModel> listOwnManaZone;
        private List<Models.CardGUIModel> listOwnBattleGround;
        private List<Models.CardGUIModel> listOwnGraveyard;
        private List<Models.CardGUIModel> listOwnSafeGuardZone;
        private List<Models.CardGUIModel> listOppManaZone;
        private List<Models.CardGUIModel> listOppBattleGround;
        private List<Models.CardGUIModel> listOppGraveyard;
        private List<Models.CardGUIModel> listOppSafeGuardZone;
        private List<Models.CardGUIModel> selectedCards;
        private List<Models.CardGUIModel> ableToSelect;
        private List<Event> animationsAndEvents;
        #endregion

        #region BUTTONS
        private Button btnPlayAsMana;
        private Button btnNextPhase;
        private Button btnPlayCard;
        private Button btnAttackSafeguards;
        private Button btnAttackCreatures;
        private Button btnEndTurn;
        private Button btnWin;
        #endregion

        private int ableToSelectLimit;
        private Image zoomedImage;
        private bool itIsOwnTurn;
        private Log.Logger logger;
        private const bool OWN = true;
        private const bool OPP = false;
        private DispatcherTimer animationTimer;
        private DispatcherTimer checkInitialAnimationsFinished;
        private object animationsAndEventsQueueLock = new object();

        public string Phase;
        public ImageSource BackgroundImageSource { get { return backgroundImage.Source; } }

        public GUIGameRoom(GUIWindows.GUI parent_, Communication com_, int GameRoomID_, int DeckID_, Models.CardCollection CardCollection_)
        {
            InitializeComponent();

            parent = parent_;
            ctrl = new Controllers.GameRoomController(this, com_, GameRoomID_, DeckID_, CardCollection_);

            logger = new Log.Logger();

            grdOwnGrave.Children.Add(new Models.CardGUIModel(null, this, AnimationAndEventsConstants.graveInitialPosition, Visibility.Hidden).Border);
            grdOppGrave.Children.Add(new Models.CardGUIModel(null, this, AnimationAndEventsConstants.graveInitialPosition, Visibility.Hidden).Border);

            initButtons();
            initTimers();
            initLists();
            initZoomedInImage();

            ctrl.loadPageData();
            beginListening();
        }

        private void initTimers()
        {
            checkServerResponse = new DispatcherTimer();
            checkServerResponse.Interval = new TimeSpan(0, 0, 0, 0, 100);
            checkServerResponse.Tick += checkServerResponse_Tick;
            animationTimer = new DispatcherTimer();
            animationTimer.Interval = new TimeSpan(0, 0, 0, 0, 50);
            animationTimer.Tick += AnimationTimer_Tick;
            checkInitialAnimationsFinished = new DispatcherTimer();
            checkInitialAnimationsFinished.Interval = new TimeSpan(0, 0, 0, 0, 300);
            checkInitialAnimationsFinished.Tick += CheckInitialAnimationsFinished_Tick;
        }

        private void initLists()
        {
            listHand = new List<Models.CardGUIModel>();
            listOwnManaZone = new List<Models.CardGUIModel>();
            listOwnBattleGround = new List<Models.CardGUIModel>();
            listOwnGraveyard = new List<Models.CardGUIModel>();
            listOwnSafeGuardZone = new List<Models.CardGUIModel>();
            listOppManaZone = new List<Models.CardGUIModel>();
            listOppBattleGround = new List<Models.CardGUIModel>();
            listOppGraveyard = new List<Models.CardGUIModel>();
            listOppSafeGuardZone = new List<Models.CardGUIModel>();
            selectedCards = new List<Models.CardGUIModel>();
            ableToSelect = new List<Models.CardGUIModel>();
            animationsAndEvents = new List<Event>();
        }

        private void initButtons()
        {
            btnPlayAsMana = getActionButton();
            btnNextPhase = getActionButton();
            btnPlayCard = getActionButton();
            btnEndTurn = getActionButton();
            btnAttackSafeguards = getActionButton();
            btnAttackCreatures = getActionButton();
            btnWin = getActionButton();
            btnWin.Content = "Attack Opponent!";
            btnWin.Click += BtnWin_Click;
            btnPlayAsMana.Content = "Play as Mana";
            btnPlayAsMana.Click += BtnPlayAsMana_Click;
            btnNextPhase.Content = "Next Phase";
            btnNextPhase.Click += BtnNextPhase_Click;
            btnPlayCard.Content = "Play Card";
            btnPlayCard.Click += BtnPlayCard_Click;
            btnAttackSafeguards.Content = "Attack Safeguards";
            btnAttackSafeguards.Click += BtnAttackSafeguards_Click;
            btnAttackCreatures.Content = "Attack Creatures";
            btnAttackCreatures.Click += BtnAttackCreatures_Click;
            btnEndTurn.Content = "End Turn";
            btnEndTurn.Click += BtnEndTurn_Click;

        }

        private void initZoomedInImage()
        {
            // create an invisibile image that will become the zoomed in image whenever auser right clicks a card from the collection
            zoomedImage = new Image();
            zoomedImage.Width = 280;
            zoomedImage.Height = 388;
            zoomedImage.VerticalAlignment = VerticalAlignment.Top;
            zoomedImage.HorizontalAlignment = HorizontalAlignment.Left;
            zoomedImage.Visibility = Visibility.Hidden;
            zoomedImage.Stretch = Stretch.UniformToFill;
            grdParent.Children.Add(zoomedImage);
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

        // starts the magic
        public void startRolling()
        {
            // we start checking the animation queue
            animationTimer.Start();

            // we start animating the begining of the match
            animateOwnSafeguards();
            animateOppSafeguards();
            animateInitialHand();

            // we start checking for the end of the initial animations
            checkInitialAnimationsFinished.Start();

            // we ask the server if we're the ones having the first move { types of response: YOURTURN | OPPTURN }
            ctrl.send(new Models.ClientMessage("GETINITIALGAMESTATE", new List<string>() { ctrl.GameRoomID.ToString() }));
        }

        // animate initial conditions

        private void animateOwnSafeguards()
        {
            Models.CardGUIModel safeGuard;

            Thickness margin = new Thickness(5, 0, 0, 0);
            for (int i = 1; i <= 5; i++)
            {
                Animations.MoveAnimation animation;

                // add the actual card
                safeGuard = new Models.CardGUIModel(null, this, AnimationAndEventsConstants.ownDeckLocation, Visibility.Hidden, i);
                grdParent.Children.Add(safeGuard.Border);

                animation = new Animations.MoveAnimation(grdParent,
                    grdOwnSafeguards,
                    grdParent,
                    null,
                    listOwnSafeGuardZone,
                    safeGuard,
                    AnimationAndEventsConstants.DESTINATIONSAFEGUARD);
                animation.setSpeed(10);
                animation.startsWithHiddenOrigin = true;
                addEvent(new Event(animation));
            }
        }

        private void animateOppSafeguards()
        {
            Models.CardGUIModel safeGuard;

            for (int i = 1; i <= 5; i++)
            {
                Animations.MoveAnimation animation;

                // add the actual card
                safeGuard = new Models.CardGUIModel(null, this, AnimationAndEventsConstants.oppDeckLocation, Visibility.Hidden, i);
                grdParent.Children.Add(safeGuard.Border);

                animation = new Animations.MoveAnimation(
                    grdParent,
                    grdOppSafeguards, 
                    grdParent, 
                    null,
                    listOppSafeGuardZone, 
                    safeGuard,
                    AnimationAndEventsConstants.DESTINATIONSAFEGUARD);
                animation.setSpeed(10);
                animation.startsWithHiddenOrigin = true;
                addEvent(new Event(animation));
            }
        }

        private void animateInitialHand()
        {
            Models.CardGUIModel card;

            for (int i = 0; i < 5; i++)
            {
                Animations.MoveAnimation animation;

                card = new Models.CardGUIModel(ctrl.getCardFromInitialHand(), this, AnimationAndEventsConstants.ownDeckLocation, Visibility.Hidden);
                grdParent.Children.Add(card.Border);

                animation = new Animations.MoveAnimation(
                    grdParent, 
                    grdHand, 
                    grdParent, 
                    null,
                    listHand,
                    card,
                    AnimationAndEventsConstants.DESTINATIONOWNHAND);
                animation.setSpeed(10);
                animation.startsWithHiddenOrigin = true;
                addEvent(new Event(animation));
            }
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

        // update game state
        public void updateGameState(bool itIsOwnTurn, string phase)
        {
            Phase = phase;
            if (itIsOwnTurn)
                lblTurn.Content = "Your turn";
            else
                lblTurn.Content = "Opponent's turn";
            this.itIsOwnTurn = itIsOwnTurn;
            lblPhase.Content = phase;
        }

        // update info board
        public void updateInfoBoard(string type, bool own, int ammount)
        {
            TextBlock value;

            switch(type)
            {
                case "mana":
                    value = own ? txtOwnMana : txtOppMana;
                    break;
                case "deck":
                    value = own ? txtOwnDeck : txtOppDeck;
                    break;
                case "hand":
                    value = own ? txtOwnHand : txtOppHand;
                    break;
                case "grave":
                    value = own ? txtOwnGrave : txtOppGrave;
                    break;
                default:
                    value = null;
                    break;
            }

            if (value != null)
                value.Text = (Int32.Parse(value.Text) + ammount).ToString();
        }

        // select/deselect methods

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
            ableToSelectLimit = count;
            ableToSelect = cards;
        }

        public bool isAbleToSelect(Models.CardGUIModel card)
        {
            if (ableToSelect.IndexOf(card) != -1)
                return true;
            return false;
        }

        // next phase

        private void BtnNextPhase_Click(object sender, RoutedEventArgs e)
        {
            switch (Phase)
            {
                case "Mana phase":
                    loadSummonPhase();
                    break;
                case "Summon phase":
                    addRunMethodEvent(new Event(loadAttackPhase));
                    break;
            }
        }

        // zoomed in image

        public void cardImage_MouseRightButtonDown(object sender, MouseEventArgs e)
        {
            Image img = (Image)sender;

            try
            {
                zoomedImage.Source = img.Source;
            }
            catch (Exception ex)
            {
                logger.Log(ex.ToString());
            }

            Point location = ((Button)(img.Parent)).TranslatePoint(new Point(0, 0), grdParent);
            if (location.X + zoomedImage.Width > parent.Width - 100)
                location.X -= zoomedImage.Width + 15;
            else
                location.X += 75;

            if (location.Y + zoomedImage.Height > parent.Height)
                location.Y -= zoomedImage.Height - 90;

            zoomedImage.Margin = new Thickness(location.X, location.Y, 0, 0);

            zoomedImage.Visibility = Visibility.Visible;
        }

        public void cardImage_MouseRightButtonUp(object sender, MouseEventArgs e)
        {
            zoomedImage.Visibility = Visibility.Hidden;
            zoomedImage.Source = null;
        }

        // animation queue

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            lock(animationsAndEventsQueueLock)
            {
                if (animationsAndEvents.Count != 0)
                {
                    switch (animationsAndEvents[0].type)
                    {
                        case AnimationAndEventsConstants.TYPEMOVE:
                            if (animationsAndEvents[0].moveAnimation.isFinished)
                                animationsAndEvents.RemoveAt(0);
                            else
                            if (!animationsAndEvents[0].moveAnimation.isRunning)
                                animationsAndEvents[0].moveAnimation.startAnimation();
                            break;
                        case AnimationAndEventsConstants.TYPEROTATE:
                            if (animationsAndEvents[0].rotateAnimation.isFinished)
                                animationsAndEvents.RemoveAt(0);
                            else
                            if (!animationsAndEvents[0].rotateAnimation.isRunning)
                                animationsAndEvents[0].rotateAnimation.startAnimation();
                            break;
                        case AnimationAndEventsConstants.TYPEALIGN:
                            if (animationsAndEvents[0].alignAnimation.isFinished)
                                animationsAndEvents.RemoveAt(0);
                            else
                            if (!animationsAndEvents[0].alignAnimation.isRunning)
                                animationsAndEvents[0].alignAnimation.startAnimation();
                            break;
                        case AnimationAndEventsConstants.TYPERUNMETHOD:
                            animationsAndEvents.RemoveAt(0);
                            animationsAndEvents[0].runMethod();
                            break;
                        case AnimationAndEventsConstants.TYPETRIGGER:
                            animationsAndEvents.RemoveAt(0);
                            animationsAndEvents[0].triggerEffect();
                            break;
                        case AnimationAndEventsConstants.TYPEPROCESSSHIELD:
                            animationsAndEvents.RemoveAt(0);
                            animationsAndEvents[0].triggerProcessShield();
                            break;
                        case AnimationAndEventsConstants.TYPEWAIT:
                            break;
                    }
                }
            }
        }

        private void addEvent(Event e, bool withWait = false)
        {
            lock (animationsAndEventsQueueLock)
            {
                if (animationsAndEvents.Count > 0 && animationsAndEvents[0].type == AnimationAndEventsConstants.TYPEWAIT)
                {
                    if (!withWait)
                        animationsAndEvents.RemoveAt(0);
                    animationsAndEvents.Insert(0, e);
                }
                else
                {
                    animationsAndEvents.Add(e);
                    if (withWait)
                        addWaitEvent();
                }
            }
        }

        private void addEvents(List<Event> events, bool withWait=false)
        {
            lock (animationsAndEventsQueueLock)
            {
                if (animationsAndEvents.Count > 0 && animationsAndEvents[0].type == AnimationAndEventsConstants.TYPEWAIT)
                {
                    if (!withWait)
                        animationsAndEvents.RemoveAt(0);
                    for (int i = 0; i < events.Count; i++)
                    {
                        animationsAndEvents.Insert(i, events[i]);
                    }
                }
                else
                {
                    foreach (Event e in events)
                    {
                        animationsAndEvents.Add(e);
                    }
                    if (withWait)
                        addWaitEvent();
                }
            }
        }

        public void addRunMethodEvent(Event e)
        {
            lock (animationsAndEventsQueueLock)
            {
                if (animationsAndEvents.Count > 0 && animationsAndEvents[0].type == AnimationAndEventsConstants.TYPEWAIT)
                {
                    animationsAndEvents.RemoveAt(0);
                    animationsAndEvents.Insert(0, e);
                }
                else
                    animationsAndEvents.Add(e);
            }
        }

        public void addTriggerEvent(SpecialEffect se, CardWithGameProperties card)
        {
            Event e = new Event(triggerEffect, se, card);

            lock (animationsAndEventsQueueLock)
            {
                if (animationsAndEvents.Count > 0 && animationsAndEvents[0].type == AnimationAndEventsConstants.TYPEWAIT)
                {
                    animationsAndEvents.Insert(0, e);
                }
                else
                {
                    animationsAndEvents.Add(e);
                    addWaitEvent();
                }
            }
        }

        public void addProcessShieldEvent(int cardID, int shieldNumber)
        {
            Event e = new Event(processShield, cardID, shieldNumber);

            lock (animationsAndEventsQueueLock)
            {
                animationsAndEvents.Add(e);
                addWaitEvent();
            }
        }

        private void addWaitEvent(bool inFront=false)
        {
            Event e = new Event(AnimationAndEventsConstants.TYPEWAIT);

            lock (animationsAndEventsQueueLock)
            {
                if (inFront)
                    animationsAndEvents.Insert(0, e);
                else
                    animationsAndEvents.Add(e);
            }
        }

        private void CheckInitialAnimationsFinished_Tick(object sender, EventArgs e)
        {
            if (animationsAndEvents.Count == 0)
            {
                checkInitialAnimationsFinished.Stop();

                if (itIsOwnTurn)
                    loadManaPhase();
            }
        }

        // Game Over

        public void loadEndGame(bool youHaveWon)
        {
            stopListening();
            ctrl.send(new Models.ClientMessage("JOINLOBBY"));
            parent.loadEndGame(youHaveWon);
        }

        private void BtnSurrender_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to surrender?", "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                ctrl.send(new Models.GameMessage("ISURRENDER"));
                loadEndGame(false);
            }
        }
    }
}
