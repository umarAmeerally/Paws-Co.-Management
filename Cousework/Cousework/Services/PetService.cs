using Cousework.DataStructures;
using Cousework.Models;
using System;

namespace Cousework.Services
{
    public class PetService
    {
        private readonly HashTable<Pet> _petTable;
        private readonly HashTable<Owner> _ownerTable;

        public PetService(HashTable<Pet> petTable, HashTable<Owner> ownerTable)
        {
            _petTable = petTable;
            _ownerTable = ownerTable;
        }

        public void AddPet(Pet newPet)
        {
            bool ownerExists = false;

            foreach (var owner in _ownerTable.GetAllElements())
            {
                if (owner.OwnerId == newPet.OwnerId)
                {
                    ownerExists = true;
                    break;
                }
            }

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

        public bool UpdatePet(int petId, Pet updatedPet)
        {
            foreach (var pet in _petTable.GetAllElements())
            {
                if (pet.PetId == petId)
                {
                    pet.Name = updatedPet.Name;
                    pet.Species = updatedPet.Species;
                    pet.Breed = updatedPet.Breed;
                    pet.Age = updatedPet.Age;
                    pet.Gender = updatedPet.Gender;
                    pet.MedicalHistory = updatedPet.MedicalHistory;
                    Console.WriteLine("Pet updated successfully.");
                    return true;
                }
            }

            Console.WriteLine("Pet not found.");
            return false;
        }

        public bool DeletePet(int petId)
        {
            for (int i = 0; i < _petTable.Count(); i++)
            {
                var pet = _petTable[i];
                if (pet != null && pet.PetId == petId)
                {
                    _petTable[i] = default;
                    Console.WriteLine("Pet deleted successfully.");
                    return true;
                }
            }

            Console.WriteLine("Pet not found.");
            return false;
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


        public HashTable<Pet> GetPetHashTable() => _petTable;

    }
}
