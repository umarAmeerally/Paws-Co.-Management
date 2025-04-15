using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Cousework.Models;

public partial class PetCareContext : DbContext
{
    public PetCareContext()
    {
    }

    public PetCareContext(DbContextOptions<PetCareContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Appointment> Appointments { get; set; }

    public virtual DbSet<MedicalRecord> MedicalRecords { get; set; }

    public virtual DbSet<Owner> Owners { get; set; }

    public virtual DbSet<Pet> Pets { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)

        => optionsBuilder.UseSqlServer("Data Source=HP;Initial Catalog=Cousework;Integrated Security=True;Encrypt=True;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.AppointmentId).HasName("PK__appointm__A50828FCBC0E923B");

            entity.HasOne(d => d.Owner).WithMany(p => p.Appointments).HasConstraintName("FK__appointme__owner__4222D4EF");

            entity.HasOne(d => d.Pet).WithMany(p => p.Appointments)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__appointme__pet_i__412EB0B6");
        });

        modelBuilder.Entity<MedicalRecord>(entity =>
        {
            entity.HasKey(e => e.RecordId).HasName("PK__medical___BFCFB4DDDD3BD242");

            entity.Property(e => e.Date).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Pet).WithMany(p => p.MedicalRecords)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__medical_r__pet_i__45F365D3");
        });

        modelBuilder.Entity<Owner>(entity =>
        {
            entity.HasKey(e => e.OwnerId).HasName("PK__owners__3C4FBEE4AC0D81BF");
        });

        modelBuilder.Entity<Pet>(entity =>
        {
            entity.HasKey(e => e.PetId).HasName("PK__pets__390CC5FEB2008147");

            entity.Property(e => e.DateRegistered).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Owner).WithMany(p => p.Pets)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__pets__owner_id__3C69FB99");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
