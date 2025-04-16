using Cousework.DataStructures;
using Cousework.Models;
using Cousework.Services;
using Cousework.Utils;
using Spectre.Console;
using System;

namespace Cousework;

enum DataSource { Csv, Database }
record MenuItem(string Label, DataSource Value);

class Program
{
    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;   // emoji & UTF‑8
        Console.Title = "🐾  Pet Management System";

        AnsiConsole.Write(new FigletText("Pet Care").Centered().Color(Color.SpringGreen3));

        // choose data source 
        var srcChoice = AnsiConsole.Prompt(
            new SelectionPrompt<MenuItem>()
                .Title("Choose [green]data source[/]:")
                .UseConverter(i => i.Label)
                .AddChoices(
                    new MenuItem("📄  Load data from CSV", DataSource.Csv),
                    new MenuItem("🗄️  Load data from Database", DataSource.Database)));

        bool isCsvMode = srcChoice.Value == DataSource.Csv;

        var context = new PetCareContext();
        HashTable<Owner> ownerTable = null!;
        HashTable<Pet> petTable = null!;
        HashTable<Appointment> apptTable = null!;

        switch (srcChoice.Value)
        {
            case DataSource.Csv:
                {
                    string csvPath = AnsiConsole.Ask<string>("CSV path?", "C:/Users/akash/Desktop/pet_management_data.csv");
                    AnsiConsole.Status().Spinner(Spinner.Known.Dots).Start("Parsing CSV…", _ =>
                    {
                        var csv = new CSVReader();
                        csv.ParseCSV(csvPath);
                        ownerTable = csv.OwnerHashTable;
                        petTable = csv.PetHashTable;
                        apptTable = csv.AppointmentHashTable;
                    });
                    CliUi.Success("CSV loaded.");
                    CliUi.ShowLoadSummary(ownerTable, petTable, apptTable, "CSV");
                    break;
                }
            case DataSource.Database:
                {
                    AnsiConsole.Status().Spinner(Spinner.Known.Line).Start("Loading DB…", _ =>
                    {
                        (ownerTable, petTable, apptTable) = DatabaseLoader.LoadData(context);
                    });
                    CliUi.Success("Database loaded.");
                    CliUi.ShowLoadSummary(ownerTable, petTable, apptTable, "Database");
                    break;
                }
        }

        var ownerSvc = new OwnerService(ownerTable);
        var petSvc = new PetService(context, petTable, ownerTable);
        var apptSvc = new AppointmentService(apptTable);

        RunMenu(ownerSvc, petSvc, apptSvc, context, isCsvMode);
    }

    
    static void RunMenu(
        OwnerService owners,
        PetService pets,
        AppointmentService appts,
        PetCareContext db,
        bool isCsvMode)
    {
        while (true)
        {
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("\n[bold dodgerblue1]Main Menu[/]")
                    .PageSize(15)
                    .AddChoices(new[]
                    {
                        "👥  Display owners",
                        "➕  Add owner",
                        "✏️  Update owner",
                        "🗑️  Delete owner",
                        "🐶  Display pets",
                        "➕  Add pet",
                        "✏️  Update pet",
                        "🗑️  Delete pet",
                        "📅  View appointments",
                        "➕  Add appointment",
                        "✏️  Update appointment",
                        "🔍  Search data",
                        "💾  Save all data",
                        "🚪  Exit"
                    }));

            switch (choice.Split(' ')[0])
            {
                case "👥": CliUi.DisplayOwners(owners); break;
                case "➕" when choice.Contains("owner"): CliUi.AddOwner(owners, db); break;
                case "✏️" when choice.Contains("owner"): CliUi.UpdateOwner(owners, db); break;
                case "🗑️" when choice.Contains("owner"):
                    if (isCsvMode)
                        AnsiConsole.MarkupLine("[yellow]⚠ Deletion disabled in CSV mode.[/]");
                    else
                        CliUi.DeleteOwner(owners, pets, db);
                    break;

                case "🐶": CliUi.DisplayPets(pets); break;
                case "➕" when choice.Contains("pet"): CliUi.AddPet(owners, pets, db); break;
                case "✏️" when choice.Contains("pet"): CliUi.UpdatePet(owners, pets); break;
                case "🗑️" when choice.Contains("pet"):
                    if (isCsvMode)
                        AnsiConsole.MarkupLine("[yellow]⚠ Deletion disabled in CSV mode.[/]");
                    else
                        CliUi.DeletePet(pets, appts, owners, db);
                    break;

                case "📅": CliUi.ViewAppointments(appts); break;
                case "➕" when choice.Contains("appointment"): CliUi.AddAppointment(appts, pets, owners); break;
                case "✏️" when choice.Contains("appointment"): CliUi.UpdateAppointmentStatus(appts, pets, owners); break;

                case "🔍": CliUi.SearchData(owners, pets, appts); break;

                case "💾": CliUi.SaveAllData(owners, pets, appts); break;
                case "🚪": return;
            }
        }
    }
}
