using MedicineMonitor.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedicineMonitor.Infrastructure.Persistence;

public sealed class MedicineMonitorDbContext(DbContextOptions<MedicineMonitorDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Box> Boxes => Set<Box>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FirebaseUid).HasMaxLength(128).IsRequired();
            entity.HasIndex(x => x.FirebaseUid).IsUnique();
            entity.Property(x => x.Email).HasMaxLength(320).IsRequired();
            entity.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.PhoneNumber).HasMaxLength(30);
            entity.Property(x => x.CreatedAtUtc).IsRequired();
            entity.Property(x => x.UpdatedAtUtc).IsRequired();
        });

        modelBuilder.Entity<Box>(entity =>
        {
            entity.ToTable("boxes");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.BoxId)
                .HasMaxLength(100)
                .IsRequired();

            entity.HasIndex(x => x.BoxId)
                .IsUnique();

            entity.Property(x => x.UserId)
                .IsRequired();

            entity.HasIndex(x => x.UserId);

            entity.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.CreatedAtUtc)
                .IsRequired();
        });
    }
}
