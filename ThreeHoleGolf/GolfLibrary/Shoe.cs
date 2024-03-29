﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ServiceModel;

namespace GolfLibrary
{


    public interface ICallBack
    {
        [OperationContract(IsOneWay = true)]
        void UpdateGui(CallBackInfo info);
    }



    [ServiceContract(CallbackContract = typeof(ICallBack))]
    public interface IShoe
    {
        [OperationContract]                     Guid RegisterForCallbacks();
        [OperationContract(IsOneWay = true)]    void UnregisterForCallbacks(Guid key);
        [OperationContract]                     string Draw();
        [OperationContract]                     void Shuffle();
        int NumCards { [OperationContract] get; }
        int NumDecks { [OperationContract] get; [OperationContract] set; }
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class Shoe : IShoe
    {
       

        // member variables

        private Dictionary<Guid, ICallBack> clientCallBacks = new Dictionary<Guid, ICallBack>();
        private List<Card> cards = new List<Card>();
        private int numDecks = 1;
        private int cardIdx;

        #region Ctors
        // C'tors
        public Shoe()
        {
            cardIdx = 0;
            repopulate();
        }

        public Shoe(int numOfDecks)
        {
            cardIdx = 0;
            numDecks = numOfDecks;
            repopulate();
        }
        #endregion


        #region register/unregister CallBacks
        // A client will call this method to register its intent to receive 
        // Message updates via the callback mechanism
        public Guid RegisterForCallbacks()
        {
            ICallBack cb = OperationContext.Current.GetCallbackChannel<ICallBack>();

            Guid key = Guid.NewGuid();
            clientCallBacks.Add(key, cb);

            return key;
        }

        // A client willc all this method to register its wish
        // to no longer receive message updates via the callback mechanism
        public void UnregisterForCallbacks(Guid key)
        {
            if (clientCallBacks.Keys.Contains<Guid>(key))
                clientCallBacks.Remove(key);
        }
        #endregion

        // Notiffies every connected (and "registered") client of the message's current state
        private void updateAllClients(bool reset)
        {
            CallBackInfo info = new CallBackInfo(reset);

            foreach (ICallBack cb in clientCallBacks.Values)
                cb.UpdateGui(info);

        }


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
                    return cards[cardIdx++].sName;
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message.ToString());
                return cards[cardIdx - 1].sName;
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
                            if ( !( r == Card.RankID.Joker ))
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
    } // end class
}
