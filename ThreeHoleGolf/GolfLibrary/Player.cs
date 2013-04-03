using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Serialization;


using System.ServiceModel;

namespace GolfLibrary
{
    
    [Serializable]
    public class Player
    {
        public string Name { get; set; }
        
        public int Points { get; set; }
        
        public bool isReady { get; set; }

        public bool myTurn { get; set; }
        
        public Player(string _name, bool _isReady)
        {
            this.Name = _name;
            this.isReady = _isReady;
            this.Points = 0;
            myTurn = false;
        }
    }
}
