using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace AdventureWorksSoakTest
{
    class Program
    {
        private static CancellationTokenSource tokenSource = new CancellationTokenSource();

        static void Main(string[] args)
        {
            int numClients = int.Parse(ConfigurationManager.AppSettings["NumClients"]);

            try
            {
                // Start running queries
                var driver = new ClientsDriver(numClients);
                var token = tokenSource.Token;
                Task.Factory.StartNew(() => driver.Run(token));
                Task.Factory.StartNew(() => driver.WaitForEnter(tokenSource));
                driver.runCompleteEvent.WaitOne();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Application failed with error: {e.Message}");
            }
        }
    }
}
