using Cousework.DataStructures;
using Cousework.Models;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cousework.Services
{
    public class AppointmentService
    {
        private readonly HashTable<Appointment> _appointmentTable;

        public AppointmentService(HashTable<Appointment> appointmentTable)
        {
            _appointmentTable = appointmentTable;
        }

        public void AddAppointment(PetService petService, OwnerService ownerService)
        {
            AnsiConsole.MarkupLine("\n[bold dodgerblue1]📅 Add New Appointment[/]");

            var selectedPet = PromptForPetByOwner(ownerService, petService);

            if (selectedPet == null)
            {
                AnsiConsole.MarkupLine("[red]No pet selected. Cannot add appointment.[/]");
                return;
            }
            int day = AnsiConsole.Prompt(
                new TextPrompt<int>("Enter [green]day[/] [[1-31]]:")
                .Validate(d =>
                {
                    return d is >= 1 and <= 31
                        ? ValidationResult.Success()
                        : ValidationResult.Error("[red]Day must be between 1 and 31[/]");
                }));

                int month = AnsiConsole.Prompt(
                new TextPrompt<int>("Enter [green]month[/] [[1-12]]:")
                    .Validate(m =>
                    {
                        return m is >= 1 and <= 12
                            ? ValidationResult.Success()
                            : ValidationResult.Error("[red]Month must be between 1 and 12[/]");
                    }));

                int year = AnsiConsole.Prompt(
                new TextPrompt<int>("Enter [green]year[/] [[e.g. 2025]]:")
                    .Validate(y =>
                    {
                        return y >= 2000 && y <= 2100
                            ? ValidationResult.Success()
                            : ValidationResult.Error("[red]Year must be between 2000 and 2100[/]");
                    }));

            DateTime appointmentDate;

            // ✅ Construct the date safely and catch only this point
            try
            {
                appointmentDate = new DateTime(year, month, day);
            }
            catch
            {
                AnsiConsole.MarkupLine("[red]❌ Invalid date combination (e.g., Feb 30). Please try again.[/]");
                return; // or loop back if you want to retry
            }



            // Select Appointment Type
            string type = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[green]Select Appointment Type[/]:")
                    .AddChoices("Check-up", "Vaccination", "Grooming", "Surgery"));

            // Select Appointment Status
            string status = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[green]Select Appointment Status[/]:")
                    .AddChoices("Scheduled", "Completed", "Cancelled"));

            // Generate unique appointment ID
            int newAppointmentId = _appointmentTable.GetAllElements().Any()
                ? _appointmentTable.GetAllElements().Max(a => a.AppointmentId) + 1
                : 1;

            var newAppointment = new Appointment
            {
                AppointmentId = newAppointmentId,
                AppointmentDate = appointmentDate,
                Type = type,
                Status = status,
                PetId = selectedPet.PetId,
                OwnerId = selectedPet.OwnerId
            };

            // Insert into hash table
            _appointmentTable.Insert(newAppointment);

            AnsiConsole.MarkupLine("[green]✔ Appointment added successfully![/]");
        }


        public bool UpdateAppointmentStatus(PetService petService, OwnerService ownerService)
        {
            // Prompt for the owner's name or pet's name to find matching appointments
            var search = AnsiConsole.Ask<string>("Enter part of the [green]owner's[/] or [green]pet's[/] name:");

            // Find matching appointments
            var matchingAppointments = _appointmentTable
                .GetAllElements()
                .Where(a => petService
                            .GetPetHashTable()
                            .GetAllElements()
                            .Any(p => p.Name.Contains(search, StringComparison.OrdinalIgnoreCase) && p.PetId == a.PetId) ||
                            ownerService
                            .GetOwnerHashTable()
                            .GetAllElements()
                            .Any(o => o.Name.Contains(search, StringComparison.OrdinalIgnoreCase) && o.OwnerId == a.OwnerId))
                .ToList();

            if (matchingAppointments.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]❌ No matching appointments found.[/]");
                return false;
            }

            // Display the matching appointments
            AnsiConsole.MarkupLine("\n[blue]Matching Appointments:[/]");
            foreach (var appointment in matchingAppointments)
            {
                string petName = "Unknown Pet";
                string ownerName = "Unknown Owner";

                if (appointment?.PetId != null)
                {
                    var pet = petService.GetPetHashTable().SearchByKey(appointment.PetId.Value);
                    if (pet != null)
                    {
                        petName = pet.Name ?? "Unknown Pet";

                        if (pet.OwnerId != null)
                        {
                            var owner = ownerService.GetOwnerHashTable().SearchByKey(pet.OwnerId.Value);
                            if (owner != null)
                                ownerName = owner.Name ?? "Unknown Owner";
                        }
                    }
                }

                AnsiConsole.MarkupLine($"[bold yellow]ID:[/] {appointment.AppointmentId} | [green]Pet:[/] {petName} | [blue]Owner:[/] {ownerName} | [cyan]Date:[/] {appointment.AppointmentDate:dd/MM/yyyy} | [purple]Status:[/] {appointment.Status}");
            }

            int appointmentId = AnsiConsole.Ask<int>("\nEnter the [green]Appointment ID[/] from the list:");

            var appointmentToUpdate = matchingAppointments.FirstOrDefault(a => a.AppointmentId == appointmentId);
            if (appointmentToUpdate == null)
            {
                AnsiConsole.MarkupLine("[red]❌ Appointment not found.[/]");
                return false;
            }

            // Status options
            var newStatus = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select the [green]new status[/]:")
                    .AddChoices("Scheduled", "Completed", "Cancelled")
            );

            // Update and confirm
            appointmentToUpdate.Status = newStatus;
            AnsiConsole.MarkupLine("[green]✅ Appointment status updated successfully.[/]");
            return true;
        }



        public void DisplayAppointments()
        {
            Console.WriteLine("Appointments:");
            _appointmentTable.DisplayContents();
        }

        public bool DeleteAppointment(int appointmentId)
        {
            return _appointmentTable.DeleteByKey(appointmentId);
        }

     

        public void DeleteAppointmentsByPetId(int petId)
        {
            var appointmentsToDelete = _appointmentTable.GetAllElements()
                                                        .Where(a => a.PetId == petId)
                                                        .Select(a => a.AppointmentId)
                                                        .ToList();

            foreach (var appointmentId in appointmentsToDelete)
            {
                _appointmentTable.DeleteByKey(appointmentId);
            }
        }


        public HashTable<Appointment> GetAppointmentHashTable() => _appointmentTable;

        public static Pet PromptForPetByOwner(OwnerService ownerService, PetService petService)
        {
            AnsiConsole.MarkupLine("\n[bold yellow]🔍 Find Pet by Owner[/]");

            string search = AnsiConsole.Ask<string>("Enter part of the owner's [green]name[/] or [green]email[/]:");

            var matchingOwners = ownerService
                .GetOwnerHashTable()
                .GetAllElements()
                .Where(o => o.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                            o.Email.Contains(search, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (matchingOwners.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]❌ No matching owners found.[/]");
                return null;
            }

            // Let the user select from matching owners
            var selectedOwner = AnsiConsole.Prompt(
                new SelectionPrompt<Owner>()
                    .Title("Select an [blue]owner[/]:")
                    .UseConverter(o => $"ID: {o.OwnerId} | {o.Name} ({o.Email})")
                    .AddChoices(matchingOwners));

            var petsOwnedByOwner = petService
                .GetPetHashTable()
                .GetAllElements()
                .Where(p => p.OwnerId == selectedOwner.OwnerId)
                .ToList();

            if (petsOwnedByOwner.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]⚠️ This owner does not have any pets.[/]");
                return null;
            }

            // Let user pick one of their pets
            var selectedPet = AnsiConsole.Prompt(
                new SelectionPrompt<Pet>()
                    .Title("Select a [blue]pet[/]:")
                    .UseConverter(p => $"ID: {p.PetId} | {p.Name} ({p.Breed})")
                    .AddChoices(petsOwnedByOwner));

            return selectedPet;
        }

    }


}
