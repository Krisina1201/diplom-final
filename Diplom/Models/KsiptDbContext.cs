using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Diplom.Models;

public partial class KsiptDbContext : DbContext
{
    public KsiptDbContext()
    {
    }

    public KsiptDbContext(DbContextOptions<KsiptDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Classroom> Classrooms { get; set; }

    public virtual DbSet<Inventory> Inventories { get; set; }

    public virtual DbSet<InventoryType> InventoryTypes { get; set; }

    public virtual DbSet<ResponsiblePerson> ResponsiblePersons { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Database=ksipt_db;Username=ksipt_user;Password=1234;Host=83.166.246.113");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Classroom>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("classrooms_pkey");

            entity.ToTable("classrooms");

            entity.HasIndex(e => e.QrCodeData, "classrooms_qr_code_data_key").IsUnique();

            entity.HasIndex(e => e.RoomNumber, "classrooms_room_number_key").IsUnique();

            entity.HasIndex(e => e.IsActive, "idx_classrooms_is_active");

            entity.HasIndex(e => e.ResponsibleId, "idx_classrooms_responsible_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Capacity).HasColumnName("capacity");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.QrCodeData)
                .HasMaxLength(50)
                .HasColumnName("qr_code_data");
            entity.Property(e => e.ResponsibleId).HasColumnName("responsible_id");
            entity.Property(e => e.RoomName)
                .HasMaxLength(100)
                .HasColumnName("room_name");
            entity.Property(e => e.RoomNumber)
                .HasMaxLength(10)
                .HasColumnName("room_number");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Responsible).WithMany(p => p.Classrooms)
                .HasForeignKey(d => d.ResponsibleId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("classrooms_responsible_id_fkey");
        });

        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("inventory_pkey");

            entity.ToTable("inventory");

            entity.HasIndex(e => e.ClassroomId, "idx_inventory_classroom_id");

            entity.HasIndex(e => e.ItemType, "idx_inventory_item_type");

            entity.HasIndex(e => e.PurchaseDate, "idx_inventory_purchase_date");

            entity.HasIndex(e => e.InventoryNumber, "inventory_inventory_number_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClassroomId).HasColumnName("classroom_id");
            entity.Property(e => e.ConditionDescription).HasColumnName("condition_description");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.InventoryNumber).HasColumnName("inventory_number");
            entity.Property(e => e.ItemName)
                .HasMaxLength(100)
                .HasColumnName("item_name");
            entity.Property(e => e.ItemType).HasColumnName("item_type");
            entity.Property(e => e.Manufacturer)
                .HasMaxLength(100)
                .HasColumnName("manufacturer");
            entity.Property(e => e.Model)
                .HasMaxLength(100)
                .HasColumnName("model");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.PurchaseDate).HasColumnName("purchase_date");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.WarrantyUntil).HasColumnName("warranty_until");

            entity.HasOne(d => d.Classroom).WithMany(p => p.Inventories)
                .HasForeignKey(d => d.ClassroomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inventory_classroom_id_fkey");

            entity.HasOne(d => d.ItemTypeNavigation).WithMany(p => p.Inventories)
                .HasForeignKey(d => d.ItemType)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("inventory_item_type_fk");
        });

        modelBuilder.Entity<InventoryType>(entity =>
        {
            entity.HasKey(e => e.InventoryTypeId).HasName("inventory_type_pkey");

            entity.ToTable("inventory_type");

            entity.Property(e => e.InventoryTypeId).HasColumnName("inventory_type_id");
            entity.Property(e => e.InventoryTypeTitle)
                .HasMaxLength(255)
                .HasColumnName("inventory_type_title");
        });

        modelBuilder.Entity<ResponsiblePerson>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("responsible_persons_pkey");

            entity.ToTable("responsible_persons");

            entity.HasIndex(e => new { e.LastName, e.FirstName, e.MiddleName }, "idx_responsible_persons_full_name");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .HasColumnName("first_name");
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .HasColumnName("last_name");
            entity.Property(e => e.MiddleName)
                .HasMaxLength(50)
                .HasColumnName("middle_name");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.Position)
                .HasMaxLength(100)
                .HasColumnName("position");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
