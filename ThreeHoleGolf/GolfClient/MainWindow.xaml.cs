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
    public partial class MainWindow : Window
    {

        private IShoe shoe = null;


        private int numPlayers = 0;
        public MainWindow()
        {
            InitializeComponent();
            try
            {

                // configure the ABCs of using the cardsLibrary as a service
                ChannelFactory<IShoe> channel = new ChannelFactory<IShoe>(
                 new NetTcpBinding(),
                 new EndpointAddress("net.tcp://localhost:9000/GolfLibrary/Shoe"));

                //Activate the shoe
                shoe = channel.CreateChannel();
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

            string card = shoe.Draw();
            int deck_size = shoe.NumCards;
            MessageBox.Show(card);
            MessageBox.Show(""+deck_size);

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

        public void SwapContestantImage( string _playerID, string _identifier, string newcard)
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
            //Draw Card
            string card = shoe.Draw();

            //Show the card drawn in the center
            btn_drawnCard.Visibility = Visibility.Visible;

            //if (card == "JokerBlack")
            //{
            //    Console.WriteLine("Found the jokerblack");
            //    Image i = new Image();
            //    i.Source = new BitmapImage(new Uri(@"\Images\Cards\" + card + ".jpg", UriKind.RelativeOrAbsolute));
            //}
            


            (btn_drawnCard.FindName("facedrawnCard") as Image).Source = new BitmapImage(new Uri(@"\Images\Cards\" + card + ".jpg", UriKind.RelativeOrAbsolute));

            // Collapsing buttons doesn't work 
            // Hiding buttons doens't work,
            // we may have unregister the event itself and register it again after they discard/keep the drawn card
        }

        






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
    }
}
