using MedicineMonitor.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedicineMonitor.Infrastructure.Persistence;

public sealed class MedicineMonitorDbContext(DbContextOptions<MedicineMonitorDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Box> Boxes => Set<Box>();
    public DbSet<MedicationSmsNotification>
    MedicationSmsNotifications =>
    Set<MedicationSmsNotification>();
    public DbSet<MedicalDocument> MedicalDocuments =>Set<MedicalDocument>();

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

        modelBuilder.Entity<MedicationSmsNotification>(
    entity =>
    {
        entity.ToTable(
            "medication_sms_notifications");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.MedicationId)
            .HasMaxLength(100)
            .IsRequired();

        entity.Property(x => x.UserId)
            .IsRequired();

        entity.Property(x => x.ScheduledDate)
            .HasColumnType("date")
            .IsRequired();

        entity.Property(x => x.ScheduledTime)
            .HasColumnType("time")
            .IsRequired();

        entity.Property(x => x.PhoneNumber)
            .HasMaxLength(30)
            .IsRequired();

        entity.Property(x => x.Status)
            .HasMaxLength(30)
            .IsRequired();

        entity.Property(x => x.TextLkMessageId)
            .HasMaxLength(200);

        entity.Property(x => x.SentAtUtc);

        entity.Property(x => x.CreatedAtUtc)
            .IsRequired();

        entity.HasIndex(x => new
        {
            x.MedicationId,
            x.ScheduledDate,
            x.ScheduledTime
        })
        .IsUnique();
    });

        modelBuilder.Entity<MedicalDocument>(
                entity =>
                {
                    entity.ToTable("medical_documents");

                    entity.HasKey(x => x.Id);

                    entity.Property(x => x.UserId)
                        .IsRequired();

                    entity.Property(x => x.DocumentType)
                        .HasMaxLength(50)
                        .IsRequired();

                    entity.Property(x => x.DocumentName)
                        .HasMaxLength(200)
                        .IsRequired();

                    entity.Property(x => x.OriginalFileName)
                        .HasMaxLength(255)
                        .IsRequired();

                    entity.Property(x => x.StoragePath)
                        .HasMaxLength(500)
                        .IsRequired();

                    entity.Property(x => x.ContentType)
                        .HasMaxLength(100)
                        .IsRequired();

                    entity.Property(x => x.FileSizeBytes)
                        .IsRequired();

                    entity.Property(x => x.DocumentDate)
                        .HasColumnType("date");

                    entity.Property(x => x.Description)
                        .HasMaxLength(1000);

                    entity.Property(x => x.CreatedAtUtc)
                        .IsRequired();

                    entity.HasIndex(x => x.UserId);

                    entity.HasIndex(x => new
                    {
                        x.UserId,
                        x.CreatedAtUtc
                    });
                });
    }
}
