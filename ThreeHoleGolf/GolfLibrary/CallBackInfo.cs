using System;
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

        //[DataMember]        public string User { get; private set; }
        [DataMember]
        public bool getMessages { get; private set; }

        public CallBackInfo(bool b)
        {
            getMessages = b;
        }


    }
}
