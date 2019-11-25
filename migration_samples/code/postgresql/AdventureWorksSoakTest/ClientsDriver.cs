using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AdventureWorksSoakTest
{
    class ClientsDriver
    {
        public System.Threading.ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
        private int numClients;

        public ClientsDriver(int numClients)
        {
            this.numClients = numClients;
        }

        public void Run(CancellationToken token)
        {
            var rnd = new Random();

            for (int clientNum = 0; clientNum < this.numClients; clientNum++)
            {
                string clientName = $"Client {clientNum}";

                var client = new Client(clientName, clientNum);
                Task.Factory.StartNew(() => client.RunQueries());
            }

            while (!token.IsCancellationRequested)
            {
                // Run until the user stops the devices by pressing Enter
            }

            this.runCompleteEvent.Set();
        }

        public void WaitForEnter(CancellationTokenSource tokenSource)
        {
            Console.WriteLine("Press Enter to stop clients");
            Console.ReadLine();
            tokenSource.Cancel();
        }
    }
}