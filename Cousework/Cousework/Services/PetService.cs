using Cousework.DataStructures;
using Cousework.Models;
using Microsoft.EntityFrameworkCore;
using Spectre.Console;
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
            string search = AnsiConsole.Ask<string>("Enter part of the owner's [blue]name or email[/]:");

            // Find matching owners
            var matchingOwners = ownerService
                .GetOwnerHashTable()
                .GetAllElements()
                .Where(o => o.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                            o.Email.Contains(search, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!matchingOwners.Any())
            {
                Warn("No owner matches.");
                return null;
            }

            // Let the user select the owner from the matching list using a selection prompt
            var selectedOwner = AnsiConsole.Prompt(
                new SelectionPrompt<Owner>()
                    .Title("[blue]Select Owner[/]")
                    .UseConverter(owner => Markup.Escape($"[{owner.OwnerId}] {owner.Name} ({owner.Email})"))
                    .AddChoices(matchingOwners)
            );

            // Find all pets for the selected owner
            var petsOwnedByOwner = petService
                .GetPetHashTable()
                .GetAllElements()
                .Where(p => p.OwnerId == selectedOwner.OwnerId)
                .ToList();

            if (!petsOwnedByOwner.Any())
            {
                AnsiConsole.MarkupLine("[red]This owner does not have any pets.[/]");
                return null;
            }

            // Let the user select the pet using a selection prompt
            var selectedPet = AnsiConsole.Prompt(
                new SelectionPrompt<Pet>()
                    .Title("[blue]Select Pet to Update[/]")
                    .UseConverter(pet => Markup.Escape($"[{pet.PetId}] {pet.Name} ({pet.Breed})"))
                    .AddChoices(petsOwnedByOwner)
            );

            return selectedPet;
        }



        public bool UpdatePet(OwnerService ownerService, PetService petService)
        {
            // Step 1: Prompt user to select the pet by owner's name or email
            var petToUpdate = PromptForPetByOwner(ownerService, petService);

            if (petToUpdate == null)
            {
                Console.WriteLine("Pet not found or operation cancelled.");
                return false;
            }

            // Step 2: Updating pet details with prompts
            Console.WriteLine("Updating pet details. Press Enter to keep the current value.\n");

            // Pet Name (with prompt and validation)
            string name = PromptValidNameUpdate("Pet Name", petToUpdate.Name);
            petToUpdate.Name = name;

            // Species
            string species = AnsiConsole.Ask<string>($"Species [grey](press Enter to keep current: {petToUpdate.Species})[/]", petToUpdate.Species);
            petToUpdate.Species = string.IsNullOrWhiteSpace(species) ? petToUpdate.Species : species;

            // Breed
            string breed = AnsiConsole.Ask<string>($"Breed [grey](press Enter to keep current: {petToUpdate.Breed})[/]", petToUpdate.Breed);
            petToUpdate.Breed = string.IsNullOrWhiteSpace(breed) ? petToUpdate.Breed : breed;

            // Age (with validation and Enter-to-keep)
            petToUpdate.Age = PromptValidIntUpdate("Age", petToUpdate.Age ?? 0);

            // Gender (selection prompt)
            string gender = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"Gender ({petToUpdate.Gender})")
                    .AddChoices("Male", "Female")
            );
            petToUpdate.Gender = gender;

            // Medical History (multi-selection with default)
            var historyChoices = AnsiConsole.Prompt(
                new MultiSelectionPrompt<string>()
                    .Title($"Medical History ({petToUpdate.MedicalHistory})")
                    .NotRequired()
                    .InstructionsText("[violet](Press space to select, enter to confirm)[/]")
                    .AddChoices("Vaccinated", "Has Allergies", "Has Chronic Illness", "Recently Treated", "None")
            );

            string medicalHistory = historyChoices.Any() ? string.Join(", ", historyChoices) : "None"; // Default to "None" if nothing selected
            petToUpdate.MedicalHistory = medicalHistory;

            // Date Registered (Auto-assigned to current date, no change needed)
            DateTime dateRegistered = DateTime.Now;
            petToUpdate.DateRegistered = dateRegistered;

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
                newId = rand.Next(0, 1000000);

                bool inHash = _petTable.GetAllElements().Any(p => p.PetId == newId);
                bool inDb = context.Pets.Any(p => p.PetId == newId);

                exists = inHash || inDb;

            } while (exists);

            return newId;
        }

        public static string PromptValidNameUpdate(string prompt, string currentValue)
        {
            while (true)
            {
                var input = AnsiConsole.Ask<string>($"{prompt} [grey](press Enter to keep current: {currentValue})[/]", currentValue);

                // If the user presses Enter, input will equal currentValue (because it's passed as default)
                if (string.IsNullOrWhiteSpace(input))
                    return currentValue;

                if (input.Length >= 2)
                    return input;

                AnsiConsole.MarkupLine("[red]Name must be at least 2 characters.[/]");
            }
        }


        public static int PromptValidIntUpdate(string prompt, int currentValue, int min = 0, int max = int.MaxValue)
        {
            while (true)
            {
                var input = AnsiConsole.Ask<string>($"{prompt} [grey](press Enter to keep current: {currentValue})[/]", currentValue.ToString());

                if (string.IsNullOrWhiteSpace(input))
                    return currentValue;

                if (int.TryParse(input, out int result) && result >= min && result <= max)
                    return result;

                AnsiConsole.MarkupLine($"[red]Please enter a valid number between {min} and {max}.[/]");
            }
        }


        public HashTable<Pet> GetPetHashTable() => _petTable;

        static void Warn(string msg) => AnsiConsole.MarkupLine($"[red]{msg}[/]");
        static string Ask(string label, string def = "") =>
        AnsiConsole.Ask<string>($"[dodgerblue1]{label}[/]", def);


    }

}