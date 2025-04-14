﻿using Cousework.DataStructures;
using Cousework.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Cousework
{
    public static class DatabaseHelper
    {
        public static void SaveOwnersToDatabase(HashTable<Owner> ownerTable, string connectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<PetCareContext>();
            optionsBuilder.UseSqlServer(connectionString);

            using (var context = new PetCareContext(optionsBuilder.Options))
            {
                foreach (var owner in ownerTable.GetAllElements())
                {
                    Console.WriteLine($"Processing owner: {owner.OwnerId}, {owner.Name}, {owner.Email}");

                    var existingOwner = context.Owners.FirstOrDefault(o => o.OwnerId == owner.OwnerId);

                    if (existingOwner == null)
                    {
                        context.Owners.Add(owner);
                        Console.WriteLine("Adding new owner.");
                    }
                    else
                    {
                        existingOwner.Name = owner.Name;
                        existingOwner.Email = owner.Email;
                        existingOwner.Phone = owner.Phone;
                        Console.WriteLine("Updating existing owner.");
                    }
                }

                context.SaveChanges();
                Console.WriteLine("Owners successfully saved to the database.");
            }
        }

        public static void SavePetsToDatabase(HashTable<Pet> petTable, string connectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<PetCareContext>();
            optionsBuilder.UseSqlServer(connectionString);

            using (var context = new PetCareContext(optionsBuilder.Options))
            {
                foreach (var pet in petTable.GetAllElements())
                {
                    Console.WriteLine($"Processing pet: {pet.PetId}, {pet.Name}, OwnerId: {pet.OwnerId}");

                    // Only add if the owner exists
                    var ownerExists = context.Owners.Any(o => o.OwnerId == pet.OwnerId);
                    if (!ownerExists)
                    {
                        Console.WriteLine($"Skipping pet {pet.Name} as OwnerId {pet.OwnerId} does not exist in DB.");
                        continue;
                    }

                    var existingPet = context.Pets.FirstOrDefault(p => p.PetId == pet.PetId);

                    if (existingPet == null)
                    {
                        context.Pets.Add(pet);
                        Console.WriteLine("Adding new pet.");
                    }
                    else
                    {
                        existingPet.Name = pet.Name;
                        existingPet.Species = pet.Species;
                        existingPet.Breed = pet.Breed;
                        existingPet.Age = pet.Age;
                        existingPet.Gender = pet.Gender;
                        existingPet.MedicalHistory = pet.MedicalHistory;
                        existingPet.DateRegistered = pet.DateRegistered;

                        Console.WriteLine("Updating existing pet.");
                    }
                }

                context.SaveChanges();
                Console.WriteLine("Pets successfully saved to the database.");
            }
        }

        public static void SaveAppointmentsToDatabase(HashTable<Appointment> appointmentTable, string connectionString)
        {
            using (var context = new PetCareContext())
            {
                var appointments = appointmentTable.GetAllElements();

                foreach (var appointment in appointments)
                {
                    // Only add if not already in DB (based on unique AppointmentId)
                    bool exists = context.Appointments.Any(a => a.AppointmentId == appointment.AppointmentId);
                    if (!exists)
                    {
                        context.Appointments.Add(appointment);
                    }
                }

                context.SaveChanges();
            }
        }

        public static bool DeleteOwnerAndPetsFromDatabase(int ownerId, PetCareContext context)
        {
            var owner = context.Owners.Include(o => o.Pets).FirstOrDefault(o => o.OwnerId == ownerId);

            if (owner == null)
            {
                Console.WriteLine($"Owner with ID {ownerId} not found in database.");
                return false;
            }

            // Delete related pets first (if any)
            var relatedPets = context.Pets.Where(p => p.OwnerId == ownerId).ToList();
            if (relatedPets.Any())
            {
                context.Pets.RemoveRange(relatedPets);
                Console.WriteLine($"Deleted {relatedPets.Count} pet(s) linked to owner.");
            }

            // Now delete the owner
            context.Owners.Remove(owner);
            context.SaveChanges();
            Console.WriteLine($"Owner {owner.Name} (ID: {ownerId}) deleted from database.");
            return true;
        }

        public static bool DeletePetAndAppointmentsFromDatabase(int petId, PetCareContext context)
        {
            var pet = context.Pets.FirstOrDefault(p => p.PetId == petId);

            if (pet == null)
            {
                Console.WriteLine($"Pet with ID {petId} not found in database.");
                return false;
            }

            // Delete related appointments
            var relatedAppointments = context.Appointments.Where(a => a.PetId == petId).ToList();
            if (relatedAppointments.Any())
            {
                context.Appointments.RemoveRange(relatedAppointments);
                Console.WriteLine($"Deleted {relatedAppointments.Count} appointment(s) linked to pet.");
            }

            // Now delete the pet
            context.Pets.Remove(pet);
            context.SaveChanges();
            Console.WriteLine($"Pet {pet.Name} (ID: {petId}) deleted from database.");
            return true;
        }


    }
}
