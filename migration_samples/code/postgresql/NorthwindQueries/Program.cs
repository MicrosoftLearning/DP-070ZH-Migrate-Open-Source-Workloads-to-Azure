using System;
using Npgsql;
using System.Configuration;
using System.Text;

namespace NorthwindQueries
{
    class Program
    {
        private static readonly string connectionString = ConfigurationManager.AppSettings["ConnectionString"];
        static void Main(string[] args)
        {
            Console.WriteLine("Querying Northwind database");

            try
            {
                NpgsqlConnection conn = new NpgsqlConnection(connectionString);
                conn.Open();

                displayResults("SELECT category_id, category_name, description FROM categories", conn);
                displayResults("SELECT * FROM employees", conn);
                displayResults("SELECT COUNT(*) FROM orders", conn);
                displayResults("SELECT * FROM products", conn);
                displayResults("SELECT COUNT(*) FROM customers", conn);

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
