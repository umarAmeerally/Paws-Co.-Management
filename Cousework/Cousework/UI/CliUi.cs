using Cousework.DataStructures;
using Cousework.Models;
using Cousework.Services;
using Cousework.Utils;
using Spectre.Console;
using System;
using System.Linq;
using System.Text.RegularExpressions;

// Alias Spectre.Console.Table to avoid clash with EF‑Core metadata Table
using ConsoleTable = Spectre.Console.Table;

namespace Cousework;

public static class CliUi
{
    
    //  Load‑summary table
    
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

    
    //  Owners
    
    public static void DisplayOwners(OwnerService svc)
        => RenderTable(svc.GetOwnerHashTable().GetAllElements());

    public static void AddOwner(OwnerService svc, PetCareContext ctx)
    {
        AnsiConsole.MarkupLine("[bold green]📝 Add New Owner[/]");

        var owner = new Owner
        {
            OwnerId = svc.GenerateTrulyUniqueOwnerId(ctx),
            Name = PromptValidName("[blue]Owner Name[/] (e.g. John Doe):"),
            Email = PromptValidEmail("[blue]Email[/] (e.g. john@example.com):"),
            Phone = PromptValidPhone("[blue]Phone[/] (optional, e.g. +447123456789):"),
            Address = PromptValidAddress("[blue]Address[/] (optional):")
        };

        svc.AddOwner(owner);
        Success("Owner added.");
    }

    public static void UpdateOwner(OwnerService svc, PetCareContext ctx)
    {
        AnsiConsole.MarkupLine("[bold blue]🔍 Search for Owner to Update[/]");

        string search = Ask("Part of owner name/email");
        var owners = svc.GetOwnerHashTable().GetAllElements()
            .Where(o => (o.Name ?? "").Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        (o.Email ?? "").Contains(search, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (!owners.Any()) { Warn("No owner matches."); return; }

        var cur = AnsiConsole.Prompt(
            new SelectionPrompt<Owner>()
                .Title("Select owner to update")
                .UseConverter(o => Markup.Escape($"[{o.OwnerId}] {o.Name} ({o.Email})"))
                .AddChoices(owners));

        var upd = new Owner
        {
            Name = PromptValidNameUpdate("[blue]Owner Name[/] (current):", cur.Name),
            Email = PromptValidEmailUpdate("[blue]Email[/] (current):", cur.Email),
            Phone = PromptValidPhoneUpdate("[blue]Phone[/] (current):", cur.Phone),
            Address = PromptValidAddressUpdate("[blue]Address[/] (current):", cur.Address)
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

        if (!owners.Any()) { Warn("No owner matches."); return; }

        var selected = AnsiConsole.Prompt(
            new SelectionPrompt<Owner>()
                .Title("Select owner to delete")
                .UseConverter(o => Markup.Escape($"[{o.OwnerId}] {o.Name} ({o.Email})"))
                .AddChoices(owners));

        if (!AnsiConsole.Confirm($"Delete [red]{selected.Name}[/] and all their pets/appointments?"))
        {
            AnsiConsole.MarkupLine("[yellow]Deletion cancelled.[/]");
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

    
    //  Pets
    
    public static void DisplayPets(PetService svc)
        => RenderTable(svc.GetPetHashTable().GetAllElements());

    public static void AddPet(OwnerService oSvc, PetService pSvc, PetCareContext ctx)
    {
        string search = Ask("Part of owner name/email");
        var owners = oSvc.GetOwnerHashTable().GetAllElements()
            .Where(o => (o.Name ?? "").Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        (o.Email ?? "").Contains(search, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (!owners.Any()) { Warn("No owner matches."); return; }

        var owner = AnsiConsole.Prompt(
            new SelectionPrompt<Owner>()
                .Title("Select owner")
                .UseConverter(o => Markup.Escape($"[{o.OwnerId}] {o.Name} ({o.Email})"))
                .AddChoices(owners));

        string name = PromptValidName("[blue]Pet Name[/] (e.g. Bella):");
        string species = Ask("[blue]Species[/] (e.g. Dog, Cat):");
        string breed = Ask("[blue]Breed[/] (optional):");
        int age = PromptValidInt("[blue]Age[/] (e.g. 3):", min: 0);

        string gender = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[blue]Gender[/]")
                .AddChoices("Male", "Female"));

        var historyChoices = AnsiConsole.Prompt(
            new MultiSelectionPrompt<string>()
                .Title("[blue]Medical History[/] (space = select, enter = confirm)")
                .NotRequired()
                .AddChoices("Vaccinated", "Has Allergies", "Chronic Illness",
                            "Recently Treated", "None"));

        string medicalHistory = historyChoices.Any()
            ? string.Join(", ", historyChoices)
            : "None";

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
            DateRegistered = DateTime.Now
        };

        pSvc.AddPet(pet);
        Success("Pet added.");
    }

    public static void UpdatePet(OwnerService oSvc, PetService pSvc)
    {
        if (pSvc.UpdatePet(oSvc, pSvc))
            Success("Pet updated.");
    }

    public static void DeletePet(
        PetService pSvc,
        AppointmentService aSvc,
        OwnerService oSvc,
        PetCareContext db)
    {
        string search = Ask("Part of Pet Name or Owner Name");

        var matchingOwners = oSvc.GetOwnerHashTable().GetAllElements()
            .Where(o => o.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var petsByOwners = matchingOwners.SelectMany(owner =>
            pSvc.GetPetHashTable().GetAllElements()
                .Where(p => p.OwnerId == owner.OwnerId));

        var petsByName = pSvc.GetPetHashTable().GetAllElements()
            .Where(p => p.Name.Contains(search, StringComparison.OrdinalIgnoreCase));

        var allMatches = petsByName.Concat(petsByOwners)
            .GroupBy(p => p.PetId).Select(g => g.First()).ToList();

        if (!allMatches.Any()) { Warn("No matching pets."); return; }

        var selectedPet = AnsiConsole.Prompt(
            new SelectionPrompt<Pet>()
                .Title("Select pet to delete:")
                .UseConverter(p =>
                {
                    var owner = oSvc.GetOwnerById(p.OwnerId ?? 0);
                    return $"[{p.PetId}] {p.Name} - {p.Species} ({owner?.Name ?? "Unknown Owner"})";
                })
                .AddChoices(allMatches));

        if (DatabaseHelper.DeletePetAndAppointmentsFromDatabase(selectedPet.PetId, db))
        {
            pSvc.DeletePet(selectedPet.PetId);
            aSvc.DeleteAppointmentsByPetId(selectedPet.PetId);
            Success("Pet & appointments deleted.");
        }
        else Warn("Pet not found.");
    }

    
    //  Appointments
    
    public static void ViewAppointments(AppointmentService svc)
        => RenderTable(svc.GetAppointmentHashTable().GetAllElements());

    public static void AddAppointment(AppointmentService a, PetService p, OwnerService o)
        => a.AddAppointment(p, o);

    public static void UpdateAppointmentStatus(AppointmentService a, PetService p, OwnerService o)
    {
        if (a.UpdateAppointmentStatus(p, o))
            Success("Appointment updated.");
    }

    
    //  Search (Owners / Pets / Appointments)
    
    public static void SearchData(
        OwnerService oSvc,
        PetService pSvc,
        AppointmentService aSvc)
    {
        var entity = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Search in:")
                .AddChoices("Owners", "Pets", "Appointments"));

        string term = Ask("Enter search term").ToLowerInvariant();

        switch (entity)
        {
            case "Owners":
                var owners = oSvc.GetOwnerHashTable().GetAllElements()
                    .Where(o => $"{o.OwnerId} {o.Name} {o.Email}".ToLower().Contains(term));
                RenderTable(owners);
                break;

            case "Pets":
                var pets = pSvc.GetPetHashTable().GetAllElements()
                    .Where(p => $"{p.PetId} {p.Name} {p.Species} {p.Breed}".ToLower().Contains(term));
                RenderTable(pets);
                break;

            case "Appointments":
                var appts = aSvc.GetAppointmentHashTable().GetAllElements()
                    .Where(a => a.ToString().ToLower().Contains(term));
                RenderTable(appts);
                break;
        }
    }

    
    //  Save all data to DB
    
    public static void SaveAllData(
        OwnerService oSvc,
        PetService pSvc,
        AppointmentService aSvc)
    {
        if (!AnsiConsole.Confirm("Save all data to database?")) return;

        string cs = "Data Source=AKASH;Initial Catalog=updateddbms;Integrated Security=True;Trust Server Certificate=True";

        AnsiConsole.Status().Spinner(Spinner.Known.Line).Start("Saving…", _ =>
        {
            DatabaseHelper.SaveOwnersToDatabase(oSvc.GetOwnerHashTable(), cs);
            DatabaseHelper.SavePetsToDatabase(pSvc.GetPetHashTable(), cs);
            DatabaseHelper.SaveAppointmentsToDatabase(aSvc.GetAppointmentHashTable(), cs);
        });

        Success("All data saved.");
    }

    
    //  Helper prompts & validators
    
    static string Ask(string label, string def = "")
        => AnsiConsole.Ask<string>($"[dodgerblue1]{label}[/]", def);

    static int PromptValidInt(string msg, int min = 0, int max = 100)
    {
        while (true)
        {
            var input = Ask(msg);
            if (int.TryParse(input, out int v) && v >= min && v <= max) return v;
            Warn($"Enter a number between {min} and {max}.");
        }
    }

    static string PromptValidName(string label)
    {
        while (true)
        {
            var input = Ask(label).Trim();
            if (input.Length == 0) Warn("Name required.");
            else if (input.Length > 100) Warn("Max 100 chars.");
            else if (!input.All(c => char.IsLetter(c) || c == ' ')) Warn("Letters/spaces only.");
            else return input;
        }
    }

    static string PromptValidEmail(string label)
    {
        while (true)
        {
            var input = Ask(label).Trim();
            if (input.Length == 0) Warn("Email required.");
            else if (!Regex.IsMatch(input, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                Warn("Invalid email format.");
            else return input;
        }
    }

    static string PromptValidPhone(string label)
    {
        while (true)
        {
            var input = Ask(label).Trim();
            if (input == "") return "";  // optional
            if (Regex.IsMatch(input, @"^[\\d\\+\\-\\(\\)\\s]{7,15}$")) return input;
            Warn("Digits, +‑()‑space only.");
        }
    }

    static string PromptValidAddress(string label)
    {
        while (true)
        {
            var input = Ask(label).Trim();
            if (input == "") return "";          // optional
            if (input.Length <= 255) return input;
            Warn("Max 255 chars.");
        }
    }

    // update variants (Enter = keep current)
    static string PromptValidNameUpdate(string prompt, string cur)
    {
        var input = Ask($"{prompt} [grey](Enter = keep)[/]", cur).Trim();
        return string.IsNullOrWhiteSpace(input) ? cur : input;
    }
    static string PromptValidEmailUpdate(string prompt, string cur)
    {
        var input = Ask($"{prompt} [grey](Enter = keep)[/]", cur).Trim();
        return string.IsNullOrWhiteSpace(input) ? cur : input;
    }
    static string PromptValidPhoneUpdate(string prompt, string cur)
    {
        var input = Ask($"{prompt} [grey](Enter = keep)[/]", cur).Trim();
        return string.IsNullOrWhiteSpace(input) ? cur : input;
    }
    static string PromptValidAddressUpdate(string prompt, string cur)
    {
        var input = Ask($"{prompt} [grey](Enter = keep)[/]", cur).Trim();
        return string.IsNullOrWhiteSpace(input) ? cur : input;
    }

    
    //  Spectre helpers
    
    public static void Success(string msg) => AnsiConsole.MarkupLine($"[green]✔ {msg}[/]");
    public static void Warn(string msg) => AnsiConsole.MarkupLine($"[red]{msg}[/]");

    static void RenderTable<T>(System.Collections.Generic.IEnumerable<T> rows)
    {
        var t = new ConsoleTable().Border(TableBorder.Rounded)
                                  .AddColumn(typeof(T).Name);
        foreach (var r in rows)
            t.AddRow(r?.ToString() ?? "");
        AnsiConsole.Write(t);
    }
}
