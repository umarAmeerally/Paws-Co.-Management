using Cousework.DataStructures;
using Cousework.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Cousework.Services
{
    public class PetService
    {
        private readonly HashTable<Pet> _petTable;
        private readonly HashTable<Owner> _ownerTable;
        private readonly PetCareContext _context;

        public PetService(PetCareContext context, HashTable<Pet> petTable, HashTable<Owner> ownerTable)
        {
            _context = context;
            _petTable = petTable;
            _ownerTable = ownerTable;
        }

        public void AddPet(Pet newPet)
        {
            bool ownerExists = _ownerTable.GetAllElements().Any(o => o.OwnerId == newPet.OwnerId);

            if (!ownerExists)
            {
                Console.WriteLine($"Owner with ID {newPet.OwnerId} does not exist. Cannot add pet.");
                return;
            }

            _petTable.Insert(newPet);
            Console.WriteLine("Pet added successfully.");
        }

        public void DisplayPets()
        {
            Console.WriteLine("Displaying pets from HashTable:");
            _petTable.DisplayContents();
        }


        public static Pet PromptForPetByOwner(OwnerService ownerService, PetService petService)
        {
            // Prompt user for the owner's name or email
            Console.Write("Enter part of the owner's name or email: ");
            string search = Console.ReadLine();

            // Find matching owners
            var matchingOwners = ownerService
                .GetOwnerHashTable()
                .GetAllElements()
                .Where(o => o.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                            o.Email.Contains(search, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (matchingOwners.Count == 0)
            {
                Console.WriteLine("No matching owners found.");
                return null;
            }

            // Display matching owners
            Console.WriteLine("\nMatching Owners:");
            foreach (var owner in matchingOwners)
            {
                Console.WriteLine($"ID: {owner.OwnerId}, Name: {owner.Name}, Email: {owner.Email}");
            }

            // Let the user select the owner from the matching list
            Console.Write("\nEnter the Owner ID from the above list: ");
            if (int.TryParse(Console.ReadLine(), out int ownerId))
            {
                // Find all pets for this owner
                var petsOwnedByOwner = petService
                    .GetPetHashTable()
                    .GetAllElements()
                    .Where(p => p.OwnerId == ownerId)
                    .ToList();

                if (petsOwnedByOwner.Count == 0)
                {
                    Console.WriteLine("This owner does not have any pets.");
                    return null;
                }

                // Display pets for the selected owner
                Console.WriteLine("\nMatching Pets:");
                foreach (var pet in petsOwnedByOwner)
                {
                    Console.WriteLine($"Pet ID: {pet.PetId}, Name: {pet.Name}, Breed: {pet.Breed}");
                }

                // Let the user select the pet
                Console.Write("\nEnter the Pet ID to update: ");
                if (int.TryParse(Console.ReadLine(), out int petId))
                {
                    var petToUpdate = petsOwnedByOwner.FirstOrDefault(p => p.PetId == petId);
                    return petToUpdate;
                }
            }

            return null;
        }


        public bool UpdatePet(OwnerService ownerService, PetService petService)
        {
            // Prompt user to select the pet by owner's name
            var petToUpdate = PromptForPetByOwner(ownerService, petService);

            if (petToUpdate == null)
            {
                Console.WriteLine("Pet not found or operation cancelled.");
                return false;
            }

            // Now update the pet details
            Console.WriteLine("Updating pet details. Press Enter to keep the current value.\n");

            Console.Write($"Enter Pet Name ({petToUpdate.Name}): ");
            string name = Console.ReadLine();
            petToUpdate.Name = string.IsNullOrWhiteSpace(name) ? petToUpdate.Name : name;

            Console.Write($"Enter Species ({petToUpdate.Species}): ");
            string species = Console.ReadLine();
            petToUpdate.Species = string.IsNullOrWhiteSpace(species) ? petToUpdate.Species : species;

            Console.Write($"Enter Breed ({petToUpdate.Breed}): ");
            string breed = Console.ReadLine();
            petToUpdate.Breed = string.IsNullOrWhiteSpace(breed) ? petToUpdate.Breed : breed;

            Console.Write($"Enter Age ({petToUpdate.Age}): ");
            string ageInput = Console.ReadLine();
            if (int.TryParse(ageInput, out int newAge))
                petToUpdate.Age = newAge;

            Console.Write($"Enter Gender ({petToUpdate.Gender}): ");
            string gender = Console.ReadLine();
            petToUpdate.Gender = string.IsNullOrWhiteSpace(gender) ? petToUpdate.Gender : gender;

            Console.Write($"Enter Medical History ({petToUpdate.MedicalHistory}): ");
            string history = Console.ReadLine();
            petToUpdate.MedicalHistory = string.IsNullOrWhiteSpace(history) ? petToUpdate.MedicalHistory : history;

            Console.Write($"Enter Date Registered ({petToUpdate.DateRegistered:dd/MM/yyyy}): ");
            string dateInput = Console.ReadLine();
            if (DateTime.TryParseExact(dateInput, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime newDate))
                petToUpdate.DateRegistered = newDate;

            Console.WriteLine($"Pet with ID {petToUpdate.PetId} updated successfully.");
            return true;
        }





        public bool DeletePet(int petId)
        {
            bool isDeleted = _petTable.DeletePetByPetId(petId);
            if (isDeleted)
            {
                Console.WriteLine($"Pet with PetId {petId} successfully deleted.");
            }
            else
            {
                Console.WriteLine($"No pet found with PetId {petId}.");
            }
            return isDeleted;
        }


        public void DeletePetsByOwnerId(int ownerId)
        {
            var petsToDelete = _petTable.GetAllElements()
                                        .Where(p => p.OwnerId == ownerId)
                                        .Select(p => p.PetId)
                                        .ToList();

            foreach (var petId in petsToDelete)
            {
                _petTable.DeleteByKey(petId);
            }
        }


        public int GenerateTrulyUniquePetId(PetCareContext context)
        {
            Random rand = new Random();
            int newId;
            bool exists;

            do
            {
                newId = rand.Next(0, 9999);

                bool inHash = _petTable.GetAllElements().Any(p => p.PetId == newId);
                bool inDb = context.Pets.Any(p => p.PetId == newId);

                exists = inHash || inDb;

            } while (exists);

            return newId;
        }

        public HashTable<Pet> GetPetHashTable() => _petTable;
    

    

    }

}