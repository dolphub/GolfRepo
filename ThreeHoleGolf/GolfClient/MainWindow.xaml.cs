//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Data;
//using System.Windows.Documents;
//using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using System.Windows.Navigation;
//using System.Windows.Shapes;

//using System.ServiceModel;
//using GolfLibrary;

//namespace GolfClient
//{
//    /// <summary>
//    /// Interaction logic for MainWindow.xaml
//    /// </summary>
//    public partial class MainWindow : Window
//    {
//        private IShoe shoe = null;

//        public MainWindow()
//        {
//            InitializeComponent();

//            try
//            {

//                // configure the ABCs of using the cardsLibrary as a service
//                ChannelFactory<IShoe> channel = new ChannelFactory<IShoe>(
//                 new NetTcpBinding(),
//                 new EndpointAddress("net.tcp://localhost:9000/GolfLibrary/Shoe"));

//                shoe = channel.CreateChannel();

//                // set upslider control(# of decks)
//                sliderDecks.Minimum = 1;
//                sliderDecks.Maximum = 10;
//                sliderDecks.Value = shoe.NumDecks;

//                // initialize the card counts
//                updateCardCounts();

//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show(ex.Message);
//            }
//        }

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
//    }
//}
