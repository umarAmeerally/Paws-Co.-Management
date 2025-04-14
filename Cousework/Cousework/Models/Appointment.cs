using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Cousework.Models;

[Table("appointments")]
public partial class Appointment
{
    [Key]
    [Column("appointment_id")]
    public int AppointmentId { get; set; }

    [Column("pet_id")]
    public int? PetId { get; set; }

    [Column("owner_id")]
    public int? OwnerId { get; set; }

    [Column("appointment_date", TypeName = "datetime")]
    public DateTime AppointmentDate { get; set; }

    [Column("type")]
    [StringLength(20)]
    [Unicode(false)]
    public string? Type { get; set; }

    [Column("status")]
    [StringLength(20)]
    [Unicode(false)]
    public string? Status { get; set; }

    [ForeignKey("OwnerId")]
    [InverseProperty("Appointments")]
    public virtual Owner? Owner { get; set; }

    [ForeignKey("PetId")]
    [InverseProperty("Appointments")]
    public virtual Pet? Pet { get; set; }

    public override string ToString()
    {
        return $"AppointmentId: {AppointmentId}, Date: {AppointmentDate}, Type: {Type}, Status: {Status}, PetId: {PetId}";
    }

}
