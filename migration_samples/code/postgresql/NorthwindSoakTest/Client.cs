using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace NorthwindSoakTest
{
    public class Client
    {
        private readonly string clientName;
        private static readonly string connectionString = ConfigurationManager.AppSettings["ConnectionString"];
        private NpgsqlConnection conn;

        public Client(string clientName)
        {
            this.clientName = clientName;
            conn = new NpgsqlConnection(connectionString);
        }

        internal void RunQueries()
        {
            Random rnd = new Random();

            while (true)
            {
                try
                {
                    conn.Open();

                    displayResults("SELECT category_id, category_name, description FROM categories", conn);
                    displayResults("SELECT * FROM employees", conn);
                    displayResults("SELECT COUNT(*) FROM orders", conn);
                    displayResults("SELECT * FROM products", conn);
                    displayResults("SELECT COUNT(*) FROM customers", conn);

                    conn.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Client {clientName} failed with error: {e.Message}");
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        conn.Close();
                    }
                }
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
