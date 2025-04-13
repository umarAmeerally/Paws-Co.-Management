using Cousework.DataStructures;
using Cousework.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Cousework
{
    public static class DatabaseHelper
    {
        public static void SaveOwnersToDatabase(HashTable<Owner> ownerTable, string connectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<PetCareContext>();
            optionsBuilder.UseSqlServer(connectionString);

            using (var context = new PetCareContext(optionsBuilder.Options))
            {
                foreach (var owner in ownerTable.GetAllElements())
                {
                    Console.WriteLine($"Processing owner: {owner.OwnerId}, {owner.Name}, {owner.Email}");

                    var existingOwner = context.Owners.FirstOrDefault(o => o.OwnerId == owner.OwnerId);

                    if (existingOwner == null)
                    {
                        context.Owners.Add(owner);
                        Console.WriteLine("Adding new owner.");
                    }
                    else
                    {
                        existingOwner.Name = owner.Name;
                        existingOwner.Email = owner.Email;
                        existingOwner.Phone = owner.Phone;
                        existingOwner.Address = owner.Address;
                        Console.WriteLine("Updating existing owner.");
                    }
                }

                context.SaveChanges();
                Console.WriteLine("Owners successfully saved to the database.");
            }
        }

        public static void SavePetsToDatabase(HashTable<Pet> petTable, string connectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<PetCareContext>();
            optionsBuilder.UseSqlServer(connectionString);

            using (var context = new PetCareContext(optionsBuilder.Options))
            {
                foreach (var pet in petTable.GetAllElements())
                {
                    Console.WriteLine($"Processing pet: {pet.PetId}, {pet.Name}, OwnerId: {pet.OwnerId}");

                    // Only add if the owner exists
                    var ownerExists = context.Owners.Any(o => o.OwnerId == pet.OwnerId);
                    if (!ownerExists)
                    {
                        Console.WriteLine($"Skipping pet {pet.Name} as OwnerId {pet.OwnerId} does not exist in DB.");
                        continue;
                    }

                    var existingPet = context.Pets.FirstOrDefault(p => p.PetId == pet.PetId);

                    if (existingPet == null)
                    {
                        context.Pets.Add(pet);
                        Console.WriteLine("Adding new pet.");
                    }
                    else
                    {
                        existingPet.Name = pet.Name;
                        existingPet.Species = pet.Species;
                        existingPet.Breed = pet.Breed;
                        existingPet.Age = pet.Age;
                        existingPet.OwnerId = pet.OwnerId;
                        Console.WriteLine("Updating existing pet.");
                    }
                }

                context.SaveChanges();
                Console.WriteLine("Pets successfully saved to the database.");
            }
        }


    }
}
