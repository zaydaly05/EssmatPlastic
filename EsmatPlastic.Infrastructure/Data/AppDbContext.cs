using EsmatPlastic.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EsmatPlastic.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();

    public DbSet<StockTransaction> StockTransactions => Set<StockTransaction>();

    public DbSet<Permission> Permissions => Set<Permission>();

    public DbSet<UserPermission> UserPermissions => Set<UserPermission>();

    public DbSet<DeletedRecord> DeletedRecords => Set<DeletedRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Username)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasIndex(x => x.Username)
                .IsUnique();

            entity.Property(x => x.PasswordHash)
                .IsRequired();

            entity.Property(x => x.FullName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(x => x.Role)
                .IsRequired();
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(x => x.Description)
                .HasMaxLength(1000);
        });

        modelBuilder.Entity<ProductVariant>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(x => x.Size)
                .HasMaxLength(100);

            entity.Property(x => x.Color)
                .HasMaxLength(100);

            entity.Property(x => x.CapType)
                .HasMaxLength(100);

            entity.Property(x => x.Material)
                .HasMaxLength(100);

            entity.HasOne(x => x.Product)
                .WithMany(x => x.Variants)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<StockTransaction>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Quantity)
                .HasPrecision(18, 3);

            entity.Property(x => x.Type)
                .IsRequired();

            entity.Property(x => x.Notes)
                .HasMaxLength(1000);

            entity.HasOne(x => x.ProductVariant)
                .WithMany()
                .HasForeignKey(x => x.ProductVariantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.User)
                .WithMany(x => x.StockTransactions)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasIndex(x => x.Name)
                .IsUnique();

            entity.Property(x => x.Description)
                .HasMaxLength(500);
        });

        modelBuilder.Entity<UserPermission>(entity =>
        {
            entity.HasKey(x => new
            {
                x.UserId,
                x.PermissionId
            });

            entity.HasOne(x => x.User)
                .WithMany(x => x.UserPermissions)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Permission)
                .WithMany(x => x.UserPermissions)
                .HasForeignKey(x => x.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DeletedRecord>(entity =>
        {
            entity.HasKey(x => new { x.EntityType, x.RecordKey });
            entity.Property(x => x.EntityType).HasMaxLength(100).IsRequired();
            entity.Property(x => x.RecordKey).HasMaxLength(500).IsRequired();
            entity.Property(x => x.DeletedAt).IsRequired();
        });
    }
}




