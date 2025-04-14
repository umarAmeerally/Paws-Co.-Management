using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Cousework.Models;

[Table("owners")]
[Index("Email", Name = "UQ__owners__AB6E61641CC4A2DE", IsUnique = true)]
public partial class Owner
{
    [Key]
    [Column("owner_id")]
    public int OwnerId { get; set; }

    [Column("name")]
    [StringLength(100)]
    [Unicode(false)]
    public string Name { get; set; } = null!;

    [Column("email")]
    [StringLength(100)]
    [Unicode(false)]
    public string Email { get; set; } = null!;

    [Column("phone")]
    [StringLength(15)]
    [Unicode(false)]
    public string? Phone { get; set; }

    [Column("address", TypeName = "text")]
    public string? Address { get; set; }

    [InverseProperty("Owner")]
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    [InverseProperty("Owner")]
    public virtual ICollection<Pet> Pets { get; set; } = new List<Pet>();

    public override string ToString()
    {
        return $"OwnerId: {OwnerId}, Name: {Name}, Email: {Email}, Phone: {Phone}";
    }
}


