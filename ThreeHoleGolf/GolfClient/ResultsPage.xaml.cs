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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace GolfClient
{
    /// <summary>
    /// Interaction logic for ResultsPage.xaml
    /// </summary>
    public partial class ResultsPage : Window
    {
        private DispatcherTimer delay;
        private int timeCount = 1;
        private List<string> playerNames = new List<string>();
        private List<int> playerPoints = new List<int>();

        public ResultsPage()
        {
            InitializeComponent();

        }

        public void LoadResults(string[] names, int[] points)
        {
            this.Show();
            playerNames = names.ToList();
            playerPoints = points.ToList();
            //"Winner" will be the winner name that will be inputed
            delay = new DispatcherTimer();
            delay.Tick += new EventHandler(timer_switch);
            delay.Interval = new TimeSpan(0, 0, 1);
            delay.Start();
        }

        private void timer_switch(object sender, EventArgs e)
        {
            try
            {
                timeCount -= 1;
                if (timeCount < 0)
                {
                    lv_names.Visibility = System.Windows.Visibility.Visible;
                    lv_point.Visibility = System.Windows.Visibility.Visible;
                    this.Width = 800;
                    this.Height = 600;

                    ListViewItem nameTitle = new ListViewItem();
                    nameTitle.Content = "Names: \n";
                    nameTitle.FontSize = 18;
                    nameTitle.FontFamily = new System.Windows.Media.FontFamily("Cooper Black");
                    nameTitle.IsEnabled = false;

                    ListViewItem pointTitle = new ListViewItem();
                    pointTitle.Content = "Points: \n";
                    pointTitle.FontSize = 18;
                    pointTitle.FontFamily = new System.Windows.Media.FontFamily("Cooper Black");
                    pointTitle.IsEnabled = false;

                    lv_names.Items.Add(nameTitle);
                    lv_point.Items.Add(pointTitle);

                    List<KeyValuePair<string, int>> players = new List<KeyValuePair<string, int>>();
                    for (int i = 0; i < playerNames.Count; ++i)
                    {
                        players.Add(new KeyValuePair<string, int>(playerNames[i], playerPoints[i]));
                    }

                    players = players.OrderBy(x => x.Value).ToList();

                    for (int i = 0; i < players.Count; ++i)
                    {
                        //Winner get a special detail
                        ListViewItem name = new ListViewItem();
                        ListViewItem point = new ListViewItem();
                        if (i == 0)
                        {
                            name.Content = (i + 1) + ".      " + players[i].Key + "\n";
                            name.FontSize = 20;
                            name.Foreground = new SolidColorBrush(Colors.MediumSpringGreen);
                            name.FontFamily = new System.Windows.Media.FontFamily("Cooper Black");
                            name.IsEnabled = false;

                            point.Content = players[i].Value.ToString() + "\n";
                            point.FontSize = 20;
                            point.Foreground = new SolidColorBrush(Colors.MediumSpringGreen);
                            point.FontFamily = new System.Windows.Media.FontFamily("Cooper Black");
                            point.IsEnabled = false;
                        }
                        else
                        {
                            name.Content = (i + 1) + ".\t" + players[i].Key + "\n";
                            name.FontSize = 14;
                            name.FontFamily = new System.Windows.Media.FontFamily("Cooper Black");
                            name.IsEnabled = false;

                            point.Content = players[i].Value.ToString() + "\n";
                            point.FontSize = 14;
                            point.FontFamily = new System.Windows.Media.FontFamily("Cooper Black");
                            point.IsEnabled = false;
                        }

                        lv_names.Items.Add(name);
                        lv_point.Items.Add(point);
                    }

                    img_results.Visibility = Visibility.Visible;
                    img_start.Visibility = Visibility.Hidden;

                    delay.Stop();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
