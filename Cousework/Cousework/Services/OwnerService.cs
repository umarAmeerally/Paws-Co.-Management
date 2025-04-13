using Cousework.DataStructures;
using Cousework.Models;
using System;

namespace Cousework.Services
{
    public class OwnerService
    {
        private readonly HashTable<Owner> _ownerTable;

        public OwnerService(HashTable<Owner> ownerTable)
        {
            _ownerTable = ownerTable;
        }

        // Method to add a new owner
        public void AddOwner(Owner newOwner)
        {
            _ownerTable.Insert(newOwner);
            Console.WriteLine("Owner added successfully.");
        }

        // Method to display owners
        public void DisplayOwners()
        {
            Console.WriteLine("Displaying owners from HashTable:");
            _ownerTable.DisplayContents();
        }

        public bool UpdateOwner(int ownerId, Owner updatedOwner)
        {
            foreach (var owner in _ownerTable.GetAllElements())
            {
                if (owner.OwnerId == ownerId)
                {
                    owner.Name = updatedOwner.Name;
                    owner.Email = updatedOwner.Email;
                    owner.Phone = updatedOwner.Phone;
                    owner.Address = updatedOwner.Address;
                    Console.WriteLine("Owner updated successfully.");
                    return true;
                }
            }

            Console.WriteLine("Owner not found.");
            return false;
        }

        public bool DeleteOwner(int ownerId)
        {
            for (int i = 0; i < _ownerTable.Count(); i++)
            {
                var owner = _ownerTable[i];
                if (owner != null && owner.OwnerId == ownerId)
                {
                    _ownerTable[i] = default; // Remove the element
                    Console.WriteLine("Owner deleted successfully.");
                    return true;
                }
            }

            Console.WriteLine("Owner not found.");
            return false;
        }

        public int GenerateTrulyUniqueOwnerId(PetCareContext context)
        {
            Random rand = new Random();
            int newId;
            bool exists;

            do
            {
                newId = rand.Next(0, 9999);

                // Check in both hash table and database
                bool inHash = _ownerTable.GetAllElements().Any(o => o.OwnerId == newId);
                bool inDb = context.Owners.Any(o => o.OwnerId == newId);

                exists = inHash || inDb;

            } while (exists);

            return newId;
        }

   

        public HashTable<Owner> GetOwnerHashTable() => _ownerTable;

    }
}
