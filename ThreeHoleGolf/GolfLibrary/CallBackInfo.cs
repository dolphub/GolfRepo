using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.Runtime.Serialization; 

namespace GolfLibrary
{
    [DataContract]
    public class CallBackInfo
    {

        public string DrawnCard { get; private set; }
        public string DiscardedCard { get; private set; }

        public CallBackInfo(string _drawn, string _discarded)
        {
            DrawnCard = _drawn;
            DiscardedCard = _discarded;
        }


    }
}
