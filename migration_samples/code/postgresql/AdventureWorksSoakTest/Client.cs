using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Text;

namespace AdventureWorksSoakTest
{
    class Client
    {
        private readonly string clientName;
        private static readonly int numReplicas = int.Parse(ConfigurationManager.AppSettings["NumReplicas"]);
        private NpgsqlConnection conn;

        public Client(string clientName, int clientNum)
        {
            this.clientName = clientName;
            string connectionString = ConfigurationManager.AppSettings[$"ConnectionString{clientNum % numReplicas}"];
            conn = new NpgsqlConnection(connectionString);
        }

        internal void RunQueries()
        {
            Random rnd = new Random();
            conn.Open();
            while (true)
            {
                try
                {
                    //conn.Open();

                    displayResults("SELECT * FROM production.vproductanddescription", conn);
                    displayResults("SELECT * FROM purchasing.vendor", conn);
                    displayResults("SELECT * FROM sales.specialoffer", conn);
                    displayResults("SELECT * FROM sales.salesorderheader", conn);
                    displayResults("SELECT * FROM sales.salesorderdetail", conn);
                    displayResults("SELECT * FROM person.person", conn);

                    //conn.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Client {clientName} failed with error: {e.Message}");
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        //conn.Close();
                    }
                }
            }
        }

        private void displayResults(string queryString, NpgsqlConnection conn)
        {
            Console.WriteLine($"{this.clientName} : {queryString}");
            NpgsqlCommand command = new NpgsqlCommand(queryString, conn);
            Stopwatch timer = new Stopwatch();
            timer.Start();
            using (var results = command.ExecuteReader())
            {
                int n = 0;
                while (results.Read())
                {
                    n++;
                    StringBuilder resultString = new StringBuilder();
                    for (int fieldNum = 0; fieldNum < results.FieldCount; fieldNum++)
                    {
                        resultString.Append($"{results[fieldNum]} ");
                    }
                    //if (n < 10)
                    if (n < 0)
                    {
                        Console.WriteLine($"{this.clientName} : {resultString.ToString().Substring(1, 40)}...");
                    }
                }
            }
            timer.Stop();
            Console.WriteLine($"Response time: {timer.ElapsedMilliseconds} ms\n");
        }
    }
}
