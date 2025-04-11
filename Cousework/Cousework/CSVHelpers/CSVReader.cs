using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cousework.DataStructures;
using Cousework.Models;

namespace Cousework.CSVHelpers
{
    public static class CSVReader
    {
        // This method will load the owner data from the CSV file into the provided hash table
        public static void LoadOwnersFromCSV(string filePath, HashTable<Owner> ownerTable)
        {
            try
            {
                using (var reader = new StreamReader(filePath))
                {
                    // Skip the header line
                    reader.ReadLine();

                    // Read each line in the CSV
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var values = line.Split(',');  // Split by comma

                        // Assuming the CSV columns: OwnerId, Name, Email, Phone, Address
                        var owner = new Owner
                        {
                            Name = values[1],
                            Email = values[2],
                            Phone = values[3],
                            Address = values[4]
                        };


                        // Insert the created owner into the hash table
                        ownerTable.Insert(owner);
                    }
                }

                Console.WriteLine("Owners loaded successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading CSV file: {ex.Message}");
            }
        }
    }
}
