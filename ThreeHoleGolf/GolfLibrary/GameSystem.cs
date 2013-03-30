using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ServiceModel;

namespace GolfLibrary
{
<<<<<<< HEAD:ThreeHoleGolf/GolfLibrary/Shoe.cs
   [ServiceContract]
    public interface IShoe
=======



    public interface IGameCallBack
    {
        [OperationContract(IsOneWay = true)]
        void UpdateDrawn(string _drawn);
        //void UpdateDiscard(string _discard);
    }

    [ServiceContract(CallbackContract = typeof(IGameCallBack))]
    public interface IGameSystem
>>>>>>> master:ThreeHoleGolf/GolfLibrary/GameSystem.cs
    {
        [OperationContract]
        string Draw();

        [OperationContract]
        List<string> DrawThreeCards();

        [OperationContract(IsOneWay = true)]
        void Shuffle();

        [OperationContract]
        bool Join(string name);

        [OperationContract(IsOneWay = true)]
        void Leave(string name);

        int NumCards { [OperationContract] get; }
        int NumDecks { [OperationContract] get; [OperationContract] set; }
        string DiscardedCard { [OperationContract]get; [OperationContract]set; }

    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class GameSystem : IGameSystem
    {
<<<<<<< HEAD:ThreeHoleGolf/GolfLibrary/Shoe.cs
=======



>>>>>>> master:ThreeHoleGolf/GolfLibrary/GameSystem.cs
        // member variables
        private List<Card> cards = new List<Card>();
        private Dictionary<string, IGameCallBack> gameCallBacks = new Dictionary<string, IGameCallBack>();
        private int numDecks = 1;
        private int cardIdx;
        private string _drawnCard, _discardCard;


        // C'tors
        public GameSystem()
        {
            cardIdx = 0;
            repopulate();
        }

        public GameSystem(int numOfDecks)
        {
            cardIdx = 0;
            numDecks = numOfDecks;
            repopulate();
        }

        #region GameCallBackSystem
        public bool Join(string name)
        {
            // Unique name for the game
            if (gameCallBacks.ContainsKey(name.ToUpper()))
                return false;
            else
            {
                // Retrieve a clients callback proxy
                IGameCallBack gcb = OperationContext.Current.GetCallbackChannel<IGameCallBack>();

                gameCallBacks.Add(name.ToUpper(), gcb);
                Console.WriteLine(name + " has connected.");

                return true;
            }
        }

        public void Leave(string name)
        {
            if (gameCallBacks.ContainsKey(name.ToUpper()))
            {
                gameCallBacks.Remove(name.ToUpper());
                Console.WriteLine(name + " has disconnected.");
            }
        }


        private void updateAllUsers()
        {
            //String[] msgs = messages.ToArray<string>();

            foreach (IGameCallBack gcb in gameCallBacks.Values)
                gcb.UpdateDrawn(this._drawnCard);

            //foreach (IUserCallBack cb in userCallbacks.Values)
            //    cb.SendAllMessages(msgs);
        }

        #endregion



        // public methods and properties
        // returns the next card
        public string Draw()
        {
            try
            {
<<<<<<< HEAD:ThreeHoleGolf/GolfLibrary/Shoe.cs
                Console.WriteLine("Dealing the " + cards[cardIdx].Name + ".");
                return cards[cardIdx++].sName;
=======
                if (cardIdx == cards.Count())
                {
                    throw new System.IndexOutOfRangeException("The shoe is empty. Please reset.\n");

                }
                else
                {
                    Console.WriteLine("Dealing the " + cards[cardIdx].Name + ".");
                    _drawnCard = cards[cardIdx].sName;
                    updateAllUsers();
                    return cards[cardIdx++].sName;
                }
>>>>>>> master:ThreeHoleGolf/GolfLibrary/GameSystem.cs
            }
            catch (Exception ex)
            {
                //Console.Write(ex.Message.ToString());
                return "shoe empty";
            }

<<<<<<< HEAD:ThreeHoleGolf/GolfLibrary/Shoe.cs
=======
        }

        public List<string> DrawThreeCards()
        {
            List<string> threecards;
            try
            {
                threecards = new List<string>();
                for (int i = 0; i < 3; ++i)
                {
                    if (cardIdx == cards.Count())
                    {
                        throw new System.IndexOutOfRangeException("The shoe is empty. Please reset.\n");

                    }
                    else
                    {
                        Console.WriteLine("Dealing the " + cards[cardIdx].Name + ".");
                        threecards.Add(cards[cardIdx++].sName);
                    }
                }
                return threecards;
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message.ToString());
                return new List<string>();
            }
>>>>>>> master:ThreeHoleGolf/GolfLibrary/GameSystem.cs
        }

        // reorder the cards in the shoe
        public void Shuffle()
        {
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
                    if (!(s == Card.SuitID.Red || s == Card.SuitID.Black))
                    {
                        // foreach rank in this suit
                        foreach (Card.RankID r in Enum.GetValues(typeof(Card.RankID)))
                        {
                            if (!(r == Card.RankID.Joker))
                                // add card
                                cards.Add(new Card(s, r));
                        }
                    }
                }

                cards.Add(new Card(Card.SuitID.Black, Card.RankID.Joker));
                cards.Add(new Card(Card.SuitID.Black, Card.RankID.Joker));
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

        public string DiscardedCard
        {
            set { _discardCard = value; }
            get { return _discardCard; }
        }


    } // end class
}
