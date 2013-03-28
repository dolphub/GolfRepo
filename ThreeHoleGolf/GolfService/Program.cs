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
            ServiceHost servHost = null;
            try
            {
                // Address
                servHost = new ServiceHost(typeof(GameSystem));

                // Start the service
                servHost.Open();

                // Keep the server running until <Enter> is pressed
                Console.WriteLine("Shoe service is activated. Press <Enter> to quit.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                // Shut down the service
                Console.ReadKey();
                if (servHost != null)
                    servHost.Close();
            }
        }
    }
}
