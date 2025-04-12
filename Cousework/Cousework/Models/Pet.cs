using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Cousework.Models;

[Table("pets")]
public partial class Pet
{
    [Key]
    [Column("pet_id")]
    public int PetId { get; set; }

    [Column("name")]
    [StringLength(100)]
    [Unicode(false)]
    public string Name { get; set; } = null!;

    [Column("species")]
    [StringLength(50)]
    [Unicode(false)]
    public string Species { get; set; } = null!;

    [Column("breed")]
    [StringLength(100)]
    [Unicode(false)]
    public string? Breed { get; set; }

    [Column("age")]
    public int? Age { get; set; }

    [Column("gender")]
    [StringLength(10)]
    [Unicode(false)]
    public string? Gender { get; set; }

    [Column("owner_id")]
    public int? OwnerId { get; set; }

    [Column("medical_history", TypeName = "text")]
    public string? MedicalHistory { get; set; }

    [Column("date_registered", TypeName = "datetime")]
    public DateTime? DateRegistered { get; set; }

    [InverseProperty("Pet")]
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    [InverseProperty("Pet")]
    public virtual ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();

    [ForeignKey("OwnerId")]
    [InverseProperty("Pets")]
    public virtual Owner? Owner { get; set; }

    public override string ToString()
    {
        return $"PetId: {PetId}, Name: {Name}, Species: {Species}, Breed: {Breed}, Age: {Age}, OwnerId: {OwnerId}";
    }

}
