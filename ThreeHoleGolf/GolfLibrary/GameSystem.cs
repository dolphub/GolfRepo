using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ServiceModel;

namespace GolfLibrary
{

    public interface IGameCallBack
    {
        [OperationContract(IsOneWay = true)]
        void UpdateDrawn(string _drawn);

        [OperationContract(IsOneWay = true)]
        void UpdateDiscard(string _discard);

        [OperationContract(IsOneWay = true)]
        void UpdateContestantCard(string userName, string newCard, string oldCard, bool fromDrawn, string btnName, string objectName);

        [OperationContract(IsOneWay = true)]
        void NewPlayerJoin(string[] _names);

        [OperationContract(IsOneWay = true)]
        void PlayerDisconnected(string[] _names);

        //void UpdateDiscard(string _discard);
    }

    [ServiceContract(CallbackContract = typeof(IGameCallBack))]
    public interface IGameSystem
    {
        [OperationContract]
        string Draw();

        [OperationContract]
        void ContestentSwap(string userName, string newCard, string oldCard, bool fromDiscard, string btnName, string objectName);

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
//        // member variables
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

                if (gameCallBacks.Count > 1)
                {
                    foreach (IGameCallBack cb in gameCallBacks.Values)
                        cb.NewPlayerJoin(gameCallBacks.Keys.ToArray());
                }

                return true;
            }
        }

        public void Leave(string name)
        {
            if (gameCallBacks.ContainsKey(name.ToUpper()))
            {
                gameCallBacks.Remove(name.ToUpper());
                Console.WriteLine(name + " has disconnected.");

                if (gameCallBacks.Count > 0)
                {
                    foreach (IGameCallBack cb in gameCallBacks.Values)
                        cb.PlayerDisconnected(gameCallBacks.Keys.ToArray());
                }
            }
        }


        private void updateAllUsers()
        {
        }

        #endregion



        // public methods and properties
        // returns the next card
        public string Draw()
        {
            try
            {
                if (cardIdx == cards.Count())
                {
                    throw new System.IndexOutOfRangeException("The shoe is empty. Please reset.\n");

                }
                else
                {
                    Console.WriteLine("Dealing the " + cards[cardIdx].Name + ".");
                    _drawnCard = cards[cardIdx].sName;

                    //Update all clients
                    foreach (IGameCallBack gcb in gameCallBacks.Values)
                        gcb.UpdateDrawn(this._drawnCard);

                    return cards[cardIdx++].sName;
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                return "shoe empty";
            }
        }

        private string formatName(string _raw)
        {
            string refined = _raw;
            if (_raw.Length > 1)
                return _raw[0].ToString().ToUpper() + _raw.Substring(1, _raw.Length - 1).ToLower();
            else
                return _raw;  
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
            set {
                _discardCard = value;

                foreach (IGameCallBack gcb in gameCallBacks.Values)
                    gcb.UpdateDiscard(_discardCard);
            }
            get { return _discardCard; }
        }


        public void ContestentSwap(string userName, string newCard, string oldCard, bool fromDiscard, string btnName, string objectName)
        {
            foreach (IGameCallBack gcb in gameCallBacks.Values)
                gcb.UpdateContestantCard(userName, newCard, oldCard, fromDiscard, btnName, objectName);
        }
    } // end class
}
