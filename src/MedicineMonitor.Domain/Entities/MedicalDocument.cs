using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicineMonitor.Domain.Entities;

public sealed class MedicalDocument
{
    private MedicalDocument()
    {
    }

    public MedicalDocument(
        Guid userId,
        string documentType,
        string documentName,
        string originalFileName,
        string storagePath,
        string contentType,
        long fileSizeBytes,
        DateOnly? documentDate,
        string? description)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        DocumentType = documentType;
        DocumentName = documentName;
        OriginalFileName = originalFileName;
        StoragePath = storagePath;
        ContentType = contentType;
        FileSizeBytes = fileSizeBytes;
        DocumentDate = documentDate;
        Description = description;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string DocumentType { get; private set; } = string.Empty;

    public string DocumentName { get; private set; } = string.Empty;

    public string OriginalFileName { get; private set; } = string.Empty;

    public string StoragePath { get; private set; } = string.Empty;

    public string ContentType { get; private set; } = string.Empty;

    public long FileSizeBytes { get; private set; }

    public DateOnly? DocumentDate { get; private set; }

    public string? Description { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
}
