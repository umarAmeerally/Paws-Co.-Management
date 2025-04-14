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
