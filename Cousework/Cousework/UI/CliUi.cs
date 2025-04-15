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
        var owner = new Owner
        {
            OwnerId = svc.GenerateTrulyUniqueOwnerId(ctx),
            Name = Ask("Owner name"),
            Email = Ask("Email"),
            Phone = Ask("Phone"),
            Address = Ask("Address")
        };
        svc.AddOwner(owner);
        Success("Owner added.");
    }

    public static void UpdateOwner(OwnerService svc, PetCareContext ctx)
    {
        int id = AskInt("Owner ID to update");
        var cur = svc.GetOwnerHashTable().GetAllElements().FirstOrDefault(o => o.OwnerId == id);
        if (cur == null) { Warn("Owner not found."); return; }

        var upd = new Owner
        {
            OwnerId = id,
            Name = Ask($"Owner name ({cur.Name})", cur.Name),
            Email = Ask($"Email ({cur.Email})", cur.Email),
            Phone = Ask($"Phone ({cur.Phone})", cur.Phone),
            Address = Ask($"Address ({cur.Address})", cur.Address)
        };
        svc.UpdateOwner(id, upd);
        Success("Owner updated.");
    }

    public static void DeleteOwner(OwnerService oSvc, PetService pSvc, PetCareContext db)
    {
        int id = AskInt("Owner ID to delete");
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
        string search = Ask("Part of owner name/email");
        var owners = oSvc.GetOwnerHashTable().GetAllElements()
            .Where(o => (o.Name ?? "").Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        (o.Email ?? "").Contains(search, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (!owners.Any()) { Warn("No owner matches."); return; }

        var owner = AnsiConsole.Prompt(
            new SelectionPrompt<Owner>()
                .Title("Select owner")
                .UseConverter(o => $"[{o.OwnerId}] {o.Name} ({o.Email})")
                .AddChoices(owners));

        var pet = new Pet
        {
            PetId = pSvc.GenerateTrulyUniquePetId(ctx),
            OwnerId = owner.OwnerId,
            Name = Ask("Pet name"),
            Species = Ask("Species"),
            Breed = Ask("Breed"),
            Age = AskInt("Age"),
            Gender = Ask("Gender"),
            MedicalHistory = Ask("Medical history"),
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

    public static void DeletePet(PetService pSvc, AppointmentService aSvc, PetCareContext db)
    {
        int id = AskInt("Pet ID to delete");
        if (DatabaseHelper.DeletePetAndAppointmentsFromDatabase(id, db))
        {
            pSvc.DeletePet(id);
            aSvc.DeleteAppointmentsByPetId(id);
            Success("Pet & appointments deleted.");
        }
        else Warn("Pet not found.");
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

        string cs = "Data Source=AKASH;Initial Catalog=petmanagementdb;Integrated Security=True;Trust Server Certificate=True";

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
    static string Ask(string label, string def = "") =>
        AnsiConsole.Ask<string>($"[dodgerblue1]{label}[/]", def);

    static int AskInt(string label) =>
        AnsiConsole.Ask<int>($"[dodgerblue1]{label}[/]");

    static void Success(string msg) => AnsiConsole.MarkupLine($"[green]✔ {msg}[/]");
    static void Warn(string msg) => AnsiConsole.MarkupLine($"[red]{msg}[/]");

    static void RenderTable<T>(System.Collections.Generic.IEnumerable<T> rows)
    {
        var t = new ConsoleTable().Border(TableBorder.Rounded)
                                  .AddColumn(typeof(T).Name);
        foreach (var r in rows)
            t.AddRow(r?.ToString() ?? "");
        AnsiConsole.Write(t);
    }
}

