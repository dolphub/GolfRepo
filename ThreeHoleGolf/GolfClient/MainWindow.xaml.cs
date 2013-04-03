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
        private IGameSystem gameSystem = null;
        private string card = null;
        private string usrName = "";
        private DispatcherTimer timer;
        private int timerCounter = 5;


        public MainWindow()
        {

            try
            {
                // configure the ABCs of using the cardsLibrary as a service
                DuplexChannelFactory<IGameSystem> channel = new DuplexChannelFactory<IGameSystem>(this, "Game");
                //Activate the shoe
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

                // Set variables and objects
                timer = new DispatcherTimer();
                timer.Tick += new EventHandler(timer_Tick);
                timer.Interval = new TimeSpan(0, 0, 1);

                this.GameTable.IsEnabled = false;
                this.userCardGrid.IsEnabled = false;
                this.userCardGrid.Opacity = .5;
                this.GameTable.Opacity = .5;
                lbl_userName.Content += this.formatName(login.tb_username.Text);
                usrName = formatName(login.tb_username.Text);
                this.Title = usrName;
            }
            catch (Exception ex)
            {
                this.Close();
                MessageBox.Show(ex.Message);
            }

        }

        private void StartGame()
        {
            DrawThreeCards();
            this.GameTable.IsEnabled = true;
            this.GameTable.Opacity = 1.0;
            this.userCardGrid.IsEnabled = true;
            this.userCardGrid.Opacity = 1.0;
            this.btn_blindDeck.IsEnabled = false;

            foreach (PlayerTemplate pt in PlayerGrid.Children.OfType<PlayerTemplate>())
                (pt.FindName("ReadyImage") as Image).Visibility = System.Windows.Visibility.Visible;

            btn_blindDeck_dummy.PreviewMouseLeftButtonDown -= btn_PreviewMouseLeftButtonDown;
            btn_discardDeck.PreviewMouseLeftButtonDown -= btn_PreviewMouseLeftButtonDown;
            btn_drawnCard.PreviewMouseLeftButtonDown -= btn_PreviewMouseLeftButtonDown;

            gameSystem.StartGame();
            Message("Game has started!");

        }

        private void timer_Tick(object sender, EventArgs e)
        {

            Message("All players ready! Game will start in " + timerCounter + "...");
            timerCounter -= 1;
            if (timerCounter < 0)
            {
                timer.Stop();
                StartGame();
            }

        }

        private void Message(string msg)
        {
            this.GameMessageLbl.Content = msg;
        }


        private void DrawThreeCards()
        {
            List<string> startingCards;
            try
            {
                startingCards = gameSystem.DrawThreeCards();
                for (int i = 1; i <= 3; i++)
                {
                    (btn_card1.FindName("face" + i) as Image).Source = new BitmapImage(new Uri(@"\Images\Cards\" + startingCards[i - 1] + ".jpg", UriKind.RelativeOrAbsolute));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // Swapping and Showing Cards
        //public void SwapImage(bool _showface, FrameworkElement btn, string _identity, string newcard)
        //{
        //    (btn.FindName("face" + _identity) as Image).Source = new BitmapImage(new Uri(newcard, UriKind.RelativeOrAbsolute));
        //    if (_showface) // Showing Face Card
        //    {
        //        (btn.FindName("back" + _identity) as Image).Visibility = System.Windows.Visibility.Hidden;
        //        (btn.FindName("face" + _identity) as Image).Visibility = System.Windows.Visibility.Visible;
        //    }
        //    else // Showing Back Card
        //    {
        //        (btn.FindName("back" + _identity) as Image).Visibility = System.Windows.Visibility.Visible;
        //        (btn.FindName("face" + _identity) as Image).Visibility = System.Windows.Visibility.Hidden;
        //    }
        //}

        //public void SwapRawImage(FrameworkElement btn, string id, string card)
        //{
        //    (btn.FindName(id) as Image).Source = new BitmapImage(new Uri(card + ".jpg", UriKind.RelativeOrAbsolute));
        //}

        //public void SwapContestantImage(string _playerID, string _identifier, string newcard)
        //{
        //    try
        //    {

        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Error swapping player " + _playerID + "'s card\n" + ex.InnerException.ToString());
        //    }
        //}

        //Draw a card from the deck
        private void btn_blindDeck_Click(object sender, RoutedEventArgs e)
        {
            if (btn_drawnCard.Visibility == Visibility.Hidden)
                //Draw Card
                card = gameSystem.Draw();
        }


        private void btn_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Button b = sender as Button;
            DataObject dataObject = new DataObject(b);
            DragDrop.DoDragDrop(b, dataObject, DragDropEffects.Move);
        }

        private void btn_discardDeck_Drop(object sender, DragEventArgs e)
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
                string[] tempOldCard = (draggedFromButton.FindName(objectName) as Image).Source.ToString().Split('/');
                oldSwapImg = tempOldCard[tempOldCard.Length - 1].Split('.')[0];

                string[] tempNewCard = ((btn_discardDeck.FindName("facediscardDeck") as Image).Source.ToString().Split('.')[0]).Split('/');
                string newSwapImg = tempNewCard[tempNewCard.Length - 1];

                gameSystem.ContestentSwap(this.usrName, newSwapImg, oldSwapImg, true, buttonName, objectName);
            }
            else
            {
                string[] tempOldCard = (draggedToButton.FindName(objectName) as Image).Source.ToString().Split('/');
                oldSwapImg = tempOldCard[tempOldCard.Length - 1].Split('.')[0];

                string[] tempNewCard = ((btn_drawnCard.FindName("facedrawnCard") as Image).Source.ToString().Split('.')[0]).Split('/');
                string newSwapImg = tempNewCard[tempNewCard.Length - 1];

                gameSystem.ContestentSwap(this.usrName, newSwapImg, oldSwapImg, false, buttonName, objectName);
            }

            gameSystem.UpdateTurn();

        }

        // format a name for first capital, rest lower
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
                "A pair of equal cards in the same column scores zero points for the column (even if the equal cards are twos).\n" +
                "The player who has the lowest cumulative score after nine deals wins.";

            form.Controls.Add(rtb);
            form.ShowDialog();

        }

        #region CallBack Delegates


        /// <summary>
        /// Delagate and function for when a player draws a new card
        /// </summary>
        /// <param name="drawn"></param>
        private delegate void UpdateDrawnDelegate(string drawn);
        public void UpdateDrawn(string _drawn)
        {
            if (this.Dispatcher.Thread == System.Threading.Thread.CurrentThread)
            {
                try
                {
                    btn_drawnCard.Visibility = Visibility.Visible;
                    (btn_drawnCard.FindName("facedrawnCard") as Image).Source = new BitmapImage(new Uri(@"\Images\Cards\" + _drawn + ".jpg", UriKind.RelativeOrAbsolute));

                    //btn_discardDeck.PreviewMouseLeftButtonDown -= btn_PreviewMouseLeftButtonDown;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
                this.Dispatcher.BeginInvoke(new UpdateDrawnDelegate(UpdateDrawn), new object[] { _drawn });
        }

        private delegate void UpdateQueueDelegate(string username, bool isReady, bool gameStart, bool hasEnoughPlayers);
        public void UpdateQueue(string username, bool isReady, bool gameStart, bool hasEnoughPlayers)
        {
            if (this.Dispatcher.Thread == System.Threading.Thread.CurrentThread)
            {
                try
                {
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
                    //btn_discardDeck.PreviewMouseLeftButtonDown += btn_PreviewMouseLeftButtonDown;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
                this.Dispatcher.BeginInvoke(new UpdateDiscardedDelegate(UpdateDiscard), new object[] { _discard });

        }

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
                            //btn_discardDeck.PreviewMouseLeftButtonDown += btn_PreviewMouseLeftButtonDown;
                        }
                        else
                        {

                            (btn_discardDeck.FindName("facediscardDeck") as Image).Source = new BitmapImage(new Uri(@"\Images\Cards\" + oldCard + ".jpg", UriKind.RelativeOrAbsolute));
                            (btn_discardDeck.FindName("facediscardDeck") as Image).Visibility = System.Windows.Visibility.Visible;

                            Button droppedToBtn = userCardGrid.FindName(btnName) as Button;
                            (droppedToBtn.FindName(objectName) as Image).Source = new BitmapImage(new Uri(@"\Images\Cards\" + newCard + ".jpg", UriKind.RelativeOrAbsolute));
                            (droppedToBtn.FindName(objectName) as Image).Visibility = System.Windows.Visibility.Visible;

                            //btn_discardDeck.PreviewMouseLeftButtonDown += btn_PreviewMouseLeftButtonDown;
                            //btn_blindDeck_dummy.PreviewMouseLeftButtonDown += btn_PreviewMouseLeftButtonDown;
                            btn_drawnCard.Visibility = System.Windows.Visibility.Hidden;
                        }
                    }
                    else // someone else did, update the players panel
                    {
                        if (fromDiscard)
                        {
                            (btn_discardDeck.FindName("facediscardDeck") as Image).Source = new BitmapImage(new Uri(@"\Images\Cards\" + oldCard + ".jpg", UriKind.RelativeOrAbsolute));

                            // swap contestent images
                            foreach (PlayerTemplate pt in PlayerGrid.Children.OfType<PlayerTemplate>())
                            {
                                if (pt.Name.Replace('_', ' ') == userName)
                                {
                                    (pt.FindName(objectName) as Image).Source = new BitmapImage(new Uri(@"\Images\Cards\" + newCard + ".jpg", UriKind.RelativeOrAbsolute));
                                    (pt.FindName(objectName) as Image).Visibility = System.Windows.Visibility.Visible;
                                    break;
                                }
                            }
                            //btn_discardDeck.PreviewMouseLeftButtonDown += btn_PreviewMouseLeftButtonDown;

                        }
                        else
                        {
                            (btn_discardDeck.FindName("facediscardDeck") as Image).Source = new BitmapImage(new Uri(@"\Images\Cards\" + oldCard + ".jpg", UriKind.RelativeOrAbsolute));
                            (btn_discardDeck.FindName("facediscardDeck") as Image).Visibility = System.Windows.Visibility.Visible;
                            //btn_discardDeck.PreviewMouseLeftButtonDown += btn_PreviewMouseLeftButtonDown;
                            //btn_blindDeck_dummy.PreviewMouseLeftButtonDown += btn_PreviewMouseLeftButtonDown;
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
                    gameSystem.GameState();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
                this.Dispatcher.BeginInvoke(new UpdateContestantCardDelegate(UpdateContestantCard), new object[] { userName, newCard, oldCard, fromDiscard, btnName, objectName });
        }

        private delegate void NewPlayerDelagate(string[] name);
        public void NewPlayerJoin(string[] _names)
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
                        PlayerTemplate pt = new PlayerTemplate(formatName(_names[i]).Replace(' ', '_'), 0);
                        this.PlayerGrid.Children.Add(pt);
                    }
                    gameSystem.GameState();
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

        /// <summary>
        /// Delegate and function for when a player leaves, update the player grid
        /// to show that the player is no longer playing the game
        /// </summary>
        /// <param name="name"></param>
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

        private delegate void NextTurnDelegate(Player[] _players);
        public void NextTurn(Player[] _players)
        {
            if (this.Dispatcher.Thread == System.Threading.Thread.CurrentThread)
            {
                try
                {
                    string currentPlayer = null;
                    _players.All(p =>
                    {
                        if (p.myTurn)
                            currentPlayer = p.Name;
                        return true;
                    });
                    for (int i = 0; i < _players.Count(); ++i)
                    {
                        if (currentPlayer != null)
                        {
                            if (_players[i].myTurn)
                            {
                                btn_blindDeck_dummy.PreviewMouseLeftButtonDown += btn_PreviewMouseLeftButtonDown;
                                btn_discardDeck.PreviewMouseLeftButtonDown += btn_PreviewMouseLeftButtonDown;
                                btn_drawnCard.PreviewMouseLeftButtonDown += btn_PreviewMouseLeftButtonDown;
                                btn_blindDeck.IsEnabled = true;
                                Message("It's your turn!");
                            }
                            else
                            {
                                btn_blindDeck_dummy.PreviewMouseLeftButtonDown -= btn_PreviewMouseLeftButtonDown;
                                btn_discardDeck.PreviewMouseLeftButtonDown -= btn_PreviewMouseLeftButtonDown;
                                btn_drawnCard.PreviewMouseLeftButtonDown -= btn_PreviewMouseLeftButtonDown;
                                btn_blindDeck.IsEnabled = false;
                                Message(formatName(currentPlayer.ToString()) + "'s Turn!");
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
                this.Dispatcher.BeginInvoke(new NextTurnDelegate(NextTurn), new object[] { _players });
        }

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
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
                this.Dispatcher.BeginInvoke(new ResetClientDelegate(ResetClients), new object[] { });
        }



        public void UpdateGui(CallBackInfo info)
        {
            throw new NotImplementedException();
        }

        #endregion

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (gameSystem != null)
                gameSystem.Leave(usrName);
        }

        private void btn_Ready_Click_1(object sender, RoutedEventArgs e)
        {
            if (btn_Ready.Content.ToString().Contains("Un"))
            {
                btn_Ready.Content = "Ready";
                gameSystem.UpdateQueue(this.usrName, false);
            }
            else
            {
                btn_Ready.Content = "Un-Ready";
                gameSystem.UpdateQueue(this.usrName, true);
            }
        }




    }
}


