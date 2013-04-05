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
        [OperationContract( IsOneWay = true )] void UpdateDrawn( string _drawn );
        [OperationContract( IsOneWay = true )] void UpdateDiscard( string _discard );
        [OperationContract( IsOneWay = true )] void UpdateContestantCard( string userName, string newCard, string oldCard, bool fromDrawn, string btnName, string objectName );
        [OperationContract( IsOneWay = true )] void NewPlayerJoin( string[] _names );
        [OperationContract( IsOneWay = true )] void PlayerDisconnected( string[] _names );
        [OperationContract( IsOneWay = true )] void UpdateQueue( string userName, bool isReady, bool StartGame, bool hasEnoughPlayers );
        [OperationContract( IsOneWay = true )] void UpdateGameState( Player[] _players );
        [OperationContract( IsOneWay = true )] void ResetClients();
        [OperationContract( IsOneWay = true )] void NextTurn( Player[] _players );
        [OperationContract( IsOneWay = true )] void sendResults( string[] names, int[] points );
        [OperationContract( IsOneWay = true )] void SendWaitQueueFreeze();
        [OperationContract( IsOneWay = true )] void SendMessage( string msg );
        [OperationContract( IsOneWay = true )] void GameEnding();
    }

    [ServiceContract( CallbackContract = typeof( IGameCallBack ) )]
    public interface IGameSystem
    {
        [OperationContract]                     string Draw();
        [OperationContract]                     void ContestentSwap( string userName, string newCard, string oldCard, bool fromDiscard, string btnName, string objectName );
        [OperationContract]                     void UpdateQueue( string username, bool isReady );
        [OperationContract]                     List<string> DrawThreeCards();
        [OperationContract( IsOneWay = true )]  void Shuffle();
        [OperationContract]                     void GameState();
        [OperationContract]                     void ResetGame();
        [OperationContract]                     void StartGame();
        [OperationContract( IsOneWay = true )]  void GetResults();
        [OperationContract]                     int CardValue( string card );
        [OperationContract( IsOneWay = true )]  void Points( string[] _cards, string name );
        [OperationContract]                     int GetPoints( string name );
        [OperationContract( IsOneWay = true )]  void UpdateTurn();
        [OperationContract]                     bool Join( string name );
        [OperationContract( IsOneWay = true )]  void Leave( string name );
        int NumCards { [OperationContract] get; }
        int NumDecks { [OperationContract] get; [OperationContract] set; }
        string DiscardedCard { [OperationContract]get; [OperationContract]set; }
        bool GameInProgress { [OperationContract] get; [OperationContract]  set; }
        bool LastRound { [OperationContract] get; [OperationContract] set; }
    }

    [ServiceBehavior( InstanceContextMode = InstanceContextMode.Single )]
    public class GameSystem : IGameSystem
    {
        #region Service Local Variables
        private List<Card> cards = new List<Card>();
        public List<Player> Players;
        private Dictionary<string, IGameCallBack> gameCallBacks = new Dictionary<string, IGameCallBack>();
        private Dictionary<string, IGameCallBack> gameWaitQueue = new Dictionary<string, IGameCallBack>();
        private int cardIdx, playerCount = 0, endGameCounter = 0, numDecks = 1, _currentTurnPosition = 0, _lastRoundStoppingPoint;
        private string _drawnCard, _discardCard;
        private bool _gameInProgrees = false, _lastRound = false;
        #endregion
        #region Ctors
        public GameSystem()
        {
            cardIdx = 0;
            repopulate();
            Players = new List<Player>();
        }
        public GameSystem( int numOfDecks )
        {
            cardIdx = 0;
            numDecks = numOfDecks;
            repopulate();
            Players = new List<Player>();
        }
        #endregion
        #region GameCallBackSystem
        // Join a game
        public bool Join( string name )
        {
            bool isValid = false;
            foreach( char ch in name )
            {
                if( char.IsLetter( ch ) )
                {
                    isValid = true;
                }
                else if( char.IsLetterOrDigit( ch ) )
                    isValid = true;
            }
            if( char.IsLetter( name[0] ) )
                isValid = true;
            else
                isValid = false;
            // Unique name for the game
            if( gameCallBacks.ContainsKey( name.ToUpper() ) )
                return false;
            else if( !isValid )
                return false;
            else
            {
                // Retrieve a clients callback proxy
                IGameCallBack gcb = OperationContext.Current.GetCallbackChannel<IGameCallBack>();
                if( this.GameInProgress )
                {
                    gameWaitQueue.Add( name.ToUpper(), gcb );
                    gcb.SendWaitQueueFreeze();
                }
                else
                {
                    gameCallBacks.Add( name.ToUpper(), gcb );
                    Players.Add( new Player( name.ToUpper(), false ) );
                    Console.WriteLine( name + " has connected." );
                    if( gameCallBacks.Count > 1 )
                    {
                        foreach( IGameCallBack cb in gameCallBacks.Values )
                            cb.NewPlayerJoin( gameCallBacks.Keys.ToArray() );
                    }
                    GameState();
                }
                return true;
            }
        }
        // Leave a game
        public void Leave( string name )
        {
            if( gameCallBacks.ContainsKey( name.ToUpper() ) )
            {
                var p = Players.FirstOrDefault<Player>( player => player.Name == name.ToUpper() );
                Players.Remove( ( Player )p );
                gameCallBacks.Remove( name.ToUpper() );
                Console.WriteLine( name + " has disconnected." );
                if( gameCallBacks.Count > 0 )
                {
                    foreach( IGameCallBack cb in gameCallBacks.Values )
                        cb.PlayerDisconnected( gameCallBacks.Keys.ToArray() );
                }
                this._gameInProgrees = false;
                ResetGame();
            }
        }
        // Update the queue and send to all clients
        public void UpdateQueue( string username, bool isReady )
        {
            int queue = 0;
            bool startGame = false;
            bool hasEnoughPlayers = false;
            if( Players.Count > 1 )
                hasEnoughPlayers = true;
            Players.All( p =>
            {
                if( p.Name == username.ToUpper() )
                    p.isReady = isReady;
                if( !p.isReady )
                    ++queue;
                return true;
            } );
            if( queue == 0 )
                startGame = true;
            foreach( IGameCallBack gcb in gameCallBacks.Values )
                gcb.UpdateQueue( username, isReady, startGame, hasEnoughPlayers );
        }
        // Send the updated card, to all clients to show
        public void ContestentSwap( string userName, string newCard, string oldCard, bool fromDiscard, string btnName, string objectName )
        {
            foreach( IGameCallBack gcb in gameCallBacks.Values )
                gcb.UpdateContestantCard( userName, newCard, oldCard, fromDiscard, btnName, objectName );
        }
        #endregion
        // returns the next card
        public string Draw()
        {
            try
            {
                if( cardIdx == cards.Count() )
                {
                    throw new System.IndexOutOfRangeException( "The shoe is empty. Please reset.\n" );
                }
                else
                {
                    Console.WriteLine( "Dealing the " + cards[cardIdx].Name + "." );
                    _drawnCard = cards[cardIdx].sName;
                    //Update all clients
                    foreach( IGameCallBack gcb in gameCallBacks.Values )
                        gcb.UpdateDrawn( this._drawnCard );
                    return cards[cardIdx++].sName;
                }
            }
            catch( Exception ex )
            {
                Console.Write( ex.Message );
                return "shoe empty";
            }
        }
        // Resets the game to a start point, after points calculation
        public void ResetGame()
        {
            try
            {
                if( playerCount != Players.Count() )
                {
                    this.Shuffle();
                    Players.All( p => { p.isReady = false; p.Points = 0; return true; } );
                    foreach( IGameCallBack gcb in gameCallBacks.Values )
                        gcb.ResetClients();
                    GameState();

                    this.LastRound = false;
                    if( gameWaitQueue.Count > 0 )
                    {
                        foreach( string x in gameWaitQueue.Keys )
                        {
                            gameCallBacks.Add( x, gameWaitQueue[x] );
                            Players.Add( new Player( x, false ) );

                        }
                        gameWaitQueue.Clear();

                        foreach( IGameCallBack gcb in gameCallBacks.Values )
                            gcb.NewPlayerJoin( gameCallBacks.Keys.ToArray() );
                    }
                }

            }
            catch( Exception ex )
            {
                Console.WriteLine( ex.Message );
            }

        }
        // Format the name to mimic that of the clients username
        private string formatName( string _raw )
        {
            string refined = _raw;
            if( _raw.Length > 1 )
                return _raw[0].ToString().ToUpper() + _raw.Substring( 1, _raw.Length - 1 ).ToLower();
            else
                return _raw;
        }
        // Draw three cards for a client 
        public List<string> DrawThreeCards()
        {
            List<string> threecards;
            try
            {
                threecards = new List<string>();
                for( int i = 0; i < 3; ++i )
                {
                    if( cardIdx == cards.Count() )
                    {
                        throw new System.IndexOutOfRangeException( "The shoe is empty. Please reset.\n" );
                    }
                    else
                    {
                        Console.WriteLine( "Dealing the " + cards[cardIdx].Name + "." );
                        threecards.Add( cards[cardIdx++].sName );
                    }
                }
                return threecards;
            }
            catch( Exception ex )
            {
                Console.Write( ex.Message.ToString() );
                return new List<string>();
            }
        }
        // Assign the points, to the players
        private DateTime _pointsUpdate = DateTime.Now;
        public void Points( string[] _cards, string name )
        {
            if( DateTime.Now - _pointsUpdate > TimeSpan.FromMilliseconds( .05 ) )
            {
                foreach( Player player in Players )
                {
                    if( player.Name.ToUpper() == name.ToUpper() )
                    {
                        int accumulatedPoints = 0;
                        for( int i = 0; i < _cards.Length; ++i )
                        {
                            accumulatedPoints += CardValue( _cards[i] );
                        }
                        player.Points = accumulatedPoints;
                        break;
                    }
                }
                GameState();
                _pointsUpdate = DateTime.Now;
            }
        }
        public int CardValue( string card )
        {
            foreach( Card c in cards )
            {
                if( card == c.sName )
                    return c.Value;
            }

            //Bad Value
            return -10;
        }
        // Send the results to all clients
        public void GetResults()
        {
            ++endGameCounter;
            if( endGameCounter == Players.Count )
            {

                List<string> names = new List<string>();
                List<int> points = new List<int>();
                foreach( Player player in Players )
                {
                    names.Add( player.Name );
                    points.Add( player.Points );
                }
                foreach( IGameCallBack gcb in gameCallBacks.Values )
                    gcb.sendResults( names.ToArray(), points.ToArray() );

                endGameCounter = 0;
            }

        }
        // Retreive points from card name
        public int GetPoints( string name )
        {
            foreach( Player player in Players )
            {
                if( player.Name == name.ToUpper() )
                    return player.Points;
            }
            return 0;
        }
        // Re-order the cards in the shoe
        public void Shuffle()
        {
            Console.WriteLine( "Shuffling the deck..." );
            randomizeCards();
        }
        // Send an update to all clients to change queue and points
        public void GameState()
        {
            foreach( IGameCallBack gcb in gameCallBacks.Values )
                gcb.UpdateGameState( Players.ToArray() );
        }
        // Updates the turn of the game, onto the next player
        private DateTime _lastUpdate = DateTime.Now;
        public void UpdateTurn()
        {
            // Prevent a double turn from taking place
            if( DateTime.Now - _lastUpdate > TimeSpan.FromSeconds( 0.50 ) )
            {
                // Create a cyclical turn-based system
                if( this._currentTurnPosition >= Players.Count() )
                    this._currentTurnPosition = 0;
                // Check to make sure there is a game in progress 
                if( GameInProgress )
                {
                    // Update all players turns to false
                    Players.All( p => { p.myTurn = false; return true; } );
                    Console.WriteLine( Players[_currentTurnPosition].Name + "'s Turn." ); // Notify the server for observation
                    // Access the current players turn and flip the bool for their turn
                    Players[_currentTurnPosition++].myTurn = true;
                    // Update all clients with the new players turn
                    foreach( IGameCallBack gcb in gameCallBacks.Values )
                    {
                        gcb.NextTurn( Players.ToArray() );
                        // If the last round has been notified by all cards being face up
                        // send a notification to all players that they have only
                        // one turn left
                        if( LastRound )
                            gcb.SendMessage( "Last Turn!!" );
                    }
                    // If everyone has had a turn after the LastRound 
                    // has been initiated, set in motion the ending of the game
                    if( LastRound )
                        if( _currentTurnPosition == _lastRoundStoppingPoint )
                            GameInProgress = false;
                    _lastUpdate = DateTime.Now;
                }
            }



        }
        // Send notifications of all the clients to start their game
        public void StartGame()
        {
            ++playerCount; // Only start the game once for all clients
            if( this.playerCount == Players.Count )
            {
                if( !GameInProgress )
                {
                    this.GameInProgress = true;
                }
            }
        }
        // number of cards remaining in the shoe
        public int NumCards
        {
            get { return cards.Count - cardIdx; }
        }
        // helper methods
        private void repopulate()
        {
            // remove "old" cards
            cards.Clear();
            // populate with new cards
            // for each deck
            for( int d = 0; d < numDecks; d++ )
            {
                // for each suit in this deck
                foreach( Card.SuitID s in Enum.GetValues( typeof( Card.SuitID ) ) )
                {
                    if( !( s == Card.SuitID.Red || s == Card.SuitID.Black ) )
                    {
                        // foreach rank in this suit
                        foreach( Card.RankID r in Enum.GetValues( typeof( Card.RankID ) ) )
                        {
                            if( !( r == Card.RankID.Joker ) )
                            {
                                // add card and its effective scoring 
                                if( r == Card.RankID.Ace )
                                    cards.Add( new Card( s, r, 1 ) );
                                else if( r == Card.RankID.Two )
                                    cards.Add( new Card( s, r, 2 ) );
                                else if( r == Card.RankID.Three )
                                    cards.Add( new Card( s, r, 3 ) );
                                else if( r == Card.RankID.Four )
                                    cards.Add( new Card( s, r, 4 ) );
                                else if( r == Card.RankID.Five )
                                    cards.Add( new Card( s, r, 5 ) );
                                else if( r == Card.RankID.Six )
                                    cards.Add( new Card( s, r, 6 ) );
                                else if( r == Card.RankID.Seven )
                                    cards.Add( new Card( s, r, 7 ) );
                                else if( r == Card.RankID.Eight )
                                    cards.Add( new Card( s, r, 8 ) );
                                else if( r == Card.RankID.Nine )
                                    cards.Add( new Card( s, r, 9 ) );
                                else if( r == Card.RankID.Queen || r == Card.RankID.Jack || r == Card.RankID.Ten )
                                    cards.Add( new Card( s, r, 10 ) );
                                else if( r == Card.RankID.King )
                                    cards.Add( new Card( s, r, 0 ) );
                            }
                        }
                    }
                }
                cards.Add( new Card( Card.SuitID.Black, Card.RankID.Joker, -2 ) );
                cards.Add( new Card( Card.SuitID.Black, Card.RankID.Joker, -2 ) );
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
            for( int i = 0; i < cards.Count; i++ )
            {
                // choose a random index
                randIndex = rand.Next( cards.Count );
                if( randIndex != i )
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

                foreach( IGameCallBack gcb in gameCallBacks.Values )
                    gcb.UpdateDiscard( _discardCard );
            }
            get { return _discardCard; }
        }
        // Variable to set the game in progress with some functionality
        public bool GameInProgress
        {
            get { return _gameInProgrees; }
            set
            {
                _gameInProgrees = value;
                // If value is set to true, update the turn and set the
                // game in motion
                if( value )
                    UpdateTurn(); // Initiation the first turn
                else // else if the bool is false, we send final scores, and reset game
                {
                    playerCount = 0;
                    foreach( IGameCallBack gcb in gameCallBacks.Values )
                        gcb.GameEnding();
                }
            }
        }
        // Last round getter/setter
        public bool LastRound
        {
            get { return this._lastRound; }
            set
            {
                _lastRound = value;
                // set the postition the last round stops at
                if( _lastRound )
                    _lastRoundStoppingPoint = _currentTurnPosition;
            }
        }
    } // end class
} // end namespace
