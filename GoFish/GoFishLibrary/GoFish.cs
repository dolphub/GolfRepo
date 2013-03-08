using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ServiceModel;
namespace GoFishLibrary
{

    [ServiceContract]
    public interface IMessageBoard
    {
        [OperationContract(IsOneWay = true)]
        void postMessage(string message);
        [OperationContract]
        string[] getAllMessages();
    }

    //public interface IMessageBoardContract
    //{
    //    [OperationContract(IsOneWay = true)]
    //    void Reply(string messageToPost);
    //}

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class MessageBoard : IMessageBoard
    {
        private List<string> messages = new List<string>();

        public void postMessage(string message)
        {
            Console.WriteLine("Posting a Message.");
            messages.Insert(0, message);

            //IMessageBoardContract proxy = OperationContext.Current.GetCallbackChannel<IMessageBoardContract>();
            //string response = "Service object" + this.GetHashCode().ToString() + " received: " + message;
            //Console.WriteLine("Sending back: " + response);
            //proxy.Reply(response);
        }

        public string[] getAllMessages()
        {
            return messages.ToArray<string>();
        }
    }
}
