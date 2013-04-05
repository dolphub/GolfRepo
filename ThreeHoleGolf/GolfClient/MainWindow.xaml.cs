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
using System.ServiceModel;
using GolfLibrary;
using System.Timers;

namespace GolfClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant,
        UseSynchronizationContext = false)]
    public partial class MainWindow : Window, IGameCallBack
    {
        // Some local variables used by the client
        private IGameSystem gameSystem = null;
        private string card = null;
        private string usrName = "";
        private DispatcherTimer timer;
        private DispatcherTimer endTimer;
        private int timerCounter = 5;
        private Dictionary<string, bool> allCardsFlipped;
        /// <summary>
        /// Client Constructor
        /// </summary>
        public MainWindow()
        {
            try
            {
                // configure the ABCs of using the cardsLibrary as a service
                DuplexChannelFactory<IGameSystem> channel = new DuplexChannelFactory<IGameSystem>(this, "Game");
                //Activate the game
                gameSystem = channel.CreateChannel();

                //Get Username from player
                Login login = new Login();
                int counter = 0;
                do
                {
                    if (counter > 0)
                        MessageBox.Show("Invalid Username. Please try another.");
                    counter++;
                    login.ShowDialog();
                }
                while (!(gameSystem.Join(login.tb_username.Text)));

                // Build the client
                InitializeComponent();
                allCardsFlipped = new Dictionary<string, bool>();
                // Set the queue timer for players starting game
                timer = new DispatcherTimer();
                timer.Tick += new EventHandler(timer_Tick);
                timer.Interval = new TimeSpan(0, 0, 1);

                // Set the Timer object for end of game logic
                endTimer = new DispatcherTimer();
                endTimer.Tick += new EventHandler(endgame_timer);
                endTimer.Interval = new TimeSpan(0, 0, 1);

                // Set up the clients for the pre-game room
                PreGame();
                lbl_userName.Content += this.formatName(login.tb_username.Text);
                usrName = formatName(login.tb_username.Text);
                this.Title = usrName; // Set the username of this client to the Username Given
            }
            catch (Exception ex)
            {
                this.Close();
                MessageBox.Show(ex.Message);
            }

        }
        /// <summary>
        /// Pre game sets up the client for the pre game 
        /// Room, the room everyone sees waiting for a game
        /// </summary>
        private void PreGame()
        {
            this.GameTable.IsEnabled = false;
            this.userCardGrid.IsEnabled = false;
            this.userCardGrid.Opacity = .5;
            this.GameTable.Opacity = .5;

            this.btn_Ready.Content = "Ready";
            this.btn_drawnCard.Visibility = System.Windows.Visibility.Hidden;

            this.UsersBorder.BorderBrush = Brushes.Black;
            this.UsersBorder.BorderThickness = new System.Windows.Thickness(2);


            // Reset all clients to all cards face down
            foreach (PlayerTemplate pt in PlayerGrid.Children.OfType<PlayerTemplate>())
            {
                pt.PlayerBorder.BorderBrush = Brushes.Black;
                pt.PlayerBorder.BorderThickness = new System.Windows.Thickness(2);
            }


            // Set the clients cards face down
            (btn_card1.FindName("face1") as Image).Visibility = System.Windows.Visibility.Hidden;
            (btn_card1.FindName("back1") as Image).Visibility = System.Windows.Visibility.Visible;

            (btn_card2.FindName("face2") as Image).Visibility = System.Windows.Visibility.Hidden;
            (btn_card2.FindName("back2") as Image).Visibility = System.Windows.Visibility.Visible;

            (btn_card3.FindName("face3") as Image).Visibility = System.Windows.Visibility.Hidden;
            (btn_card3.FindName("back3") as Image).Visibility = System.Windows.Visibility.Visible;

            allCardsFlipped[btn_card1.Name.ToString()] = false;
            allCardsFlipped[btn_card2.Name.ToString()] = false;
            allCardsFlipped[btn_card3.Name.ToString()] = false;
        }
        /// <summary>
        /// Logic for starting a game
        /// This is what the user will see when an 
        /// game first starts
        /// </summary>
        private void StartGame()
        {
            this.btn_Ready.Visibility = System.Windows.Visibility.Hidden;
            this.btn_card1.Visibility = System.Windows.Visibility.Visible;
            this.btn_card2.Visibility = System.Windows.Visibility.Visible;
            this.btn_card3.Visibility = System.Windows.Visibility.Visible;

            foreach (PlayerTemplate pt in PlayerGrid.Children.OfType<PlayerTemplate>())
            {
                (pt.FindName("btn_card1") as Button).Visibility = System.Windows.Visibility.Visible;
                (pt.FindName("btn_card2") as Button).Visibility = System.Windows.Visibility.Visible;
                (pt.FindName("btn_card3") as Button).Visibility = System.Windows.Visibility.Visible;

                (pt.btn_card1.FindName("face1") as Image).Visibility = System.Windows.Visibility.Hidden;
                (pt.btn_card2.FindName("face2") as Image).Visibility = System.Windows.Visibility.Hidden;
                (pt.btn_card3.FindName("face3") as Image).Visibility = System.Windows.Visibility.Hidden;
                (pt.btn_card1.FindName("back1") as Image).Visibility = System.Windows.Visibility.Visible;
                (pt.btn_card2.FindName("back2") as Image).Visibility = System.Windows.Visibility.Visible;
                (pt.btn_card3.FindName("back3") as Image).Visibility = System.Windows.Visibility.Visible;
            }

            (btn_card1.FindName("face1") as Image).Visibility = System.Windows.Visibility.Hidden;
            (btn_card1.FindName("back1") as Image).Visibility = System.Windows.Visibility.Visible;

            (btn_card2.FindName("face2") as Image).Visibility = System.Windows.Visibility.Hidden;
            (btn_card2.FindName("back2") as Image).Visibility = System.Windows.Visibility.Visible;

            (btn_card3.FindName("face3") as Image).Visibility = System.Windows.Visibility.Hidden;
            (btn_card3.FindName("back3") as Image).Visibility = System.Windows.Visibility.Visible;

            DrawThreeCards();
            this.GameTable.IsEnabled = true;
            this.GameTable.Opacity = 1.0;
            this.userCardGrid.IsEnabled = true;
            this.userCardGrid.Opacity = 1.0;

            foreach (PlayerTemplate pt in PlayerGrid.Children.OfType<PlayerTemplate>())
                (pt.FindName("ReadyImage") as Image).Visibility = System.Windows.Visibility.Visible;


            gameSystem.StartGame();
        }
        /// <summary>
        /// Event Handler for our Queue timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_Tick(object sender, EventArgs e)
        {
            // Notify the clients that everone is ready
            // once the countdown hits 0, start the game
            Message("All players ready! Game will start in " + timerCounter + "...");
            timerCounter -= 1;
            if (timerCounter < 0)
            {
                timer.Stop();
                StartGame();
            }
        }
        /// <summary>
        /// Event Handler for our end game timer
        /// The timer displays the results after a few seconds, and then
        /// resets all of the clients for a new game
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void endgame_timer(object sender, EventArgs e)
        {
            timerCounter -= 1;
            if (timerCounter == 8)
            {
                gameSystem.GetResults();
            }
            if (timerCounter == 0)
            {
                endTimer.Stop();
                gameSystem.ResetGame();
            }
        }
        /// <summary>
        /// Send a message to the board
        /// </summary>
        /// <param name="msg"></param>
        private void Message(string msg)
        {
            this.GameMessageLbl.Content = msg;
        }
        /// <summary>
        /// Draw three cards for the client
        /// </summary>
        private void DrawThreeCards()
        {
            List<string> startingCards;
            try
            {
                startingCards = gameSystem.DrawThreeCards();
                for (int i = 1; i <= 3; i++) // Loop through our cards collection and assign them to the clients cards
                    (btn_card1.FindName("face" + i) as Image).Source = new BitmapImage(new Uri(@"\Images\Cards\" + startingCards[i - 1] + ".jpg", UriKind.RelativeOrAbsolute));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// Draw a card from the deck
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_blindDeck_Click(object sender, RoutedEventArgs e)
        {
            if (btn_drawnCard.Visibility == Visibility.Hidden)
                //Draw Card
                card = gameSystem.Draw();
        }
        /// <summary>
        /// Event Handler to allow button to be start dragged
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Button b = sender as Button;
            DataObject dataObject = new DataObject(b);
            DragDrop.DoDragDrop(b, dataObject, DragDropEffects.Move);
        }
        /// <summary>
        /// Event Handler for when an item has been dropped onto this button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_discardDeck_Drop(object sender, DragEventArgs e)
        {
            Button draggedFromButton = e.Data.GetData(typeof(Button)) as Button;
            // Make sure that the button being dragged onto, is not itself
            if (btn_discardDeck != draggedFromButton)
            {
                //Prevent the user from multiple clicks on the this image
                if ((btn_drawnCard.FindName("facedrawnCard") as Image).Source != null)
                {
                    btn_discardDeck.PreviewMouseLeftButtonDown += btn_PreviewMouseLeftButtonDown;
                    string[] temp = ((btn_drawnCard.FindName("facedrawnCard") as Image).Source.ToString().Split('.')[0]).Split('/');
                    gameSystem.DiscardedCard = temp[temp.Length - 1];



                    gameSystem.UpdateTurn();
                }
            }
        }
        /// <summary>
        /// Event Handler for when an item has been dropped
        /// onto any of the bottom cards
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_discardDeck_DropBottomCards(object sender, DragEventArgs e)
        {
            //Get the button that the image was dragged from
            Button draggedFromButton = e.Data.GetData(typeof(Button)) as Button;
            string name = "";
            Image draggedFrom_imgBack = new Image();
            Image draggedFrom_imgFront = new Image();

            try
            {
                Grid dataFrom = draggedFromButton.Content as Grid;
                draggedFrom_imgBack = dataFrom.Children[0] as Image;
                draggedFrom_imgFront = dataFrom.Children[1] as Image;
                name = draggedFrom_imgFront.Name;
            }
            catch (Exception) { }


            //Get the button that the image was dropped onto
            Button draggedToButton = sender as Button;
            Grid data = draggedToButton.Content as Grid;
            Image imgFront = data.Children[1] as Image;
            Image imgBack = data.Children[0] as Image;
            string objectName = imgFront.Name; // name of the x:name image
            string backName = imgBack.Name;// not needed
            string buttonName = draggedToButton.Name; // Button name we dragged to
            string[] tempfrontName = imgFront.Source.ToString().Split('/');
            //string newSwapImg = tempfrontName[tempfrontName.Length - 1].Split('.')[0]; // name of the card

            string oldSwapImg = "";

            //Set the bottom card to the image of the drawn card that was dragged down
            if (name != "")
            {
                //Draged from the discard pile
                //Set the user card image to the discarded card image
                //gameSystem.ContestentSwap(this.usrName, 
                string[] tempOldCard = (draggedToButton.FindName(objectName) as Image).Source.ToString().Split('/');
                oldSwapImg = tempOldCard[tempOldCard.Length - 1].Split('.')[0];

                string[] tempNewCard = ((draggedFromButton.FindName("facediscardDeck") as Image).Source.ToString().Split('.')[0]).Split('/');
                string newSwapImg = tempNewCard[tempNewCard.Length - 1];

                //if ((draggedToButton.FindName(objectName) as Image).Visibility != System.Windows.Visibility.Hidden)
                //    gameSystem.Points(-gameSystem.CardValue(oldSwapImg), usrName);



                gameSystem.ContestentSwap(this.usrName, newSwapImg, oldSwapImg, true, buttonName, objectName);

            }
            else
            {
                string[] tempOldCard = (draggedToButton.FindName(objectName) as Image).Source.ToString().Split('/');
                oldSwapImg = tempOldCard[tempOldCard.Length - 1].Split('.')[0];

                string[] tempNewCard = ((draggedFromButton.FindName("facedrawnCard") as Image).Source.ToString().Split('.')[0]).Split('/');
                string newSwapImg = tempNewCard[tempNewCard.Length - 1];

                //if ((draggedToButton.FindName(objectName) as Image).Visibility != System.Windows.Visibility.Hidden)
                //    gameSystem.Points(-gameSystem.CardValue(oldSwapImg), usrName);



                gameSystem.ContestentSwap(this.usrName, newSwapImg, oldSwapImg, false, buttonName, objectName);
            }



            allCardsFlipped[buttonName] = true;

            bool lastTurn = true;
            allCardsFlipped.All(c =>
            {
                if (!c.Value)
                    lastTurn = false;
                return true;
            });

            if (lastTurn && !gameSystem.LastRound)
                gameSystem.LastRound = true;

            gameSystem.UpdateTurn();


        }
        /// <summary>
        /// Find all turned over cards, and send them to calculate, and update the player points
        /// </summary>
        private void UpdatePoints()
        {
            List<string> cards = new List<string>();
            string[] temp;
            if ((this.btn_card1.FindName("face1") as Image).Visibility == System.Windows.Visibility.Visible)
            {
                temp = (this.btn_card1.FindName("face1") as Image).Source.ToString().Split('/');
                cards.Add(temp[temp.Length - 1].Split('.')[0]);
            }

            if ((this.btn_card2.FindName("face2") as Image).Visibility == System.Windows.Visibility.Visible)
            {
                temp = (this.btn_card2.FindName("face2") as Image).Source.ToString().Split('/');
                cards.Add(temp[temp.Length - 1].Split('.')[0]);
            }

            if ((this.btn_card3.FindName("face3") as Image).Visibility == System.Windows.Visibility.Visible)
            {
                temp = (this.btn_card3.FindName("face3") as Image).Source.ToString().Split('/');
                cards.Add(temp[temp.Length - 1].Split('.')[0]);
            }
            gameSystem.Points(cards.ToArray(), this.usrName);
        }
        /// <summary>
        /// Format a name to first letter uppercase
        /// and rest of the letters lowercase
        /// </summary>
        /// <param name="_raw"></param>
        /// <returns></returns>
        private string formatName(string _raw)
        {
            string refined = _raw;
            if (_raw.Length > 1)
                return _raw[0].ToString().ToUpper() + _raw.Substring(1, _raw.Length - 1).ToLower();
            else
                return _raw;
        }
        //Create a popup for the help menu
        private void btn_help_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.Form form = new System.Windows.Forms.Form();

            form.Text = "Help Menu";
            form.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;

            form.Width = 600;
            form.Height = 500;

            System.Windows.Forms.RichTextBox rtb = new System.Windows.Forms.RichTextBox();
            rtb.Enabled = false;
            rtb.Width = 590;
            rtb.Height = 500;

            rtb.Text = "The Play\n\n" +
                "The player to the dealer's left begins, and the turn to play passes clockwise. " +
                "At your turn you must either draw the top card of the face-down stock, or draw the top discard. " +
                "You may use the card you draw to replace any one of the six cards of your layout, but if you choose to replace a face-down " +
                "card you are not allowed to look at it before deciding to replace it. You place the new card face-up in your layout, and the " +
                "card that previously occupied that position is placed face-up on top of the discard pile. It is then the next player's turn. " +
                "If you draw a card from the face-down card from the stock, you may decide that you do not want it anywhere in your layout. " +
                "In that case you simply discard the drawn card face-up on the discard pile, and it is the next player's turn. It is, however, " +
                "illegal to draw the top card of the discard pile and discard the same card again, leaving the situation unchanged: if you choose to " +
                "take the discard, you must use it to replace one of your layout cards. " +
                "The play ends as soon as the last of a player's six cards is face up. The hand is then scored.\n\n"
                + "Scoring\n\n" +
                "At the end of the play, each player's layout of six cards is turned face-up and scored as follows.\n" +
                "Each ace counts 1 point.\n" + "Each two counts minus two points.\n" + "Each numeral card from 3 to 10 scores face value.\n" +
                "Each Jack or Queen scores 10 points.\n" + "Each King scores zero points.\n" +
                "Each Joker scores -2 points.\n" +
                "A pair of equal cards in the same column scores zero points for the column (even if the equal cards are twos).\n" +
                "The player who has the lowest cumulative score after nine deals wins.";

            form.Controls.Add(rtb);
            form.ShowDialog();

        }
        // The delegates for 
        #region CallBack Delegates
        // Send results information to all clients
        private delegate void SendResultsDelegate(string[] names, int[] points);
        public void sendResults(string[] names, int[] points)
        {
            if (this.Dispatcher.Thread == System.Threading.Thread.CurrentThread)
            {
                try
                {
                    ResultsPage rp = new ResultsPage();
                    rp.LoadResults(names, points);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
                this.Dispatcher.BeginInvoke(new SendResultsDelegate(sendResults), new object[] { names, points });
        }
        // When the game is about to end, update certain fields
        private delegate void GameEndingDelegate();
        public void GameEnding()
        {
            if (this.Dispatcher.Thread == System.Threading.Thread.CurrentThread)
            {
                try
                {
                    string[] temp;
                    // If there are any cards not turned over in your hand
                    // turn them over, send the update to all the other clients
                    // so that everyone can see everyones hands
                    // and then update points subsequently
                    if ((this.btn_card1.FindName("face1") as Image).Visibility == System.Windows.Visibility.Hidden)
                    {
                        temp = (this.btn_card1.FindName("face1") as Image).Source.ToString().Split('/');
                        string newcard = (temp[temp.Length - 1].Split('.')[0]);
                        temp = (this.btn_discardDeck.FindName("facediscardDeck") as Image).Source.ToString().Split('/');
                        string oldcard = (temp[temp.Length - 1].Split('.')[0]);
                        gameSystem.ContestentSwap(this.usrName, newcard, oldcard, true, "btn_card1", "face1");
                    }
                    if ((this.btn_card2.FindName("face2") as Image).Visibility == System.Windows.Visibility.Hidden)
                    {
                        temp = (this.btn_card2.FindName("face2") as Image).Source.ToString().Split('/');
                        string newcard = (temp[temp.Length - 1].Split('.')[0]);
                        temp = (this.btn_discardDeck.FindName("facediscardDeck") as Image).Source.ToString().Split('/');
                        string oldcard = (temp[temp.Length - 1].Split('.')[0]);
                        gameSystem.ContestentSwap(this.usrName, newcard, oldcard, true, "btn_card2", "face2");
                    }
                    if ((this.btn_card3.FindName("face3") as Image).Visibility == System.Windows.Visibility.Hidden)
                    {
                        temp = (this.btn_card3.FindName("face3") as Image).Source.ToString().Split('/');
                        string newcard = (temp[temp.Length - 1].Split('.')[0]);
                        temp = (this.btn_discardDeck.FindName("facediscardDeck") as Image).Source.ToString().Split('/');
                        string oldcard = (temp[temp.Length - 1].Split('.')[0]);
                        gameSystem.ContestentSwap(this.usrName, newcard, oldcard, true, "btn_card3", "face3");
                    }
                    gameSystem.GameState(); // Update game state
                    this.btn_blindDeck.IsEnabled = false;
                    this.btn_discardDeck.IsEnabled = false;
                    Message("Game Over!");
                    timerCounter = 10;
                    endTimer.Start();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
                this.Dispatcher.BeginInvoke(new GameEndingDelegate(GameEnding), new object[] { });
        }
        // Concatenate a message to the board display
        private delegate void SendMessageDelegate(string msg);
        public void SendMessage(string msg)
        {
            if (this.Dispatcher.Thread == System.Threading.Thread.CurrentThread)
            {
                try
                {
                    this.GameMessageLbl.Content += Environment.NewLine + msg;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
                this.Dispatcher.BeginInvoke(new SendMessageDelegate(SendMessage), new object[] { msg });
        }
        // Delagate and function for when a player draws a new card
        private delegate void UpdateDrawnDelegate(string drawn);
        public void UpdateDrawn(string _drawn)
        {
            if (this.Dispatcher.Thread == System.Threading.Thread.CurrentThread)
            {
                try
                {
                    // Show the card that we drew
                    btn_drawnCard.Visibility = Visibility.Visible;
                    (btn_drawnCard.FindName("facedrawnCard") as Image).Source = new BitmapImage(new Uri(@"\Images\Cards\" + _drawn + ".jpg", UriKind.RelativeOrAbsolute));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
                this.Dispatcher.BeginInvoke(new UpdateDrawnDelegate(UpdateDrawn), new object[] { _drawn });
        }
        // Updates your queue and everyones playergrid
        private delegate void UpdateQueueDelegate(string username, bool isReady, bool gameStart, bool hasEnoughPlayers);
        public void UpdateQueue(string username, bool isReady, bool gameStart, bool hasEnoughPlayers)
        {
            if (this.Dispatcher.Thread == System.Threading.Thread.CurrentThread)
            {
                try
                {
                    // Receive the updated Queue to all clients
                    // Checks if player is ready, if other players are ready, and if the game can start
                    if (formatName(username) == this.usrName)
                        this.btn_Ready.Content = isReady ? "Un-Ready" : "Ready";
                    if (!hasEnoughPlayers && isReady)
                        Message("Not enough players!");
                    else if (!hasEnoughPlayers && !isReady)
                        Message("Waiting for players...");
                    else
                    {
                        timerCounter = 5;
                        if (formatName(username) != this.usrName)
                        {
                            foreach (PlayerTemplate pt in PlayerGrid.Children.OfType<PlayerTemplate>())
                            {
                                if (pt.Name.Replace('_', ' ') == username)
                                {
                                    (pt.FindName("ReadyImage") as Image).Visibility = isReady ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
                                    (pt.FindName("NotReadyImage") as Image).Visibility = isReady ? System.Windows.Visibility.Hidden : System.Windows.Visibility.Visible;
                                    break;
                                }
                            }
                        }
                        if (gameStart)
                        {
                            timer.Start();
                            Message("All players ready! Game will start in 6...");
                        }
                        else
                        {
                            timer.Stop();
                            Message("Waiting for players...");
                        }
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
                this.Dispatcher.BeginInvoke(new UpdateQueueDelegate(UpdateQueue), new object[] { username, isReady, gameStart, hasEnoughPlayers });
        }
        // Update clients when a card has been discarded
        private delegate void UpdateDiscardedDelegate(string discard);
        public void UpdateDiscard(string _discard)
        {
            if (this.Dispatcher.Thread == System.Threading.Thread.CurrentThread)
            {
                try
                {
                    (btn_discardDeck.FindName("facediscardDeck") as Image).Source = new BitmapImage(new Uri(@"\Images\Cards\" + _discard + ".jpg", UriKind.RelativeOrAbsolute));
                    (btn_drawnCard.FindName("facedrawnCard") as Image).Source = null;
                    btn_drawnCard.Visibility = System.Windows.Visibility.Hidden;
                    (btn_discardDeck.FindName("facediscardDeck") as Image).Visibility = System.Windows.Visibility.Visible;
                    (btn_discardDeck.FindName("img_discardDeck") as Image).Visibility = System.Windows.Visibility.Hidden;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
                this.Dispatcher.BeginInvoke(new UpdateDiscardedDelegate(UpdateDiscard), new object[] { _discard });

        }
        // Update the contestents cards and yours for a realistic feel of sitting at a table
        private delegate void UpdateContestantCardDelegate(string userName, string newCard, string oldCard, bool fromDiscard, string btnName, string objectName);
        public void UpdateContestantCard(string userName, string newCard, string oldCard, bool fromDiscard, string btnName, string objectName)
        {
            if (this.Dispatcher.Thread == System.Threading.Thread.CurrentThread)
            {
                try
                {
                    // we swapped the card
                    if (userName == this.usrName)
                    {
                        if (fromDiscard)
                        {
                            (btn_discardDeck.FindName("facediscardDeck") as Image).Source = new BitmapImage(new Uri(@"\Images\Cards\" + oldCard + ".jpg", UriKind.RelativeOrAbsolute));
                            Button droppedToBtn = userCardGrid.FindName(btnName) as Button;
                            (droppedToBtn.FindName(objectName) as Image).Source = new BitmapImage(new Uri(@"\Images\Cards\" + newCard + ".jpg", UriKind.RelativeOrAbsolute));
                            (droppedToBtn.FindName(objectName) as Image).Visibility = System.Windows.Visibility.Visible;
                            UpdatePoints();
                        }
                        else
                        {
                            (btn_discardDeck.FindName("facediscardDeck") as Image).Source = new BitmapImage(new Uri(@"\Images\Cards\" + oldCard + ".jpg", UriKind.RelativeOrAbsolute));
                            (btn_discardDeck.FindName("facediscardDeck") as Image).Visibility = System.Windows.Visibility.Visible;
                            Button droppedToBtn = userCardGrid.FindName(btnName) as Button;
                            (droppedToBtn.FindName(objectName) as Image).Source = new BitmapImage(new Uri(@"\Images\Cards\" + newCard + ".jpg", UriKind.RelativeOrAbsolute));
                            (droppedToBtn.FindName(objectName) as Image).Visibility = System.Windows.Visibility.Visible;
                            btn_drawnCard.Visibility = System.Windows.Visibility.Hidden;
                            UpdatePoints();
                        }
                    }
                    else // someone else did, update the players panel
                    {
                        if (fromDiscard)
                        {
                            (btn_discardDeck.FindName("facediscardDeck") as Image).Source = new BitmapImage(new Uri(@"\Images\Cards\" + oldCard + ".jpg", UriKind.RelativeOrAbsolute));

                            // swap contestant images
                            foreach (PlayerTemplate pt in PlayerGrid.Children.OfType<PlayerTemplate>())
                            {
                                if (pt.Name.Replace('_', ' ') == userName)
                                {
                                    (pt.FindName(objectName) as Image).Source = new BitmapImage(new Uri(@"\Images\Cards\" + newCard + ".jpg", UriKind.RelativeOrAbsolute));
                                    (pt.FindName(objectName) as Image).Visibility = System.Windows.Visibility.Visible;
                                    break; // break out as soon as we find what we were looking for
                                }
                            }
                        }
                        else
                        {
                            (btn_discardDeck.FindName("facediscardDeck") as Image).Source = new BitmapImage(new Uri(@"\Images\Cards\" + oldCard + ".jpg", UriKind.RelativeOrAbsolute));
                            (btn_discardDeck.FindName("facediscardDeck") as Image).Visibility = System.Windows.Visibility.Visible;
                            btn_drawnCard.Visibility = System.Windows.Visibility.Hidden;
                            foreach (PlayerTemplate pt in PlayerGrid.Children.OfType<PlayerTemplate>())
                            {
                                if (pt.Name.Replace('_', ' ') == userName)
                                {
                                    (pt.FindName(objectName) as Image).Source = new BitmapImage(new Uri(@"\Images\Cards\" + newCard + ".jpg", UriKind.RelativeOrAbsolute));
                                    (pt.FindName(objectName) as Image).Visibility = System.Windows.Visibility.Visible;
                                    break;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
                this.Dispatcher.BeginInvoke(new UpdateContestantCardDelegate(UpdateContestantCard), new object[] { userName, newCard, oldCard, fromDiscard, btnName, objectName });
        }
        // Update the player grids for new player joining
        private delegate void NewPlayerDelagate(string[] name);
        public void NewPlayerJoin(string[] _names)
        {
            if (this.Dispatcher.Thread == System.Threading.Thread.CurrentThread)
            {
                try
                {
                    timer.Stop();
                    timerCounter = 5;
                    this.PlayerGrid.Children.Clear();
                    for (int i = 0; i < _names.Length; ++i)
                    {
                        if (formatName(_names[i]).Equals(usrName))
                            continue;
                        PlayerTemplate pt = new PlayerTemplate(formatName(_names[i]).Replace(' ', '_'), 0);
                        this.PlayerGrid.Children.Add(pt);
                    }
                    this.GameGrid.IsEnabled = true;
                    this.GameGrid.Opacity = 1.0;
                    Message("Waiting for players...");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
                this.Dispatcher.BeginInvoke(new NewPlayerDelagate(NewPlayerJoin), new object[] { _names });
        }
        // Delegate and function for when a player leaves, update the player grid to show that the player is no longer playing the game
        private delegate void PlayerDisconnectedDelegate(string[] name);
        public void PlayerDisconnected(string[] _names)
        {
            if (this.Dispatcher.Thread == System.Threading.Thread.CurrentThread)
            {
                try
                {
                    this.PlayerGrid.Children.Clear();
                    for (int i = 0; i < _names.Length; ++i)
                    {
                        if (formatName(_names[i]).Equals(usrName))
                            continue;
                        PlayerTemplate pt = new PlayerTemplate(formatName(_names[i]), 0);
                        this.PlayerGrid.Children.Add(pt);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
                this.Dispatcher.BeginInvoke(new PlayerDisconnectedDelegate(PlayerDisconnected), new object[] { _names });
        }
        // Update the gamestate for when a players points changes, or queue changes
        private delegate void UpdateGameStateDelegate(Player[] _players);
        public void UpdateGameState(Player[] _players)
        {
            if (this.Dispatcher.Thread == System.Threading.Thread.CurrentThread)
            {
                try
                {
                    for (int i = 0; i < _players.Count(); ++i)
                    {
                        if (formatName(_players[i].Name).Equals(usrName))
                        {
                            this.lbl_userPoints.Content = "Points: " + gameSystem.GetPoints(usrName);
                            continue;
                        }
                        foreach (PlayerTemplate pt in PlayerGrid.Children.OfType<PlayerTemplate>())
                        {
                            if (pt.Name.Replace('_', ' ') == formatName(_players[i].Name))
                            {
                                (pt.FindName("PlayerName") as Label).Content = "Player: " + formatName(_players[i].Name);
                                (pt.FindName("PlayerPoints") as Label).Content = "Score: " + _players[i].Points;

                                (pt.FindName("ReadyImage") as Image).Visibility = _players[i].isReady ?
                                    System.Windows.Visibility.Visible :
                                    System.Windows.Visibility.Hidden;

                                (pt.FindName("NotReadyImage") as Image).Visibility = _players[i].isReady ?
                                    System.Windows.Visibility.Hidden :
                                    System.Windows.Visibility.Visible;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
                this.Dispatcher.BeginInvoke(new UpdateGameStateDelegate(UpdateGameState), new object[] { _players });
        }
        // The next turn is run, update all clients and restrict and enable functionaltiy
        private delegate void NextTurnDelegate(Player[] _players);
        public void NextTurn(Player[] _players)
        {
            if (this.Dispatcher.Thread == System.Threading.Thread.CurrentThread)
            {
                try
                {

                    Player currentPlayer = null;
                    _players.All(p =>
                    {
                        if (p.myTurn)
                        {
                            currentPlayer = p;
                            return true;
                        }

                        return true;
                    });

                    if (formatName(currentPlayer.Name) == this.usrName)
                    {
                        btn_blindDeck.IsEnabled = true;
                        btn_discardDeck.IsEnabled = true;
                        btn_drawnCard.IsEnabled = true;

                        //btn_blindDeck.PreviewMouseLeftButtonDown += btn_PreviewMouseLeftButtonDown;
                        btn_discardDeck.PreviewMouseLeftButtonDown += btn_PreviewMouseLeftButtonDown;
                        btn_drawnCard.PreviewMouseLeftButtonDown += btn_PreviewMouseLeftButtonDown;

                        this.UsersBorder.BorderBrush = Brushes.Green;
                        this.UsersBorder.BorderThickness = new System.Windows.Thickness(5);

                        foreach (PlayerTemplate pt in PlayerGrid.Children.OfType<PlayerTemplate>())
                        {
                            pt.PlayerBorder.BorderBrush = Brushes.Black;
                            pt.PlayerBorder.BorderThickness = new System.Windows.Thickness(2);
                        }

                        Message("It's your turn!");
                    }
                    else
                    {
                        //btn_blindDeck.PreviewMouseLeftButtonDown -= btn_PreviewMouseLeftButtonDown;
                        btn_discardDeck.PreviewMouseLeftButtonDown -= btn_PreviewMouseLeftButtonDown;
                        btn_drawnCard.PreviewMouseLeftButtonDown -= btn_PreviewMouseLeftButtonDown;

                        btn_blindDeck.IsEnabled = false;
                        btn_discardDeck.IsEnabled = false;
                        btn_drawnCard.IsEnabled = false;

                        this.UsersBorder.BorderBrush = Brushes.Black;
                        this.UsersBorder.BorderThickness = new System.Windows.Thickness(2);


                        foreach (PlayerTemplate pt in PlayerGrid.Children.OfType<PlayerTemplate>())
                        {
                            if (pt.Name.Replace('_', ' ') == formatName(currentPlayer.Name))
                            {
                                pt.PlayerBorder.BorderBrush = Brushes.Green;
                                pt.PlayerBorder.BorderThickness = new System.Windows.Thickness(5);

                                pt.BringIntoView();

                            }
                            else
                            {
                                pt.PlayerBorder.BorderBrush = Brushes.Black;
                                pt.PlayerBorder.BorderThickness = new System.Windows.Thickness(2);
                            }
                        }


                        Message(formatName(currentPlayer.Name) + "'s Turn!");
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
                this.Dispatcher.BeginInvoke(new NextTurnDelegate(NextTurn), new object[] { _players });
        }
        // Reset all clients back to pre-game
        private delegate void ResetClientDelegate();
        public void ResetClients()
        {
            if (this.Dispatcher.Thread == System.Threading.Thread.CurrentThread)
            {
                try
                {
                    Message("Waiting for players...");
                    foreach (PlayerTemplate pt in PlayerGrid.Children.OfType<PlayerTemplate>())
                    {
                        (pt.FindName("ReadyImage") as Image).Visibility = System.Windows.Visibility.Hidden;
                        (pt.FindName("NotReadyImage") as Image).Visibility = System.Windows.Visibility.Visible;
                    }
                    /// UPDATE THE PLAYERS ON THE SIDE GRID FLIP CARDS OVER
                    (btn_discardDeck.FindName("facediscardDeck") as Image).Visibility = System.Windows.Visibility.Hidden;
                    (btn_discardDeck.FindName("img_discardDeck") as Image).Visibility = System.Windows.Visibility.Visible;

                    (btn_card2.FindName("face1") as Image).Visibility = System.Windows.Visibility.Hidden;
                    (btn_card2.FindName("back1") as Image).Visibility = System.Windows.Visibility.Visible;

                    (btn_card2.FindName("face1") as Image).Visibility = System.Windows.Visibility.Hidden;
                    (btn_card2.FindName("back1") as Image).Visibility = System.Windows.Visibility.Visible;

                    (btn_card2.FindName("face1") as Image).Visibility = System.Windows.Visibility.Hidden;
                    (btn_card2.FindName("back1") as Image).Visibility = System.Windows.Visibility.Visible;

                    this.btn_card1.Visibility = System.Windows.Visibility.Hidden;
                    this.btn_card2.Visibility = System.Windows.Visibility.Hidden;
                    this.btn_card3.Visibility = System.Windows.Visibility.Hidden;

                    foreach (PlayerTemplate pt in PlayerGrid.Children.OfType<PlayerTemplate>())
                    {
                        (pt.FindName("btn_card1") as Button).Visibility = System.Windows.Visibility.Hidden;
                        (pt.FindName("btn_card2") as Button).Visibility = System.Windows.Visibility.Hidden;
                        (pt.FindName("btn_card3") as Button).Visibility = System.Windows.Visibility.Hidden;
                    }




                    this.btn_Ready.Visibility = System.Windows.Visibility.Visible;
                    this.GameGrid.IsEnabled = true;
                    this.GameGrid.Opacity = 1.0;

                    PreGame();

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
                this.Dispatcher.BeginInvoke(new ResetClientDelegate(ResetClients), new object[] { });
        }
        // If a player joins a game already in progress, send them to a freezed wait screen
        private delegate void SendWaitQueueFreezeDelegate();
        public void SendWaitQueueFreeze()
        {
            if (this.Dispatcher.Thread == System.Threading.Thread.CurrentThread)
            {
                try
                {
                    this.GameGrid.IsEnabled = false;
                    this.GameGrid.Opacity = .5;
                    Message("Game in progress..." + Environment.NewLine + "Please wait.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
                this.Dispatcher.BeginInvoke(new SendWaitQueueFreezeDelegate(SendWaitQueueFreeze), new object[] { });
        }
        #endregion
        // Remove the player from the game
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (gameSystem != null)
                gameSystem.Leave(usrName);
        }
        // Toggle the client's state to play the game
        private void btn_Ready_Click_1(object sender, RoutedEventArgs e)
        {
            if (btn_Ready.Content.ToString().Contains("Un"))
            {
                gameSystem.UpdateQueue(this.usrName, false);
            }
            else
            {
                gameSystem.UpdateQueue(this.usrName, true);
            }
        }
    }
}


