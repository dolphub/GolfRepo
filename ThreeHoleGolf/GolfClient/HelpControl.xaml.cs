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
    /// Interaction logic for HelpControl.xaml
    /// </summary>
    public partial class HelpControl : UserControl
    {
        public HelpControl()
        {
            InitializeComponent();

            FlowDocument document = new FlowDocument();

            document.Blocks.Add(new Paragraph(new Run("The Play")));

            document.Blocks.Add(new Paragraph(new Run("The player to the dealer's left begins, and the turn to play passes clockwise. " + 
                "At your turn you must either draw the top card of the face-down stock, or draw the top discard. " + 
                "You may use the card you draw to replace any one of the six cards of your layout, but if you choose to replace a face-down " + 
                "card you are not allowed to look at it before deciding to replace it. You place the new card face-up in your layout, and the " + 
                "card that previously occupied that position is placed face-up on top of the discard pile. It is then the next player's turn. " + 
                "If you draw a card from the face-down card from the stock, you may decide that you do not want it anywhere in your layout. " + 
                "In that case you simply discard the drawn card face-up on the discard pile, and it is the next player's turn. It is, however, " +
                "illegal to draw the top card of the discard pile and discard the same card again, leaving the situation unchanged: if you choose to " + 
                "take the discard, you must use it to replace one of your layout cards. " + 
                "The play ends as soon as the last of a player's six cards is face up. The hand is then scored.")));

            document.Blocks.Add(new Paragraph(new Run("Scoring")));

            document.Blocks.Add(new Paragraph(new Run("At the end of the play, each player's layout of six cards is turned face-up and scored as follows.")));

            document.Blocks.Add(new Paragraph(new Run("Each ace counts 1 point.\n" + "Each two counts minus two points.\n" + "Each numeral card from 3 to 10 scores face value.\n" +
                "Each Jack or Queen scores 10 points.\n" + "Each King scores zero points.\n" + 
                "A pair of equal cards in the same column scores zero points for the column (even if the equal cards are twos).\n" + 
                "The player who has the lowest cumulative score after nine deals wins.")));

            rtb_Content.Document = document;

        }
    }
}
