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

using System.ServiceModel;
using GolfLibrary;

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
        //private IPlayer player = null;

        private int numPlayers = 0;
        public MainWindow()
        {
            InitializeComponent();
            try
            {
<<<<<<< HEAD
                //Get Username from player
                //System.Windows.Forms.Form form = new System.Windows.Forms.Form();

                //form.Text = "Help Menu";
                //form.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;

                //form.Width = 200;
                //form.Height = 200;

                //System.Windows.Forms.Label lbl = new System.Windows.Forms.Label();
                //lbl.Text = "Username: ";
                //lbl.Height = 40;
                //lbl.Width = 200;

                //System.Windows.Forms.TextBox tb = new System.Windows.Forms.TextBox();
                //tb.Height = 40;
                //tb.Width = 200;

                //System.Windows.Forms.Button button = new System.Windows.Forms.Button();
                //button.Text = "Submit";
                //button.Click += button_Click;
                //button.Height = 40;
                //button.Width = 200;

                //form.Controls.Add(tb);
                //form.Controls.Add(lbl);
                //form.Controls.Add(button);
                //form.ShowDialog();


=======
                //btn_discardDeck.Visibility = System.Windows.Visibility.Collapsed;
>>>>>>> master
                // configure the ABCs of using the cardsLibrary as a service
                DuplexChannelFactory<IGameSystem> channel = new DuplexChannelFactory<IGameSystem>(this, "Game");
                 
                

                //Activate the shoe
                gameSystem = channel.CreateChannel();

<<<<<<< HEAD
                //DuplexChannelFactory<IPlayer> playerChannel = new DuplexChannelFactory<IPlayer>(this,
                // new NetTcpBinding(),
                // new EndpointAddress("net.tcp://localhost:9000/GolfLibrary/Shoe"));

                ////Activate the shoe
                //player = playerChannel.CreateChannel();
                //ChannelFactory<IPlayer> playerChannel = new ChannelFactory<IPlayer>(
                //new NetTcpBinding(),
                //new EndpointAddress("net.tcp://localhost:9000/GolfLibrary/Shoe"));
=======
>>>>>>> master

                gameSystem.Join("Randy");

                DrawThreeCards();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void DrawThreeCards()
        {
<<<<<<< HEAD

            string card1 = shoe.Draw();
            string card2 = shoe.Draw();
            string card3 = shoe.Draw();

            (btn_card1.FindName("face1") as Image).Source = new BitmapImage(new Uri(@"\Images\Cards\" + card1 + ".jpg", UriKind.RelativeOrAbsolute));
            (btn_card2.FindName("face2") as Image).Source = new BitmapImage(new Uri(@"\Images\Cards\" + card2 + ".jpg", UriKind.RelativeOrAbsolute));
            (btn_card3.FindName("face3") as Image).Source = new BitmapImage(new Uri(@"\Images\Cards\" + card3 + ".jpg", UriKind.RelativeOrAbsolute));
=======
            List<string> startingCards;
            try
            {
                
                startingCards = gameSystem.DrawThreeCards();
>>>>>>> master

                for (int i = 1; i <= 3; i++)
                {
                    (btn_card1.FindName("face" + i) as Image).Source = new BitmapImage(new Uri(@"\Images\Cards\" + startingCards[i-1] + ".jpg", UriKind.RelativeOrAbsolute));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        protected void btn_clickTest(object sender, EventArgs e)
        {

            SwapImage(false, btn_card1, "1", @"\Images\Cards\SevenDiamonds.jpg");

            SwapContestantImage("TestPlayer_1", "3", @"\Images\Cards\JackHearts.jpg");

            card = gameSystem.Draw();
            int deck_size = gameSystem.NumCards;
            MessageBox.Show(card);
            MessageBox.Show("" + deck_size);

        }

        private void testTemplate_Click_1(object sender, RoutedEventArgs e)
        {
            numPlayers++;
            string PlayerID = "TestPlayer_" + numPlayers;
            PlayerTemplate pt = new PlayerTemplate(PlayerID, numPlayers * 7);
            this.PlayerGrid.Children.Add(pt);
            SwapImage(true, btn_card1, "1", @"\Images\Cards\SevenDiamonds.jpg");
        }

        // Swapping and Showing Cards
        public void SwapImage(bool _showface, FrameworkElement btn, string _identity, string newcard)
        {
            (btn.FindName("face" + _identity) as Image).Source = new BitmapImage(new Uri(newcard, UriKind.RelativeOrAbsolute));
            if (_showface) // Showing Face Card
            {
                (btn.FindName("back" + _identity) as Image).Visibility = System.Windows.Visibility.Hidden;
                (btn.FindName("face" + _identity) as Image).Visibility = System.Windows.Visibility.Visible;
            }
            else // Showing Back Card
            {
                (btn.FindName("back" + _identity) as Image).Visibility = System.Windows.Visibility.Visible;
                (btn.FindName("face" + _identity) as Image).Visibility = System.Windows.Visibility.Hidden;
            }
        }

        public void SwapRawImage(FrameworkElement btn, string id, string card)
        {
            (btn.FindName(id) as Image).Source = new BitmapImage(new Uri(card + ".jpg", UriKind.RelativeOrAbsolute));
        }

        public void SwapContestantImage(string _playerID, string _identifier, string newcard)
        {
            try
            {
                foreach (PlayerTemplate pt in PlayerGrid.Children.OfType<PlayerTemplate>())
                {
                    if (pt.Name == _playerID)
                    {
                        (pt.FindName("face" + _identifier) as Image).Source = new BitmapImage(new Uri(newcard, UriKind.RelativeOrAbsolute));
                        (pt.FindName("back" + _identifier) as Image).Visibility = System.Windows.Visibility.Hidden;
                        (pt.FindName("face" + _identifier) as Image).Visibility = System.Windows.Visibility.Visible;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error swapping player " + _playerID + "'s card\n" + ex.InnerException.ToString());
            }
        }

        //Draw a card from the deck
        private void btn_blindDeck_Click(object sender, RoutedEventArgs e)
        {
            if (btn_drawnCard.Visibility == Visibility.Hidden)
            {
                //Draw Card
<<<<<<< HEAD
                card = shoe.Draw();
                if (card == "shoe empty")
                {
                    btn_blindDeck.Visibility = Visibility.Hidden;
                    btn_blindDeck_dummy.Visibility = Visibility.Hidden;
                    //This would be the end of the game
                }
                else
                {
                    //Show the card drawn in the center
                    btn_drawnCard.Visibility = Visibility.Visible;
                    (btn_drawnCard.FindName("facedrawnCard") as Image).Source = new BitmapImage(new Uri(@"\Images\Cards\" + card + ".jpg", UriKind.RelativeOrAbsolute));

                    btn_discardDeck.PreviewMouseLeftButtonDown -= btn_PreviewMouseLeftButtonDown;
                }

                // Collapsing buttons doesn't work 
                // Hiding buttons doens't work,
                // we may have unregister the event itself and register it again after they discard/keep the drawn cardrf
=======
                card = gameSystem.Draw();
>>>>>>> master
            }
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
                Button b = e.Data.GetData(typeof(Button)) as Button;
                (b.FindName("facediscardDeck") as Image).Source = (btn_drawnCard.FindName("facedrawnCard") as Image).Source;

                (btn_drawnCard.FindName("facedrawnCard") as Image).Source = null;
                btn_drawnCard.Visibility = System.Windows.Visibility.Hidden;

                (b.FindName("facediscardDeck") as Image).Visibility = System.Windows.Visibility.Visible;
                (b.FindName("img_discardDeck") as Image).Visibility = System.Windows.Visibility.Hidden;

                btn_discardDeck.PreviewMouseLeftButtonDown += btn_PreviewMouseLeftButtonDown;

                gameSystem.DiscardedCard = (b.FindName("facediscardDeck") as Image).Source.ToString().Split('.')[0];
                
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
            string frontName = imgFront.Name;
            string backName = imgBack.Name;

            //Set the bottom card to the image of the drawn card that was dragged down
            if (name != "")
            {
                //Draged from the discard pile
                //Set the user card image to the discarded card image
                ImageSource tempSource = (draggedToButton.FindName(frontName) as Image).Source;
                (draggedToButton.FindName(frontName) as Image).Source = (draggedFromButton.FindName(draggedFrom_imgFront.Name) as Image).Source;

                //Set the discarded card image to the replaced user card image
                (draggedFromButton.FindName(draggedFrom_imgFront.Name) as Image).Source = tempSource;

                //Turn over the players card
                (draggedToButton.FindName(frontName) as Image).Visibility = System.Windows.Visibility.Visible;
                (draggedToButton.FindName(backName) as Image).Visibility = System.Windows.Visibility.Hidden;
            }
            else
            {
                //Draged from the drawn card
                //Set the card that is replaced to the top of the discard pile
                ImageSource tempSource = (draggedToButton.FindName(frontName) as Image).Source;

                //Set the player card to the drawn card
                (draggedToButton.FindName(frontName) as Image).Source = (draggedFromButton.FindName("facedrawnCard") as Image).Source;

                //Set the discard pile top card to the player card that was replaced
                (btn_discardDeck.FindName("facediscardDeck") as Image).Source = tempSource;

                //Make the face card shown and the back of the card hidden
                (btn_discardDeck.FindName("img_discardDeck") as Image).Visibility = Visibility.Hidden;
                (btn_discardDeck.FindName("facediscardDeck") as Image).Visibility = Visibility.Visible;

                //Hide the drawn card
                draggedFromButton.Visibility = System.Windows.Visibility.Hidden;

                //Turn the newly added user card over
                (draggedToButton.FindName(frontName) as Image).Visibility = System.Windows.Visibility.Visible;
                (draggedToButton.FindName(backName) as Image).Visibility = System.Windows.Visibility.Hidden;

                btn_discardDeck.PreviewMouseLeftButtonDown += btn_PreviewMouseLeftButtonDown;
            }

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

<<<<<<< HEAD


            //form.Close();
            //HelpControl helpControl = new HelpControl();


        }
=======
        }

        private delegate void GuiUpdateDelegate( string drawn );
        public void UpdateDrawn(string _drawn)
        {
            if (this.Dispatcher.Thread == System.Threading.Thread.CurrentThread)
            {
                try
                {
                    btn_drawnCard.Visibility = Visibility.Visible;
                    (btn_drawnCard.FindName("facedrawnCard") as Image).Source = new BitmapImage(new Uri(@"\Images\Cards\" + _drawn + ".jpg", UriKind.RelativeOrAbsolute));

                    btn_discardDeck.PreviewMouseLeftButtonDown -= btn_PreviewMouseLeftButtonDown;                  
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
                this.Dispatcher.BeginInvoke(new GuiUpdateDelegate(UpdateDrawn), new object[] { _drawn });
        }
        
        public void UpdateGui(CallBackInfo info)
        {
            throw new NotImplementedException();
        }
>>>>>>> master
    }
}


//public 









//var card1Img = btn_card1.Template.FindName("img_card1", ControlTemplate);


//        private void updateCardCounts()
//        {
//            txtHandCount.Text = lstCards.Items.Count.ToString();
//            txtShoeCount.Text = shoe.NumCards.ToString();
//        }

//        private void btnClose_Click(object sender, RoutedEventArgs e)
//        {
//            this.Close();
//        }

//        private void btnDraw_Click(object sender, RoutedEventArgs e)
//        {
//            try
//            {
//                // get a card from the shoe and add it to the hand
//                string card = shoe.Draw();
//                lstCards.Items.Add(card);
//                updateCardCounts();
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show(ex.Message);
//            }
//        }

//        private void btnShuffle_Click(object sender, RoutedEventArgs e)
//        {
//            try
//            {
//                // shuffle the shoe
//                shoe.Shuffle();

//                // reset listbox and card counts
//                lstCards.Items.Clear();
//                updateCardCounts();
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show(ex.Message);
//            }
//        }

//        private void sliderDecks_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
//        {
//            try
//            {
//                if (shoe != null)
//                {
//                    // set the number of decks
//                    shoe.NumDecks = (int)(sliderDecks.Value);
//                    if (shoe.NumDecks == 1)
//                        txtDeckCount.Text = "1 Deck";
//                    else
//                        txtDeckCount.Text = shoe.NumDecks + " Decks";

//                    // reset the listbox and card counts
//                    lstCards.Items.Clear();
//                    updateCardCounts();
//                }
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show("Slider error" + ex.Message);
//            }
//        }

//        private void btn_drawnCard_Click(object sender, RoutedEventArgs e)
//        {

//        }
