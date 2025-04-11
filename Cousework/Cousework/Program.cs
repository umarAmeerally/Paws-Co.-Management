using Cousework.CSVHelpers;
using Cousework.DataStructures;
using Cousework.Models;
using Microsoft.EntityFrameworkCore;


namespace Cousework
{
    class Program
    {
        static void Main(string[] args)
        {
            // Path to the CSV file
            string filePath = @"C:\Users\thera\Desktop\owners.csv";

            // Create a new HashTable for owners
            var ownerTable = new HashTable<Owner>();

            // Load the owners from the CSV file
            CSVReader.LoadOwnersFromCSV(filePath, ownerTable);

            // Path to your SQL Server database connection string
            string connectionString = "Data Source=HP;Initial Catalog=Cousework;Integrated Security=True;Trust Server Certificate=True";

            // Save the owners to the database
            DatabaseHelper.SaveOwnersToDatabase(ownerTable, connectionString);

            // Verify if owners were saved by printing to console
            // Instead of using foreach, manually iterate over the HashTable
            for (int i = 0; i < ownerTable.Count(); i++)
            {
                Owner owner = ownerTable[i]; // Access owner by index
                Console.WriteLine($"OwnerId: {owner.OwnerId}, Name: {owner.Name}, Email: {owner.Email}");
            }

        }
    }

}


