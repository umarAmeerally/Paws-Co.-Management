using Cousework.DataStructures;
using Cousework.Models;
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
            var selectedPet = PromptForPetByOwner(ownerService, petService);

            if (selectedPet == null)
            {
                Console.WriteLine("No pet selected. Cannot add appointment.");
                return;
            }

            Console.Write("Enter Appointment Date (yyyy-MM-dd): ");
            if (!DateTime.TryParse(Console.ReadLine(), out DateTime appointmentDate))
            {
                Console.WriteLine("Invalid date format.");
                return;
            }

            string[] validTypes = { "Check-up", "Vaccination", "Grooming", "Surgery" };
            string[] validStatuses = { "Scheduled", "Completed", "Cancelled" };

            Console.Write("Enter Appointment Type (Check-up, Vaccination, Grooming, Surgery): ");
            string type = Console.ReadLine();
            if (!validTypes.Contains(type))
            {
                Console.WriteLine("Invalid type entered.");
                return;
            }

            Console.Write("Enter Appointment Status (Scheduled, Completed, Cancelled): ");
            string status = Console.ReadLine();
            if (!validStatuses.Contains(status))
            {
                Console.WriteLine("Invalid status entered.");
                return;
            }

            // Generate unique ID
            int newAppointmentId = _appointmentTable.GetAllElements().Count() > 0

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

            // ✅ Insert into the hash table
            _appointmentTable.Insert( newAppointment);

            Console.WriteLine("Appointment added successfully.");
        }

        public bool UpdateAppointmentStatus(PetService petService, OwnerService ownerService)
        {
            // Prompt for the owner's name or pet's name to find matching appointments
            Console.Write("Enter part of the owner's name or pet's name: ");
            string search = Console.ReadLine();

            // Find matching owners or pets
            var matchingAppointments = _appointmentTable
                .GetAllElements()
                .Where(a => petService
                            .GetPetHashTable()
                            .GetAllElements()
                            .Any(p => p.Name.Contains(search, StringComparison.OrdinalIgnoreCase) && p.PetId == a.PetId) ||
                            ownerService
                            .GetOwnerHashTable()
                            .GetAllElements()
                            .Any(o => o.Name.Contains(search, StringComparison.OrdinalIgnoreCase) && o.OwnerId == a.PetId))
                .ToList();

            if (matchingAppointments.Count == 0)
            {
                Console.WriteLine("No matching appointments found.");
                return false;
            }

            // Display the matching appointments
            Console.WriteLine("\nMatching Appointments:");
            foreach (var appointment in matchingAppointments)
            {
                Console.WriteLine($"Appointment ID: {appointment.AppointmentId}, Pet ID: {appointment.PetId}, Date: {appointment.AppointmentDate.ToShortDateString()}, Status: {appointment.Status}");
            }

            // Let the user select the appointment based on index
            Console.Write("\nEnter the Appointment ID from the above list: ");
            if (int.TryParse(Console.ReadLine(), out int appointmentId))
            {
                var appointmentToUpdate = matchingAppointments.FirstOrDefault(a => a.AppointmentId == appointmentId);

                if (appointmentToUpdate != null)
                {
                    // Valid status options
                    string[] validStatuses = { "Scheduled", "Completed", "Cancelled" };

                    // Prompt the user for the new status
                    Console.Write("Enter new Appointment Status (Scheduled, Completed, Cancelled): ");
                    string status = Console.ReadLine();

                    // Validate the status input
                    if (!validStatuses.Contains(status))
                    {
                        Console.WriteLine("Invalid status entered.");
                        return false;
                    }

                    // Update only the status
                    appointmentToUpdate.Status = status;
                    Console.WriteLine("Appointment status updated successfully.");
                    return true;
                }
            }

            return false;
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

        public bool UpdateAppointment(int appointmentId, Appointment updatedAppointment)
        {
            foreach (var appointment in _appointmentTable.GetAllElements())
            {
                if (appointment.AppointmentId == appointmentId)
                {
                    appointment.PetId = updatedAppointment.PetId;
                    appointment.AppointmentDate = updatedAppointment.AppointmentDate;
                    appointment.Type = updatedAppointment.Type;
                    appointment.Status = updatedAppointment.Status;
                    Console.WriteLine("Appointment updated successfully.");
                    return true;
                }
            }

            Console.WriteLine("Appointment not found.");
            return false;
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
            // Prompt user for the owner's name or email
            Console.Write("Enter part of the owner's name or email: ");
            string search = Console.ReadLine();

            // Find matching owners
            var matchingOwners = ownerService
                .GetOwnerHashTable()
                .GetAllElements()
                .Where(o => o.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                            o.Email.Contains(search, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (matchingOwners.Count == 0)
            {
                Console.WriteLine("No matching owners found.");
                return null;
            }

            // Display matching owners
            Console.WriteLine("\nMatching Owners:");
            foreach (var owner in matchingOwners)
            {
                Console.WriteLine($"ID: {owner.OwnerId}, Name: {owner.Name}, Email: {owner.Email}");
            }

            // Let the user select the owner from the matching list
            Console.Write("\nEnter the Owner ID from the above list: ");
            if (int.TryParse(Console.ReadLine(), out int ownerId))
            {
                // Find all pets for this owner
                var petsOwnedByOwner = petService
                    .GetPetHashTable()
                    .GetAllElements()
                    .Where(p => p.OwnerId == ownerId)
                    .ToList();

                if (petsOwnedByOwner.Count == 0)
                {
                    Console.WriteLine("This owner does not have any pets.");
                    return null;
                }

                // Display pets for the selected owner
                Console.WriteLine("\nMatching Pets:");
                foreach (var pet in petsOwnedByOwner)
                {
                    Console.WriteLine($"Pet ID: {pet.PetId}, Name: {pet.Name}, Breed: {pet.Breed}");
                }

                // Let the user select the pet
                Console.Write("\nEnter the Pet ID to update: ");
                if (int.TryParse(Console.ReadLine(), out int petId))
                {
                    var petToUpdate = petsOwnedByOwner.FirstOrDefault(p => p.PetId == petId);
                    return petToUpdate;
                }
            }

            return null;
        }
    }


}
