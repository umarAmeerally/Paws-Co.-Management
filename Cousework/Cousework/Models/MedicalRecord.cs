using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Cousework.Models;

[Table("medical_records")]
public partial class MedicalRecord
{
    [Key]
    [Column("record_id")]
    public int RecordId { get; set; }

    [Column("pet_id")]
    public int? PetId { get; set; }

    [Column("diagnosis", TypeName = "text")]
    public string? Diagnosis { get; set; }

    [Column("treatment", TypeName = "text")]
    public string? Treatment { get; set; }

    [Column("vet_name")]
    [StringLength(100)]
    [Unicode(false)]
    public string? VetName { get; set; }

    [Column("date", TypeName = "datetime")]
    public DateTime? Date { get; set; }

    [ForeignKey("PetId")]
    [InverseProperty("MedicalRecords")]
    public virtual Pet? Pet { get; set; }
}
