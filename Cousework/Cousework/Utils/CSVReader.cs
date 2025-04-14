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
                var dataLines = lines.Skip(1); // Skip header

                foreach (var line in dataLines)
                {
                    var parts = line.Split(',');

                    // Parse Owner
                    if (parts.Length >= 5)
                    {
                        try
                        {
                            var owner = new Owner
                            {
                                OwnerId = int.Parse(parts[0]),
                                Name = parts[1],
                                Email = parts[2],
                                Phone = parts[3],
                                Address = parts[4] // New Address field
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
                    if (parts.Length >= 12) // Updated to include all pet details
                    {
                        try
                        {
                            var pet = new Pet
                            {
                                PetId = int.Parse(parts[5]),
                                Name = parts[6],
                                Species = parts[7],
                                Breed = parts[8],
                                Age = int.TryParse(parts[9], out int age) ? age : 0, // Default to 0 if parsing fails
                                Gender = parts[10], // New Gender field
                                MedicalHistory = parts[11], // New MedicalHistory field
                                DateRegistered = DateTime.TryParse(parts[12], out DateTime regDate) ? regDate : DateTime.MinValue, // New DateRegistered field
                                OwnerId = int.Parse(parts[0]) // Link pet to owner
                            };

                            PetHashTable.Insert(pet);
                            Console.WriteLine($"Pet {pet.Name} added to hash table.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error processing pet from line: {line}. Exception: {ex.Message}");
                        }
                    }

                    // Parse Appointment
                    if (parts.Length >= 13) // Updated to include all appointment details
                    {
                        try
                        {
                            var appointment = new Appointment
                            {
                                AppointmentId = int.Parse(parts[13]),
                                PetId = int.Parse(parts[5]),
                                AppointmentDate = DateTime.TryParse(parts[14], out DateTime appDate) ? appDate : DateTime.MinValue, // Safely parse appointment date
                                Type = parts[15],
                                Status = parts[16],
                                OwnerId = int.Parse(parts[0])
                            };

                            AppointmentHashTable.Insert(appointment);
                            Console.WriteLine($"Appointment {appointment.AppointmentId} added to hash table.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error processing appointment from line: {line}. Exception: {ex.Message}");
                        }
                    }
                }

                Console.WriteLine("CSV parsed and owners, pets, and appointments loaded into HashTables.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading the CSV file: {ex.Message}");
            }
        }
    }
}

