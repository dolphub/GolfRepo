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
        static void Main( string[] args )
        {
            ServiceHost servHost = null;
            try
            {
                // Address
                servHost = new ServiceHost( typeof( GameSystem ) );
                // Start the service
                servHost.Open();
                // wait for a keystroke to shut down the service, 
                Console.WriteLine( "Press any key to shut the service down. " );
                Console.ReadKey();
            }
            catch( Exception ex )
            {
                Console.WriteLine( ex.Message );
            }
            finally
            {
                // Shut down the service
                Console.ReadKey();
                if( servHost != null )
                    servHost.Close();
            }
        } // end main
    } // end class
} // end namespace
