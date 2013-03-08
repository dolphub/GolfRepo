using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ServiceModel;
using GoFishLibrary;


namespace GoFishService
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceHost servHost = null;
            try
            {
                // Address 
                servHost = new ServiceHost(typeof(MessageBoard),
                new Uri("net.tcp://localhost:12000/MessageBoardLibrary/"));
                // Service contract and binding
                servHost.AddServiceEndpoint(typeof(IMessageBoard), new
                NetTcpBinding(), "MessageBoard");
                // Manage the service’s life cycle
                servHost.Open();
                Console.WriteLine("Service started. Press a key to quit.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Console.ReadKey();
                if (servHost != null)
                    servHost.Close();
            }
        }
    }
}
