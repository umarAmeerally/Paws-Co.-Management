using Cousework.DataStructures;
using Cousework.Models;
using Cousework.Services;
using Spectre.Console;

using Cousework.DataStructures;
using Cousework.Models;
using Cousework.Services;
using Spectre.Console;
using System;
using System.Linq;

// ── Alias Spectre.Console.Table to avoid clash with EF‑Core's metadata Table
using ConsoleTable = Spectre.Console.Table;
using System.Text.RegularExpressions;

namespace Cousework;

public static class CliUi
{
    // ─────────────────────────────────────────────────────────────────────
    //  Load‑summary table
    // ─────────────────────────────────────────────────────────────────────
    public static void ShowLoadSummary(
        HashTable<Owner> owners,
        HashTable<Pet> pets,
        HashTable<Appointment> appts,
        string label)
    {
        var t = new ConsoleTable()
            .Border(TableBorder.Rounded)
            .Title($"[bold]{label} Loaded[/]");

        t.AddColumn("Entity");
        t.AddColumn("Count");

        t.AddRow("Owners", owners.Count().ToString());
        t.AddRow("Pets", pets.Count().ToString());
        t.AddRow("Appointments", appts.Count().ToString());

        AnsiConsole.Write(t);
    }

    // ─────────────────────────────────────────────────────────────────────
    //  Owners
    // ─────────────────────────────────────────────────────────────────────
    public static void DisplayOwners(OwnerService svc)
        => RenderTable(svc.GetOwnerHashTable().GetAllElements());

    public static void AddOwner(OwnerService svc, PetCareContext ctx)
    {

        AnsiConsole.MarkupLine("[bold green]📝 Add New Owner[/]");

        var owner = new Owner
        {
            OwnerId = svc.GenerateTrulyUniqueOwnerId(ctx),
            Name = PromptValidName("[blue]Owner Name[/] (e.g. John Doe):"),
            Email = PromptValidEmail("[blue]Email[/] (e.g. john@example.com):"),
            Phone = PromptValidPhone("[blue]Phone[/] (optional, e.g. +447123456789):"),
            Address = PromptValidAddress("[blue]Address[/] (optional):")
        };

        svc.AddOwner(owner);
        Success("Owner added.");
    }

    public static void UpdateOwner(OwnerService svc, PetCareContext ctx )
    {
        AnsiConsole.MarkupLine("[bold blue]🔍 Search for Owner to Update[/]");

        string search = Ask("Part of owner name/email");
        var owners = svc.GetOwnerHashTable().GetAllElements()
            .Where(o => (o.Name ?? "").Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        (o.Email ?? "").Contains(search, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (!owners.Any())
        {
            Warn("No owner matches.");
            return;
        }

        var cur = AnsiConsole.Prompt(
            new SelectionPrompt<Owner>()
                .Title("Select owner to update")
                .UseConverter(o => Markup.Escape($"[{o.OwnerId}] {o.Name} ({o.Email})"))
                .AddChoices(owners));

        var upd = new Owner
        {
            Name = PromptValidNameUpdate($"[blue]Owner Name[/] (current: {cur.Name}):", cur.Name),
            Email = PromptValidEmailUpdate($"[blue]Email[/] (current: {cur.Email}):", cur.Email),
            Phone = PromptValidPhoneUpdate($"[blue]Phone[/] (current: {cur.Phone}):", cur.Phone),
            Address = PromptValidAddressUpdate($"[blue]Address[/] (current: {cur.Address}):", cur.Address)
        };

        svc.UpdateOwner(cur.OwnerId, upd);
        Success("Owner updated.");
    }


    public static void DeleteOwner(OwnerService oSvc, PetService pSvc, PetCareContext db)
    {
        AnsiConsole.MarkupLine("[bold red]🗑️ Search for Owner to Delete[/]");

        string search = Ask("Part of owner name/email");
        var owners = oSvc.GetOwnerHashTable().GetAllElements()
            .Where(o => (o.Name ?? "").Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        (o.Email ?? "").Contains(search, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (!owners.Any())
        {
            Warn("No owner matches.");
            return;
        }

        var selected = AnsiConsole.Prompt(
            new SelectionPrompt<Owner>()
                .Title("Select owner to delete")
                .UseConverter(o => Markup.Escape($"[{o.OwnerId}] {o.Name} ({o.Email})"))
                .AddChoices(owners)
        );

        if (!AnsiConsole.Confirm($"Are you sure you want to delete [red]{selected.Name}[/] and all their pets/appointments?"))
        {
            AnsiConsole.MarkupLine("[yellow]⚠️ Deletion cancelled.[/]");
            return;
        }

        int id = selected.OwnerId;

        if (DatabaseHelper.DeleteOwnerAndPetsFromDatabase(id, db))
        {
            oSvc.DeleteOwner(id);
            pSvc.DeletePetsByOwnerId(id);
            Success("Owner and pets deleted.");
        }
        else Warn("Owner not found.");
    }


    // ─────────────────────────────────────────────────────────────────────
    //  Pets
    // ─────────────────────────────────────────────────────────────────────
    public static void DisplayPets(PetService svc)
        => RenderTable(svc.GetPetHashTable().GetAllElements());

    public static void AddPet(OwnerService oSvc, PetService pSvc, PetCareContext ctx)
    {
        // Search for the owner by name or email
        string search = Ask("Part of owner name/email");
        var owners = oSvc.GetOwnerHashTable().GetAllElements()
            .Where(o => (o.Name ?? "").Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        (o.Email ?? "").Contains(search, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (!owners.Any())
        {
            Warn("No owner matches.");
            return;
        }

        // Let user select the owner
        var owner = AnsiConsole.Prompt(
            new SelectionPrompt<Owner>()
                .Title("Select owner")
                .UseConverter(o => Markup.Escape($"[{o.OwnerId}] {o.Name} ({o.Email})"))
                .AddChoices(owners));

        // Add pet details with validations
        string name = PromptValidName("[blue]Pet Name[/] (e.g. Bella):");
        string species = Ask("[blue]Species[/] (e.g. Dog, Cat):");
        string breed = Ask("[blue]Breed[/] (optional):");
        int age = PromptValidInt("[blue]Age[/] (e.g. 3):", min: 0);

        // Gender selection with options
        string gender = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[blue]Gender[/]")
                .AddChoices("Male", "Female")
        );

        var historyChoices = AnsiConsole.Prompt(
        new MultiSelectionPrompt<string>()
        .Title("[blue]Medical History[/] (Select one or more)")
        .NotRequired()
        .InstructionsText("[violet](Press space to select, enter to confirm)[/]")
        .AddChoices("Vaccinated", "Has Allergies", "Has Chronic Illness", "Recently Treated", "None")
);

        // Check if any choices were selected
        string medicalHistory = historyChoices.Any() ? string.Join(", ", historyChoices) : "None"; // Default to "None" if nothing selected
        DateTime dateRegistered = DateTime.Now; // Auto-assign current date

        // Create a new Pet object with the validated inputs
        var pet = new Pet
        {
            PetId = pSvc.GenerateTrulyUniquePetId(ctx),
            OwnerId = owner.OwnerId,
            Name = name,
            Species = species,
            Breed = breed,
            Age = age,
            Gender = gender,
            MedicalHistory = medicalHistory,
            DateRegistered = dateRegistered
        };

        // Add the pet to the service
        pSvc.AddPet(pet);

        // Success message
        Success("Pet added.");
    }


    public static void UpdatePet(OwnerService oSvc, PetService pSvc)
    {
        if (pSvc.UpdatePet(oSvc, pSvc))
            Success("Pet updated.");
    }

    public static void DeletePet(PetService pSvc, AppointmentService aSvc, OwnerService oSvc, PetCareContext db)
    {
        string search = Ask("Enter part of Pet Name or Owner Name");

        // Match owners by name
        var matchingOwners = oSvc.GetOwnerHashTable().GetAllElements()
            .Where(o => o.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
            .ToList();

        // Get all pets that match the pet name or belong to matching owners
        var petsByOwners = matchingOwners
            .SelectMany(owner => pSvc.GetPetHashTable().GetAllElements()
                .Where(p => p.OwnerId == owner.OwnerId))
            .ToList();

        var petsByName = pSvc.GetPetHashTable().GetAllElements()
            .Where(p => p.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
            .ToList();

        // Combine both and remove duplicates
        var allMatches = petsByName
            .Concat(petsByOwners)
            .GroupBy(p => p.PetId)
            .Select(g => g.First())
            .ToList();

        if (!allMatches.Any())
        {
            Warn("No matching pets found.");
            return;
        }

        // Select which pet to delete
        var selectedPet = AnsiConsole.Prompt(
            new SelectionPrompt<Pet>()
                .Title("Select the pet to delete:")
                .UseConverter(p =>
                {
                    var owner = oSvc.GetOwnerById(p.OwnerId ?? 0);
                    return $"[{p.PetId}] {p.Name} - {p.Species} ({owner?.Name ?? "Unknown Owner"})";
                })
                .AddChoices(allMatches)
        );

        // Perform deletion
        if (DatabaseHelper.DeletePetAndAppointmentsFromDatabase(selectedPet.PetId, db))
        {
            pSvc.DeletePet(selectedPet.PetId);
            aSvc.DeleteAppointmentsByPetId(selectedPet.PetId);
            Success("Pet & appointments deleted.");
        }
        else
        {
            Warn("Pet not found.");
        }
    }



    // ─────────────────────────────────────────────────────────────────────
    //  Appointments
    // ─────────────────────────────────────────────────────────────────────
    public static void ViewAppointments(AppointmentService svc)
        => RenderTable(svc.GetAppointmentHashTable().GetAllElements());

    public static void AddAppointment(AppointmentService a, PetService p, OwnerService o)
        => a.AddAppointment(p, o);

    public static void UpdateAppointmentStatus(AppointmentService a, PetService p, OwnerService o)
    {
        if (a.UpdateAppointmentStatus(p, o))
            Success("Appointment updated.");
    }

    // ─────────────────────────────────────────────────────────────────────
    //  Save to DB
    // ─────────────────────────────────────────────────────────────────────
    public static void SaveAllData(
        OwnerService oSvc, PetService pSvc, AppointmentService aSvc)
    {
        if (!AnsiConsole.Confirm("Save all data to database?")) return;

        string cs = "Data Source=HP;Initial Catalog=Cousework;Integrated Security=True;Encrypt=True;Trust Server Certificate=True";

        AnsiConsole.Status().Spinner(Spinner.Known.Line).Start("Saving…", _ =>
        {
            DatabaseHelper.SaveOwnersToDatabase(oSvc.GetOwnerHashTable(), cs);
            DatabaseHelper.SavePetsToDatabase(pSvc.GetPetHashTable(), cs);
            DatabaseHelper.SaveAppointmentsToDatabase(aSvc.GetAppointmentHashTable(), cs);
        });

        Success("All data saved.");
    }

    // ─────────────────────────────────────────────────────────────────────
    //  Helpers
    // ─────────────────────────────────────────────────────────────────────
    static string Ask(string label, string def = "")
    {
        // Ask the user for input
        var input = AnsiConsole.Ask<string>($"[dodgerblue1]{label}[/]", def);

        // If no input was provided, return the default value
        return string.IsNullOrWhiteSpace(input) ? def : input;
    }


    static int AskInt(string label) =>
        AnsiConsole.Ask<int>($"[dodgerblue1]{label}[/]");

    static void Success(string msg) => AnsiConsole.MarkupLine($"[green]✔ {msg}[/]");
    static void Warn(string msg) => AnsiConsole.MarkupLine($"[red]{msg}[/]");

    static string PromptValidName(string label)
    {
        while (true)
        {
            var input = Ask(label).Trim();
            if (input.Length == 0)
            {
                Warn("Name is required.");
            }
            else if (input.Length > 100)
            {
                Warn("Name must be under 100 characters.");
            }
            else if (!input.All(c => char.IsLetter(c) || c == ' '))
            {
                Warn("Name can only contain letters and spaces.");
            }
            else return input;
        }
    }

    static string PromptValidEmail(string label)
    {
        while (true)
        {
            var input = Ask(label).Trim();
            if (input.Length == 0)
            {
                Warn("Email is required.");
            }
            else if (!input.Contains('@') || !input.Contains('.'))
            {
                Warn("Invalid email format.");
            }
            else return input;
        }
    }

    static string PromptValidPhone(string label)
    {
        while (true)
        {
            var input = Ask(label).Trim();
            if (string.IsNullOrEmpty(input)) return "";  // optional
            if (input.All(c => char.IsDigit(c) || "+-() ".Contains(c)))
                return input;
            else Warn("Phone can only contain digits, spaces, +, -, (, ).");
        }
    }

    static string PromptValidAddress(string label)
    {
        while (true)
        {
            var input = Ask(label).Trim();
            if (input.Length == 0) return "";  // optional
            if (input.Length > 255)
            {
                Warn("Address must be less than 255 characters.");
            }
            else return input;
        }
    }

    public static string PromptValidNameUpdate(string prompt, string currentValue)
    {
        while (true)
        {
            var input = AnsiConsole.Ask<string>(
                $"{prompt} [grey](press Enter to keep current)[/]",
                currentValue
            );

            if (string.IsNullOrWhiteSpace(input))
                return currentValue;

            if (input.Length >= 2)
                return input;

            AnsiConsole.MarkupLine("[red]Name must be at least 2 characters.[/]");
        }
    }

    public static string PromptValidEmailUpdate(string prompt, string currentValue)
    {
        while (true)
        {
            var input = AnsiConsole.Ask<string>(
                $"{prompt} [grey](press Enter to keep current)[/]",
                currentValue
            );

            if (string.IsNullOrWhiteSpace(input))
                return currentValue;

            if (Regex.IsMatch(input, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                return input;

            AnsiConsole.MarkupLine("[red]Invalid email format.[/]");
        }
    }

    public static string PromptValidPhoneUpdate(string prompt, string currentValue)
    {
        while (true)
        {
            var input = AnsiConsole.Ask<string>(
                $"{prompt} [grey](press Enter to keep current)[/]",
                currentValue
            );

            if (string.IsNullOrWhiteSpace(input))
                return currentValue;

            if (Regex.IsMatch(input, @"^\+?[0-9\s\-()]{7,15}$"))
                return input;

            AnsiConsole.MarkupLine("[red]Invalid phone number format.[/]");
        }
    }

    public static string PromptValidAddressUpdate(string prompt, string currentValue)
    {
        var input = AnsiConsole.Ask<string>(
            $"{prompt} [grey](press Enter to keep current)[/]",
            currentValue
        );

        return string.IsNullOrWhiteSpace(input) ? currentValue : input;
    }



    public static int PromptValidInt(string message, int min = 0, int max = 100)
    {
        while (true)
        {
            var input = AnsiConsole.Ask<string>(message);

            if (int.TryParse(input, out int value))
            {
                if (value >= min && value <= max)
                    return value;
                else
                    Warn($"Please enter a number between {min} and {max}.");
            }
            else
            {
                Warn("Invalid number. Please enter a valid integer.");
            }
        }
    }





    static void RenderTable<T>(System.Collections.Generic.IEnumerable<T> rows)
    {
        var t = new ConsoleTable().Border(TableBorder.Rounded)
                                  .AddColumn(typeof(T).Name);
        foreach (var r in rows)
            t.AddRow(r?.ToString() ?? "");
        AnsiConsole.Write(t);
    }
}

