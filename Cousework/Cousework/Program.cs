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
            var context = new PetCareContext(); 
            HashTable<Owner> ownerTable;
            HashTable<Pet> petTable;
            HashTable<Appointment> appointmentTable;


            Console.WriteLine("Choose data source:");
            Console.WriteLine("1. Load data from CSV");
            Console.WriteLine("2. Load data from Database");
            Console.Write("Enter choice: ");
            var input = Console.ReadLine();

            if (input == "1")
            {
                string csvPath = "C:\\Users\\thera\\Desktop\\pet_management_data.csv";
                var csvReader = new CSVReader();
                csvReader.ParseCSV(csvPath);

                ownerTable = csvReader.OwnerHashTable;
                petTable = csvReader.PetHashTable;
                appointmentTable = csvReader.AppointmentHashTable;

                Console.WriteLine("\nCSV parsed and data loaded into HashTables.");
            }
            else
            {
                Console.WriteLine("\nLoading data from database...");
               
                (ownerTable, petTable ,appointmentTable) = DatabaseLoader.LoadData(context);

                Console.WriteLine("Data loaded from database into HashTables.");
            }

            var ownerService = new OwnerService(ownerTable);
            var petService = new PetService(context, petTable, ownerTable);
            var appointmentService = new AppointmentService(appointmentTable);





            RunMenu(ownerService, petService,appointmentService, context);
        }


        static void RunMenu(OwnerService ownerService, PetService petService, AppointmentService appointmentService, PetCareContext context)
        {
            while (true)
            {
                Console.WriteLine("\n----- Pet Management System -----");
                Console.WriteLine("1. Display all owners");
                Console.WriteLine("2. Add new owner");
                Console.WriteLine("3. Update owner");
                Console.WriteLine("4. Delete owner");
                Console.WriteLine("5. Display all pets");
                Console.WriteLine("6. Add new pet");
                Console.WriteLine("7. Update a  pet");
                Console.WriteLine("8. Delete a  pet");
                Console.WriteLine("9. Exit");
                Console.WriteLine("10. Save all data to database");
                Console.WriteLine("11. view appointments");

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
                        Console.Write("Enter Owner ID to update: ");
                        int updateId = int.Parse(Console.ReadLine());
                        var updatedOwner = CreateOwnerFromInput(ownerService, context, updateId);
                        ownerService.UpdateOwner(updateId, updatedOwner);
                        break;

                    case "4":
                        Console.Write("Enter Owner ID to delete: ");
                        if (int.TryParse(Console.ReadLine(), out int deleteId))
                        {
                            var confirmed = DatabaseHelper.DeleteOwnerAndPetsFromDatabase(deleteId, context);

                            if (confirmed)
                            {
                                // Only remove from hash tables if DB deletion is successful
                                ownerService.DeleteOwner(deleteId);
                                petService.DeletePetsByOwnerId(deleteId); // You may need to implement this method
                                Console.WriteLine("Owner and their pets deleted from hash tables.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Invalid ID.");
                        }
                        break;


                    case "5":
                        petService.DisplayPets();
                        break;

                    case "6":
                        var newPet = CreatePetFromInput(ownerService , petService , context); // Pass ownerService!
                        if (newPet != null)
                            petService.AddPet(newPet);
                        break;

                    case "7": // Example menu case for update
                        Console.WriteLine("Updating Pet");
                        petService.UpdatePet(ownerService, petService);
                        break;

                    case "8":
                        Console.Write("Enter Pet ID to delete: ");
                        if (int.TryParse(Console.ReadLine(), out int deletePetId))
                        {
                            var deleted = DatabaseHelper.DeletePetAndAppointmentsFromDatabase(deletePetId, context);

                            if (deleted)
                            {
                                // Only remove from hash tables if DB deletion is successful
                                petService.DeletePet(deletePetId);
                                appointmentService.DeleteAppointmentsByPetId(deletePetId);
                                Console.WriteLine("Pet and their appointments deleted from hash tables.");
                            }
                            else
                            {
                                Console.WriteLine("Deletion failed. Pet not found.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Invalid ID.");
                        }

                        break;





                    case "9":
                        Console.WriteLine("Exiting...");
                        return;

                    case "10":
                        Console.WriteLine("Saving owners, pets, and appointments to the database...");

                        string connectionString = "Data Source=HP;Initial Catalog=Cousework;Integrated Security=True;Trust Server Certificate=True";

                        var ownerHashTable = ownerService.GetOwnerHashTable();
                        var petHashTable = petService.GetPetHashTable();
                        var appointmentHashTable = appointmentService.GetAppointmentHashTable(); 

                        DatabaseHelper.SaveOwnersToDatabase(ownerHashTable, connectionString);
                        DatabaseHelper.SavePetsToDatabase(petHashTable, connectionString);
                        DatabaseHelper.SaveAppointmentsToDatabase(appointmentHashTable, connectionString); 

                        Console.WriteLine("All data saved successfully.");
                        break;


                    case "11":
                        appointmentService.DisplayAppointments();
                        break;

                    default:
                        Console.WriteLine("Invalid option.");
                        break;
                }
            }
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

            Console.Write("Enter Gender (Male/Female): ");
            string gender = Console.ReadLine();

            Console.Write("Enter Medical History: ");
            string medicalHistory = Console.ReadLine();

            Console.Write("Enter Date Registered (dd/MM/yyyy): ");
            DateTime.TryParseExact(Console.ReadLine(), "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime dateRegistered);

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
                Gender = gender,
                MedicalHistory = medicalHistory,
                DateRegistered = dateRegistered,
                OwnerId = ownerId
            };
        }


    }
}
