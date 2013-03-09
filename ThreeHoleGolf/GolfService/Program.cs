using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ServiceModel;
using GolfLibrary;

namespace GolfService
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // Address
                ServiceHost servHost = new ServiceHost(typeof(Shoe),
                    new Uri("net.tcp://locahost:9000/GolfLibrary/")); // this is the assembly

                // Service contract and binding
                servHost.AddServiceEndpoint(typeof(IShoe), new NetTcpBinding(),
                    "Shoe");

                // Start the service
                servHost.Open();

                // Keep the server running until <Enter> is pressed
                Console.WriteLine("Shoe service is activated. Press <Enter> to quit.");
                Console.ReadKey();

                // Shut down the service
                servHost.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey();
            }
        }
    }
}
