using System;
using System.Collections.Generic;
using BloodDonation.EF.Tables;
using Microsoft.EntityFrameworkCore;

namespace BloodDonation.EF;

public partial class BloodBankDbContext : DbContext
{
    public BloodBankDbContext()
    {
    }

    public BloodBankDbContext(DbContextOptions<BloodBankDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Donation> Donations { get; set; }

    public virtual DbSet<Donor> Donors { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("name=DbConn");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Donation>(entity =>
        {
            entity.ToTable("Donation");

            entity.Property(e => e.DonationId).HasColumnName("DonationID");
            entity.Property(e => e.CampName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DonorId).HasColumnName("DonorID");
            entity.Property(e => e.VolumeMl).HasColumnName("VolumeML");

            entity.HasOne(d => d.Donor).WithMany(p => p.Donations)
                .HasForeignKey(d => d.DonorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Donation_Donor");
        });

        modelBuilder.Entity<Donor>(entity =>
        {
            entity.ToTable("Donor");

            entity.Property(e => e.DonorId).HasColumnName("DonorID");
            entity.Property(e => e.BloodGroup)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.City)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ContactNo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FullName)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Role__8AFACE1AB451AD6D");

            entity.ToTable("Role");

            entity.HasIndex(e => e.RoleName, "UQ__Role__8A2B6160BA3AFE8E").IsUnique();

            entity.Property(e => e.RoleName)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User__1788CC4CCD4BE68B");

            entity.ToTable("User");

            entity.HasIndex(e => e.Username, "UQ__User__536C85E4BDD165B6").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__User__A9D10534DBC91861").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FullName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_User_Role");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
