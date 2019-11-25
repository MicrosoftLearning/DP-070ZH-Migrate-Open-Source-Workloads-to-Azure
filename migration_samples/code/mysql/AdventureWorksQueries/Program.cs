using System;
using MySql.Data;
using MySql.Data.MySqlClient;
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
                MySqlConnection conn = new MySqlConnection(connectionString);
                conn.Open();

                displayResults("SELECT COUNT(*) FROM product", conn);
                displayResults("SELECT COUNT(*) FROM vendor", conn);
                displayResults("SELECT COUNT(*) FROM specialoffer", conn);
                displayResults("SELECT COUNT(*) FROM salesorderheader", conn);
                displayResults("SELECT COUNT(*) FROM salesorderdetail", conn);
                displayResults("SELECT COUNT(*) FROM customer", conn);

                conn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
        }

        private static void displayResults(string queryString, MySqlConnection conn)
        {
            Console.WriteLine(queryString);
            MySqlCommand command = new MySqlCommand(queryString, conn);
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
