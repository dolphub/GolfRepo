using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using GoFishLibrary;
using System.ServiceModel;


namespace GoFishClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IMessageBoard msgBrd = null;
        private string prefix = "";

        public MainWindow()
        {
            InitializeComponent();

            try
            {

                ChannelFactory<IMessageBoard> channel = new ChannelFactory<IMessageBoard>(
                    new NetTcpBinding(),
                    new EndpointAddress("net.tcp://localhost:12000/MessageBoardLibrary/MessageBoard"));

                msgBrd = channel.CreateChannel();
                //msgBrd = new MessageBoard();
                listMessages.ItemsSource = msgBrd.getAllMessages();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void buttonSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (textPost.Text != "")
            {
                try
                {
                    msgBrd.postMessage(prefix + textPost.Text);
                    textPost.Clear();
                    listMessages.ItemsSource = msgBrd.getAllMessages();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void buttonSet_Click(object sender, RoutedEventArgs e)
        {
            if (textAlias.Text != "")
            {
                try
                {
                    prefix = "[" + textAlias.Text + "] ";
                    buttonSet.IsEnabled = textAlias.IsEnabled = false;
                    buttonSubmit.IsEnabled = textPost.IsEnabled = listMessages.IsEnabled = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void buttonRefresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                listMessages.ItemsSource = msgBrd.getAllMessages();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

    }
}
