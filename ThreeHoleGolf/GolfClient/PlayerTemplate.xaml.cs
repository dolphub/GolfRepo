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

namespace GolfClient
{
    /// <summary>
    /// Interaction logic for PlayerTemplate.xaml
    /// </summary>
    public partial class PlayerTemplate : UserControl
    {
        public PlayerTemplate(string _name, int _score)
        {
            InitializeComponent();
            //this.MouseEnter += (s, e) => {
            //    this.PlayerBorder.BorderBrush = new SolidColorBrush(Colors.Green);    
            //};

            //this.MouseLeave += (s, e) => {
            //    this.PlayerBorder.BorderBrush = new SolidColorBrush(Colors.Black);
            //};
            this.PlayerName.Content += _name;
            this.PlayerScore.Content += _score.ToString();
            this.Name = _name.Replace('_', ' ');
        }

    }
}
