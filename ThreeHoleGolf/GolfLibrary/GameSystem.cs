﻿using System;
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

        [OperationContract(IsOneWay = true)]
        void UpdateQueue(string userName, bool isReady, bool StartGame, bool hasEnoughPlayers);

        [OperationContract(IsOneWay = true)]
        void UpdateGameState(Player[] _players);

        [OperationContract(IsOneWay = true)]
        void ResetClients();

        [OperationContract(IsOneWay = true)]
        void NextTurn(Player[] _players);


        [OperationContract(IsOneWay = true)]
        void SendWaitQueueFreeze();

        [OperationContract(IsOneWay = true)]
        void SendMessage(string msg);

        [OperationContract(IsOneWay = true)]
        void GameEnding(string[] names, int[] points);
    }

    [ServiceContract(CallbackContract = typeof(IGameCallBack))]
    public interface IGameSystem
    {
        [OperationContract]
        string Draw();

        [OperationContract]
        void ContestentSwap(string userName, string newCard, string oldCard, bool fromDiscard, string btnName, string objectName);

        [OperationContract]
        void UpdateQueue(string username, bool isReady);

        [OperationContract]
        List<string> DrawThreeCards();

        [OperationContract(IsOneWay = true)]
        void Shuffle();

        [OperationContract]
        void GameState();

        [OperationContract]
        void ResetGame();

        [OperationContract]
        void StartGame();


        [OperationContract]
        int CardValue(string card);

        [OperationContract]
        void Points(int cardValue, string name);

        [OperationContract]
        int GetPoints(string name);


        [OperationContract(IsOneWay = true)]
        void UpdateTurn();

        [OperationContract]
        bool Join(string name);

        [OperationContract(IsOneWay = true)]
        void Leave(string name);

        int NumCards { [OperationContract] get; }
        int NumDecks { [OperationContract] get; [OperationContract] set; }
        string DiscardedCard { [OperationContract]get; [OperationContract]set; }
        bool GameInProgress { [OperationContract] get; [OperationContract]  set; }
        bool LastRound { [OperationContract] get; [OperationContract] set; }
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class GameSystem : IGameSystem
    {
        // member variables
        private List<Card> cards = new List<Card>();
        private Dictionary<string, IGameCallBack> gameCallBacks = new Dictionary<string, IGameCallBack>();
        private Dictionary<string, IGameCallBack> gameWaitQueue = new Dictionary<string, IGameCallBack>();
        private int numDecks = 1;
        private int cardIdx, playerCount = 0;
        private string _drawnCard, _discardCard;
        public List<Player> Players;
        private bool _gameInProgrees = false;
        private bool _lastRound = false;
        private int _currentTurnPosition = 0, _lastRoundStoppingPoint;


        // C'tors
        public GameSystem()
        {
            cardIdx = 0;
            repopulate();
            Players = new List<Player>();
        }

        public GameSystem(int numOfDecks)
        {
            cardIdx = 0;
            numDecks = numOfDecks;
            repopulate();
            Players = new List<Player>();
        }

        #region GameCallBackSystem
        public bool Join(string name)
        {
            bool isValid = false;
            foreach (char ch in name)
            {
                if (char.IsLetter(ch))
                {
                    isValid = true;
                }
                else if (char.IsLetterOrDigit(ch))
                    isValid = true;
                
            }

            if (char.IsLetter(name[0]))
                isValid = true;
            else
                isValid = false;

            // Unique name for the game
            if (gameCallBacks.ContainsKey(name.ToUpper()))
                return false;
            else if (!isValid)
                return false;
            else
            {

                // Retrieve a clients callback proxy
                IGameCallBack gcb = OperationContext.Current.GetCallbackChannel<IGameCallBack>();

                if (this.GameInProgress)
                {
                    gameWaitQueue.Add(name.ToUpper(), gcb);
                    gcb.SendWaitQueueFreeze();
                }
                else
                {
                    gameCallBacks.Add(name.ToUpper(), gcb);
                    Players.Add(new Player(name.ToUpper(), false));
                    Console.WriteLine(name + " has connected.");

                    if (gameCallBacks.Count > 1)
                    {
                        foreach (IGameCallBack cb in gameCallBacks.Values)
                            cb.NewPlayerJoin(gameCallBacks.Keys.ToArray());
                    }
                }
                return true;
            }
        }

        public void Leave(string name)
        {
            if (gameCallBacks.ContainsKey(name.ToUpper()))
            {
                var p = Players.FirstOrDefault<Player>(player => player.Name == name.ToUpper());

                Players.Remove((Player)p);
                gameCallBacks.Remove(name.ToUpper());
                Console.WriteLine(name + " has disconnected.");

                if (gameCallBacks.Count > 0)
                {
                    foreach (IGameCallBack cb in gameCallBacks.Values)
                        cb.PlayerDisconnected(gameCallBacks.Keys.ToArray());
                }
                
                this._gameInProgrees = false;
                ResetGame();
            }
        }

       

        public void UpdateQueue(string username, bool isReady)
        {
            int queue = 0;
            bool startGame = false;
            bool hasEnoughPlayers = false;

            if (Players.Count > 1)
                hasEnoughPlayers = true;

            Players.All(p =>
            {
                if (p.Name == username.ToUpper())
                    p.isReady = isReady;
                if (!p.isReady)
                    ++queue;
                return true;
            });

            if (queue == 0)
                startGame = true;

            foreach (IGameCallBack gcb in gameCallBacks.Values)
                gcb.UpdateQueue(username, isReady, startGame, hasEnoughPlayers);
        }

        public void ContestentSwap(string userName, string newCard, string oldCard, bool fromDiscard, string btnName, string objectName)
        {
            foreach (IGameCallBack gcb in gameCallBacks.Values)
                gcb.UpdateContestantCard(userName, newCard, oldCard, fromDiscard, btnName, objectName);
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

        /// <summary>
        /// Resets the game to a start point, after points
        /// calculation
        /// </summary>
        public void ResetGame()
        {
            this.Shuffle();
            Players.All(p => { p.isReady = false; p.Points = 0; return true; });
            foreach (IGameCallBack gcb in gameCallBacks.Values)
                gcb.ResetClients();



            if (gameWaitQueue.Count > 0)
            {
                foreach (string x in gameWaitQueue.Keys)
                {
                    gameCallBacks.Add(x, gameWaitQueue[x]);
                    Players.Add(new Player(x, false));

                }
                gameWaitQueue.Clear();

                foreach (IGameCallBack gcb in gameCallBacks.Values)
                    gcb.NewPlayerJoin(gameCallBacks.Keys.ToArray());
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

        private DateTime _pointsUpdate = DateTime.Now;
        public void Points(int cardValue, string name)
        {
            if (DateTime.Now - _pointsUpdate > TimeSpan.FromSeconds(1))
            {
                foreach (Player player in Players)
                {
                    if (player.Name == name.ToUpper())
                        player.Points += cardValue;
                }
                _pointsUpdate = DateTime.Now;
            }
        }

        public int GetPoints(string name)
        {
            foreach (Player player in Players)
            {
                if (player.Name == name.ToUpper())
                    return player.Points;
            }
            //Bad value
            return -10;
        }

        // reorder the cards in the shoe
        public void Shuffle()
        {
            randomizeCards();
        }

        public void GameState()
        {
            foreach (IGameCallBack gcb in gameCallBacks.Values)
                gcb.UpdateGameState(Players.ToArray());
        }

        private DateTime _lastUpdate = DateTime.Now;
        public void UpdateTurn()
        {
            if (DateTime.Now - _lastUpdate > TimeSpan.FromSeconds(1))
            {
                if (this._currentTurnPosition >= Players.Count())
                {
                    this._currentTurnPosition = 0;
                }



                if (GameInProgress)
                {
                    Players.All(p => { p.myTurn = false; return true; });
                    Console.WriteLine(Players[_currentTurnPosition].Name + "'s Turn.");
                    Players[_currentTurnPosition++].myTurn = true;

                    foreach (IGameCallBack gcb in gameCallBacks.Values)
                    {
                        gcb.NextTurn(Players.ToArray());
                        if (LastRound)
                            gcb.SendMessage("Last Turn!!");
                    }


                    if (LastRound)
                        if (_currentTurnPosition == _lastRoundStoppingPoint)
                            GameInProgress = false;

                    _lastUpdate = DateTime.Now;
                }
            }



        }

        public void StartGame()
        {
            ++playerCount;
            if (this.playerCount == Players.Count)
            {
                if (!GameInProgress)
                {
                    this.GameInProgress = true;
                }
            }
        }

        public int CardValue(string card)
        {
            foreach (Card c in cards)
            {
                if (card == c.sName)
                    return c.Value;
            }

            //Bad Value
            return -10;
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
                            {
                                // add card
                                if (r == Card.RankID.Ace)
                                    cards.Add(new Card(s, r, 1));
                                else if (r == Card.RankID.Two)
                                    cards.Add(new Card(s, r, 2));
                                else if (r == Card.RankID.Three)
                                    cards.Add(new Card(s, r, 3));
                                else if (r == Card.RankID.Four)
                                    cards.Add(new Card(s, r, 4));
                                else if (r == Card.RankID.Five)
                                    cards.Add(new Card(s, r, 5));
                                else if (r == Card.RankID.Six)
                                    cards.Add(new Card(s, r, 6));
                                else if (r == Card.RankID.Seven)
                                    cards.Add(new Card(s, r, 7));
                                else if (r == Card.RankID.Eight)
                                    cards.Add(new Card(s, r, 8));
                                else if (r == Card.RankID.Nine)
                                    cards.Add(new Card(s, r, 9));
                                else if (r == Card.RankID.Queen || r == Card.RankID.Jack || r == Card.RankID.Ten)
                                    cards.Add(new Card(s, r, 10));
                                else if (r == Card.RankID.King)
                                    cards.Add(new Card(s, r, 0));
                            }
                        }
                    }
                }

                cards.Add(new Card(Card.SuitID.Black, Card.RankID.Joker, -2));
                cards.Add(new Card(Card.SuitID.Black, Card.RankID.Joker, -2));
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


        // Game Wide set the discarded card
        public string DiscardedCard
        {
            set
            {
                _discardCard = value;

                foreach (IGameCallBack gcb in gameCallBacks.Values)
                    gcb.UpdateDiscard(_discardCard);
            }
            get { return _discardCard; }
        }

        public bool GameInProgress
        {
            get
            {
                return _gameInProgrees;
            }
            set
            {
                _gameInProgrees = value;
                // If value is set to true, update the turn and set the
                // game in motion
                if (value)
                {
                    UpdateTurn();
                }
                else // else if the bool is false, we send final scores, and reset game
                {
                    List<string> names = new List<string>();
                    List<int> points = new List<int>();
                    foreach (Player player in Players)
                    {
                        names.Add(player.Name);
                        points.Add(player.Points);
                    }

                    foreach (IGameCallBack gcb in gameCallBacks.Values)
                        gcb.GameEnding(names.ToArray(), points.ToArray());
                    System.Threading.Thread.Sleep(5000);
                    ResetGame();
                }
            }
        }


        public bool LastRound
        {
            get
            {
                return this._lastRound;
            }
            set
            {
                _lastRound = value;
                if (_lastRound)
                    _lastRoundStoppingPoint = _currentTurnPosition;
            }
        }

    } // end class
}
