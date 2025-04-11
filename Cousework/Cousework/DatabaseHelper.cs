using Cousework.DataStructures;
using Cousework.Models;
using Microsoft.EntityFrameworkCore;

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


    }
}
