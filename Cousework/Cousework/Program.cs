using Cousework.DataStructures;
using Cousework.Models;
using Cousework.Services;
using Cousework.Utils;
using System;
using System.Linq;

namespace Cousework
{
    class Program
    {
        static void Main(string[] args)
        {
            var context = new PetCareContext(); // EF DB context
            HashTable<Owner> ownerTable;
            HashTable<Pet> petTable;

            DisplayHeader("Welcome to the Pet Management System");

            Console.WriteLine("Choose data source:");
            Console.WriteLine("1. Load data from CSV");
            Console.WriteLine("2. Load data from Database");
            Console.Write("Enter choice: ");
            var input = Console.ReadLine();

            if (input == "1")
            {
                string csvPath = @"C:\Users\mehtaab\OneDrive\Desktop\CST 2550\owners.csv"; // Update with actual file path
                var csvReader = new CSVReader();
                csvReader.ParseCSV(csvPath);
                ownerTable = csvReader.OwnerHashTable;
                petTable = csvReader.PetHashTable;

                DisplayMessage("\nCSV parsed and data loaded into HashTables.");
                string connectionString = @"Data Source=MEHTAAB\\MSSQLSERVER02;Initial Catalog=PetManagementdb;Integrated Security=True;TrustServerCertificate=True";
                DatabaseHelper.SaveOwnersToDatabase(ownerTable, connectionString);
            }
            else
            {
                Console.WriteLine("\nLoading data from database...");
                (ownerTable, petTable) = DatabaseLoader.LoadData(context);
                DisplayMessage("Data loaded from database into HashTables.");
            }

            var ownerService = new OwnerService(ownerTable);
            var petService = new PetService(petTable, ownerTable);

            RunMenu(ownerService, petService, context);
        }

        static void DisplayHeader(string header)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n===========================================");
            Console.WriteLine($"* {header} *");
            Console.WriteLine("===========================================");
            Console.ResetColor();
        }

        static void DisplayMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[INFO] {message}");
            Console.ResetColor();
        }

        static void RunMenu(OwnerService ownerService, PetService petService, PetCareContext context)
        {
            while (true)
            {
                DisplayHeader("Pet Management System");
                Console.WriteLine("1. Display all owners");
                Console.WriteLine("2. Add new owner");
                Console.WriteLine("3. Update owner");
                Console.WriteLine("4. Delete owner");
                Console.WriteLine("5. Display all pets");
                Console.WriteLine("6. Add new pet");
                Console.WriteLine("7. Update a pet");
                Console.WriteLine("8. Exit");
                Console.WriteLine("9. Save all data to database");
                Console.Write("Choose an option: ");

                string choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        ownerService.DisplayOwners();
                        break;

                    case "2":
                        var newOwner = CreateOwnerFromInput(ownerService, context);
                        ownerService.AddOwner(newOwner);
                        break;

                    case "3":
                        UpdateOwner(ownerService, context);
                        break;

                    case "4":
                        DeleteOwner(ownerService, petService, context);
                        break;

                    case "5":
                        petService.DisplayPets();
                        break;

                    case "6":
                        var newPet = CreatePetFromInput(ownerService, petService, context);
                        if (newPet != null)
                            petService.AddPet(newPet);
                        break;

                    case "7":
                        UpdatePet(petService);
                        break;

                    case "8":
                        Console.WriteLine("Exiting...");
                        return;

                    case "9":
                        SaveDataToDatabase(ownerService, petService, context);
                        break;

                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
            }
        }

        static void UpdateOwner(OwnerService ownerService, PetCareContext context)
        {
            Console.Write("Enter Owner ID to update: ");
            if (int.TryParse(Console.ReadLine(), out int updateId))
            {
                var updatedOwner = CreateOwnerFromInput(ownerService, context, updateId);
                ownerService.UpdateOwner(updateId, updatedOwner);
            }
            else
            {
                Console.WriteLine("Invalid Owner ID.");
            }
        }

        static void DeleteOwner(OwnerService ownerService, PetService petService, PetCareContext context)
        {
            Console.Write("Enter Owner ID to delete: ");
            if (int.TryParse(Console.ReadLine(), out int deleteId))
            {
                var confirmed = DatabaseHelper.DeleteOwnerAndPetsFromDatabase(deleteId, context);
                if (confirmed)
                {
                    ownerService.DeleteOwner(deleteId);
                    petService.DeletePetsByOwnerId(deleteId);
                    DisplayMessage("Owner and their pets deleted from hash tables.");
                }
            }
            else
            {
                Console.WriteLine("Invalid Owner ID.");
            }
        }

        static void UpdatePet(PetService petService)
        {
            Console.Write("Enter Pet ID to update: ");
            if (int.TryParse(Console.ReadLine(), out int petIdToUpdate))
            {
                var updatedPet = CreateUpdatedPetFromInput(petService, petIdToUpdate);
                if (updatedPet != null)
                {
                    petService.UpdatePet(petIdToUpdate, updatedPet);
                }
            }
            else
            {
                Console.WriteLine("Invalid Pet ID.");
            }
        }

        static void SaveDataToDatabase(OwnerService ownerService, PetService petService, PetCareContext context)
        {
            Console.WriteLine("Saving data to the database...");
            string connectionString = "Data Source=MEHTAAB\\MSSQLSERVER02;Initial Catalog=PetManagementdb;Integrated Security=True;TrustServerCertificate=True";
            var ownerHashTable = ownerService.GetOwnerHashTable();
            var petHashTable = petService.GetPetHashTable();

            DatabaseHelper.SaveOwnersToDatabase(ownerHashTable, connectionString);
            DatabaseHelper.SavePetsToDatabase(petHashTable, connectionString);

            DisplayMessage("All data saved successfully.");
        }

        static Owner CreateOwnerFromInput(OwnerService ownerService, PetCareContext context, int? idOverride = null)
        {
            Console.Write("Enter Owner Name: ");
            string name = Console.ReadLine();

            Console.Write("Enter Owner Email: ");
            string email = Console.ReadLine();

            Console.Write("Enter Owner Phone: ");
            string phone = Console.ReadLine();

            Console.Write("Enter Owner Address: ");
            string address = Console.ReadLine();

            int ownerId = idOverride ?? ownerService.GenerateTrulyUniqueOwnerId(context);

            return new Owner
            {
                OwnerId = ownerId,
                Name = name,
                Email = email,
                Phone = phone,
                Address = address
            };
        }

        static Pet CreatePetFromInput(OwnerService ownerService, PetService petService, PetCareContext context, int? existingPetId = null)
        {
            Console.Write("Enter Pet Name: ");
            string name = Console.ReadLine();

            Console.Write("Enter Species: ");
            string species = Console.ReadLine();

            Console.Write("Enter Breed: ");
            string breed = Console.ReadLine();

            Console.Write("Enter Age: ");
            int.TryParse(Console.ReadLine(), out int age);

            // --- Owner selection helper ---
            Console.Write("Enter part of the owner's name or email: ");
            string search = Console.ReadLine();

            var matches = ownerService
                .GetOwnerHashTable()
                .GetAllElements()
                .Where(o => o.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                            o.Email.Contains(search, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (matches.Count == 0)
            {
                Console.WriteLine("No matching owners found. Pet creation aborted.");
                return null;
            }

            Console.WriteLine("\nMatching Owners:");
            foreach (var owner in matches)
            {
                Console.WriteLine($"ID: {owner.OwnerId}, Name: {owner.Name}, Email: {owner.Email}");
            }

            Console.Write("\nEnter the Owner ID from the above list: ");
            int.TryParse(Console.ReadLine(), out int ownerId);

            // --- Create and return Pet object ---

            return new Pet
            {
                PetId = petService.GenerateTrulyUniquePetId(context),

                Name = name,
                Species = species,
                Breed = breed,
                Age = age,
                OwnerId = ownerId
            };
        }

        static Pet CreateUpdatedPetFromInput(PetService petService, int petId)
        {
            var existingPet = petService.GetPetHashTable().GetAllElements()
                                        .FirstOrDefault(p => p.PetId == petId);

            if (existingPet == null)
            {
                Console.WriteLine("Pet not found.");
                return null;
            }

            Console.WriteLine($"Updating Pet: {existingPet.Name} (ID: {existingPet.PetId})");

            Console.Write($"Enter Pet Name ({existingPet.Name}): ");
            string name = Console.ReadLine();
            name = string.IsNullOrWhiteSpace(name) ? existingPet.Name : name;

            Console.Write($"Enter Species ({existingPet.Species}): ");
            string species = Console.ReadLine();
            species = string.IsNullOrWhiteSpace(species) ? existingPet.Species : species;

            Console.Write($"Enter Breed ({existingPet.Breed}): ");
            string breed = Console.ReadLine();
            breed = string.IsNullOrWhiteSpace(breed) ? existingPet.Breed : breed;

            Console.Write($"Enter Age ({existingPet.Age}): ");
            string ageInput = Console.ReadLine();
            int age = string.IsNullOrWhiteSpace(ageInput) ? existingPet.Age ?? 0 : int.Parse(ageInput);

            Console.Write($"Enter Gender ({existingPet.Gender}): ");
            string gender = Console.ReadLine();
            gender = string.IsNullOrWhiteSpace(gender) ? existingPet.Gender : gender;

            Console.Write($"Enter Medical History ({existingPet.MedicalHistory}): ");
            string history = Console.ReadLine();
            history = string.IsNullOrWhiteSpace(history) ? existingPet.MedicalHistory : history;

            return new Pet
            {
                PetId = existingPet.PetId,
                OwnerId = existingPet.OwnerId,
                Name = name,
                Species = species,
                Breed = breed,
                Age = age,
                Gender = gender,
                MedicalHistory = history
            };
        }
    }
}

