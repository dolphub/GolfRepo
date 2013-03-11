using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolfLibrary
{
    //Need to also add Jokers
    public interface ICard
    {
        Card.SuitID Suit { get; }
        Card.RankID Rank { get; }
        string Name { get; }
        string sName { get; }
        Uri Card_uri { set; }
    }

    public class Card : ICard
    {
        // enum of possible values for the card's suit
        public enum SuitID
        {
            Clubs, Diamonds, Hearts, Spades, Red, Black
        };

        // enum of possible values for the card's rank
        public enum RankID
        {
            Ace, King, Queen, Jack, Ten, Nine, Eight, Seven, Six,
            Five, Four, Three, Two, Joker
        };

        // member variables and accessor methods
        public SuitID Suit { get; private set; }
        public RankID Rank { get; private set; }
        public string Name
        {
            get
            {
                return Rank.ToString() + " of " + Suit.ToString();
            }
        }

        public string sName
        {
            get
            {
                return Rank.ToString() + Suit.ToString();
            }
        }

        // c'tor which identifies which card this is
        public Card(SuitID s, RankID r)
        {
            Suit = s;
            Rank = r;
        }

        private Uri _card_uri;

        public Uri Card_uri
        {
            get { return _card_uri; }
            set { _card_uri = value; }
        }
    }
}

