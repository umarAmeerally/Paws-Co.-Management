using Cousework.DataStructures;
using Cousework.Models;
using System.Linq;

namespace Cousework.Utils
{
    public static class DatabaseLoader
    {
        public static (HashTable<Owner>, HashTable<Pet>) LoadData(PetCareContext context, int size = 50)
        {
            var ownerTable = new HashTable<Owner>(size);
            var petTable = new HashTable<Pet>(size);

            foreach (var owner in context.Owners)
                ownerTable.Insert(owner);

            foreach (var pet in context.Pets)
                petTable.Insert(pet);

            return (ownerTable, petTable);
        }
    }
}
