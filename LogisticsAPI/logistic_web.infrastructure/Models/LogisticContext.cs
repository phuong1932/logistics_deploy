using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace logistic_web.infrastructure.Models;

public partial class LogisticContext : DbContext
{
    public LogisticContext()
    {
    }

    public LogisticContext(DbContextOptions<LogisticContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cargolist> Cargolists { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=connectionStringLogistic");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cargolist>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Cargolis__3213E83FB21C91A8");

            entity.ToTable("Cargolist");

            entity.HasIndex(e => e.CargoCode, "UQ_Cargolist_cargo_code").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AdvanceMoney)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("advance_money");
            entity.Property(e => e.CargoCode)
                .HasMaxLength(255)
                .HasColumnName("cargo_code");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.CustomerAddress)
                .HasMaxLength(255)
                .HasColumnName("customer_address");
            entity.Property(e => e.CustomerCompanyName)
                .HasMaxLength(255)
                .HasColumnName("customer_company_name");
            entity.Property(e => e.CustomerPersonInCharge)
                .HasMaxLength(255)
                .HasColumnName("customer_person_in_charge");
            entity.Property(e => e.EmployeeCreate)
                .HasMaxLength(255)
                .HasColumnName("employee_create");
            entity.Property(e => e.EstimatedTotalAmount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("estimated_total_amount");
            entity.Property(e => e.ExchangeDate).HasColumnName("exchange_date");
            entity.Property(e => e.FilePathJson)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("file_path_json");
            entity.Property(e => e.LicenseDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("license_date");
            entity.Property(e => e.QuantityOfShipper).HasColumnName("quantity_of_shipper");
            entity.Property(e => e.ServiceType)
                .HasDefaultValue((byte)0)
                .HasColumnName("service_type");
            entity.Property(e => e.ShippingFee)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("shipping_fee");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("PK__tmp_ms_x__A4AE64B8F1312569");

            entity.ToTable("Customer");

            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.CustomerName).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.PersonInCharge).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Roles__3214EC077464FFB1");

            entity.HasIndex(e => e.RoleName, "UQ__Roles__8A2B61601F9090D2").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Deleted).HasDefaultValue(false);
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC0742CCEA7D");

            entity.HasIndex(e => e.Username, "UQ__Users__536C85E4FDBCF516").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Users__A9D1053448AF9F0E").IsUnique();

            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Deleted).HasDefaultValue(false);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(255);
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId });

            entity.ToTable("UserRole");

            entity.Property(e => e.Description).HasMaxLength(255);

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserRole_Roles");

            entity.HasOne(d => d.User).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserRole_Users");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
