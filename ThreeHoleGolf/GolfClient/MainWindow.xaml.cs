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
            string card = shoe.Draw();
            int deck_size = shoe.NumCards;
            MessageBox.Show(card);
            MessageBox.Show(""+deck_size);
        }

        private void testTemplate_Click_1(object sender, RoutedEventArgs e)
        {
            numPlayers++;
            PlayerTemplate pt = new PlayerTemplate("TestPlayer " + numPlayers, numPlayers * 7);
            this.PlayerGrid.Children.Add(pt);
        }

        //Draw a card from the deck
        private void btn_blindDeck_Click(object sender, RoutedEventArgs e)
        {
            //Draw Card
            shoe.Draw();

            //Show the card drawn in the center
            btn_drawnCard.Visibility = Visibility.Visible;

            //Change the player turns buttons to discard or keep
            btn_discard.Visibility = Visibility.Visible;
            btn_keep.Visibility = Visibility.Visible;

            btn_blindPile.Visibility = Visibility.Collapsed;
            btn_discardPile.Visibility = Visibility.Collapsed;
        }

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
