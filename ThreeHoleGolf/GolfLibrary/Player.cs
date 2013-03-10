using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ServiceModel;

namespace GolfLibrary
{
    [ServiceContract]
    public interface IPlayer
    {
        string Name { [OperationContract] get; [OperationContract] set; }
        int Points { [OperationContract] get; [OperationContract] set; }
        List<Card> Player_cards { [OperationContract] get; [OperationContract] set; }
    }

    class Player
    {
        public string Name { get; set; }
        public int Points { get; set; }
        public List<Card> Player_cards { get; set; }
    }
}
