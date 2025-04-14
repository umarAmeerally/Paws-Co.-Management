using System;
using System.IO;
using System.Linq;
using Cousework.Models;
using Cousework.DataStructures;

namespace Cousework.Utils
{
    public class CSVReader
    {
        public HashTable<Owner> OwnerHashTable { get; private set; }
        public HashTable<Pet> PetHashTable { get; private set; }

        public HashTable<Appointment> AppointmentHashTable { get; private set; }



        public CSVReader()
        {
            OwnerHashTable = new HashTable<Owner>();
            PetHashTable = new HashTable<Pet>();
            AppointmentHashTable = new HashTable<Appointment>();    
        }

        public void ParseCSV(string filePath)
        {
            try
            {
                var lines = File.ReadAllLines(filePath);
                var dataLines = lines.Skip(1);

                foreach (var line in dataLines)
                {
                    var parts = line.Split(',');

                    // Parse Owner
                    if (parts.Length >= 4)
                    {
                        try
                        {
                            var owner = new Owner
                            {
                                OwnerId = int.Parse(parts[0]),
                                Name = parts[1],
                                Email = parts[2],
                                Phone = parts[3]
                            };

                            OwnerHashTable.Insert(owner);
                            Console.WriteLine($"Owner {owner.Name} added to hash table.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error processing owner from line: {line}. Exception: {ex.Message}");
                        }
                    }

                    // Parse Pet
                    if (parts.Length >= 9)
                    {
                        try
                        {
                            var pet = new Pet
                            {
                                PetId = int.Parse(parts[4]),
                                Name = parts[5],
                                Species = parts[6],
                                Breed = parts[7],
                                Age = int.TryParse(parts[8], out int age) ? age : null,
                                OwnerId = int.Parse(parts[0]) 
                            };

                            PetHashTable.Insert(pet);
                            Console.WriteLine($"Pet {pet.Name} added to hash table.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error processing pet from line: {line}. Exception: {ex.Message}");
                        }
                    }

                    if (parts.Length >= 9)
                    {
                        try
                        {
                            var appointment = new Appointment
                            {
                                AppointmentId = int.Parse(parts[9]),
                                PetId = int.Parse(parts[4]),
                                AppointmentDate = DateTime.Parse(parts[10]),
                                Type = parts[11],
                                Status = parts[12]
                            };

                            AppointmentHashTable.Insert(appointment);
                            Console.WriteLine($"Appointment {appointment.AppointmentId} added to hash table.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error processing pet from line: {line}. Exception: {ex.Message}");
                        }
                    }
                }

                Console.WriteLine("CSV parsed and owners and pets loaded into HashTables.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading the CSV file: {ex.Message}");
            }
        }




    }
}
