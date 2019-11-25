using System;
using Npgsql;
using System.Configuration;
using System.Text;

namespace AdventureWorksQueries
{
    class Program
    {
        private static readonly string connectionString = ConfigurationManager.AppSettings["ConnectionString"];
        static void Main(string[] args)
        {
            Console.WriteLine("Querying AdventureWorks database");

            try
            {
                NpgsqlConnection conn = new NpgsqlConnection(connectionString);
                conn.Open();

                displayResults("SELECT COUNT(*) FROM production.vproductanddescription", conn);
                displayResults("SELECT COUNT(*) FROM purchasing.vendor", conn);
                displayResults("SELECT COUNT(*) FROM sales.specialoffer", conn);
                displayResults("SELECT COUNT(*) FROM sales.salesorderheader", conn);
                displayResults("SELECT COUNT(*) FROM sales.salesorderdetail", conn);
                displayResults("SELECT COUNT(*) FROM person.person", conn);
                conn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
        }

        private static void displayResults(string queryString, NpgsqlConnection conn)
        {
            Console.WriteLine(queryString);
            NpgsqlCommand command = new NpgsqlCommand(queryString, conn);
            using (var results = command.ExecuteReader())
            {
                while (results.Read())
                {
                    StringBuilder resultString = new StringBuilder();
                    for (int fieldNum = 0; fieldNum < results.FieldCount; fieldNum++)
                    {
                        resultString.Append($"{results[fieldNum]} ");
                    }
                    Console.WriteLine(resultString);
                }
            }
            Console.WriteLine();
        }
    }
}
