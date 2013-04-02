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
        bool isReady { [OperationContract]get; [OperationContract]set; }
        IGameCallBack UIdentity { [OperationContract]get; [OperationContract]set; }
        
    }

    class Player : IPlayer
    {
        public string Name { get; set; }
        public int Points { get; set; }
        public bool isReady { get; set; }
        public IGameCallBack UIdentity { get; set; }

        public Player(string _name, bool _isReady)
        {
            this.Name = _name;
            this.isReady = _isReady;
        }
    }
}
