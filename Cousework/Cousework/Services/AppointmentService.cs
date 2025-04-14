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

        public void AddAppointment(Appointment newAppointment)
        {
            _appointmentTable.Insert(newAppointment);
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

            Console.WriteLine($"Deleted {appointmentsToDelete.Count} appointments for Pet ID: {petId}");
        }

        public HashTable<Appointment> GetAppointmentHashTable() => _appointmentTable;
    }
}
