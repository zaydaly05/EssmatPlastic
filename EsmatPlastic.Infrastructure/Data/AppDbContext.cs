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

    public DbSet<OrderRequest> OrderRequests => Set<OrderRequest>();

    public DbSet<OrderRequestItem> OrderRequestItems => Set<OrderRequestItem>();

    public DbSet<Permission> Permissions => Set<Permission>();

    public DbSet<UserPermission> UserPermissions => Set<UserPermission>();

    public DbSet<DeletedRecord> DeletedRecords => Set<DeletedRecord>();

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        StampSyncTimestamps();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        StampSyncTimestamps();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void StampSyncTimestamps()
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<ISyncTimestamped>())
        {
            if (entry.State == EntityState.Added && entry.Entity.UpdatedAt == default)
            {
                entry.Entity.UpdatedAt = now;
            }

            if (entry.State == EntityState.Added && entry.Entity.SyncId == Guid.Empty)
            {
                entry.Entity.SyncId = Guid.NewGuid();
            }
            else if (entry.State == EntityState.Modified &&
                     !entry.Property(nameof(ISyncTimestamped.UpdatedAt)).IsModified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }
    }

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

            entity.Property(x => x.ReservedQuantity)
                .HasDefaultValue(0);

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

        modelBuilder.Entity<OrderRequest>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CustomerName).IsRequired().HasMaxLength(200);
            entity.Property(x => x.CustomerPhone).HasMaxLength(50);
            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrderRequestItem>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasOne(x => x.OrderRequest)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.OrderRequestId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.ProductVariant)
                .WithMany()
                .HasForeignKey(x => x.ProductVariantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                     .Where(type => typeof(ISyncTimestamped).IsAssignableFrom(type.ClrType)))
        {
            modelBuilder.Entity(entityType.ClrType)
                .HasIndex(nameof(ISyncTimestamped.SyncId))
                .IsUnique();
        }
    }
}




