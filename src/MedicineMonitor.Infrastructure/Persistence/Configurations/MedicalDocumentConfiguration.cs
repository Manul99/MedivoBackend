using MedicineMonitor.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedicineMonitor.Infrastructure.Persistence.Configurations;

public sealed class MedicalDocumentConfiguration
    : IEntityTypeConfiguration<MedicalDocument>
{
    public void Configure(
        EntityTypeBuilder<MedicalDocument> builder)
    {
        builder.ToTable("medical_documents");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("Id");

        builder.Property(x => x.UserId)
            .HasColumnName("UserId")
            .IsRequired();

        builder.Property(x => x.DocumentType)
            .HasColumnName("DocumentType")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.DocumentName)
            .HasColumnName("DocumentName")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.OriginalFileName)
            .HasColumnName("OriginalFileName")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.StoragePath)
            .HasColumnName("StoragePath")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.ContentType)
            .HasColumnName("ContentType")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.FileSizeBytes)
            .HasColumnName("FileSizeBytes")
            .IsRequired();

        builder.Property(x => x.DocumentDate)
            .HasColumnName("DocumentDate");

        builder.Property(x => x.Description)
            .HasColumnName("Description")
            .HasMaxLength(1000);

        builder.Property(x => x.CreatedAtUtc)
            .HasColumnName("CreatedAtUtc")
            .IsRequired();

        builder.HasIndex(x => x.UserId);

        builder.HasIndex(x => new
        {
            x.UserId,
            x.CreatedAtUtc
        });
    }
}