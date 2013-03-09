using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ServiceModel;

namespace GolfLibrary
{
    [ServiceContract]
    public interface IShoe
    {
        [OperationContract]
        string Draw();
        [OperationContract]
        void Shuffle();
        int NumCards { [OperationContract] get; }
        int NumDecks { [OperationContract] get; [OperationContract] set; }
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class Shoe : IShoe
    {
        // member variables
        private List<Card> cards = new List<Card>();
        private int numDecks = 1;
        private int cardIdx;

        // C'tors
        public Shoe()
        {
            cardIdx = 0;
            repopulate();
            Console.WriteLine("Creating a shoe");

        }

        public Shoe(int numOfDecks)
        {
            cardIdx = 0;
            numDecks = numOfDecks;
            repopulate();
        }




        // public methods and properties

        // this is not available through the interface
        public string DoAwesomeStuff()
        {
            return "Yeah!";
        }

        // returns the next card
        public string Draw()
        {
            if (cardIdx == cards.Count)
                throw new System.IndexOutOfRangeException("The shoe is empty. Please reset.");

            Console.WriteLine("Dealing the " + cards[cardIdx].Name + ".");
            return cards[cardIdx++].Name;
        }

        // reorder the cards in the shoe
        public void Shuffle()
        {
            Console.WriteLine("Shuffling the shoe.");
            randomizeCards();
        }

        // number of cards remaining in the shoe
        public int NumCards
        {
            get { return cards.Count - cardIdx; }
        }

        // number of decks in the shoe
        public int NumDecks
        {
            get { return numDecks; }
            set
            {
                if (numDecks != value)
                {
                    Console.WriteLine("Now using {0} decks.", numDecks);
                    numDecks = value;
                    repopulate();
                }
            }
        }




        // helper methods
        private void repopulate()
        {
            // remove "old" cards
            cards.Clear();

            // populate with new cards
            // for each deck
            for (int d = 0; d < numDecks; d++)
            {
                // for each suit in this deck
                foreach (Card.SuitID s in Enum.GetValues(typeof(Card.SuitID)))
                {
                    // foreach rank in this suit
                    foreach (Card.RankID r in Enum.GetValues(typeof(Card.RankID)))
                    {
                        // add card
                        cards.Add(new Card(s, r));
                    }
                }
            }
            // shuffle cards
            randomizeCards();
        }

        // reorder the cards
        private void randomizeCards()
        {
            Random rand = new Random();
            Card hold;
            int randIndex;

            for (int i = 0; i < cards.Count; i++)
            {
                // choose a random index
                randIndex = rand.Next(cards.Count);

                if (randIndex != i)
                {
                    // swap elements at indexes i and randIndex
                    hold = cards[i];
                    cards[i] = cards[randIndex];
                    cards[randIndex] = hold;
                }
            }

            // start dealing off the top of the deck
            cardIdx = 0;

        }
    } // end class
}
