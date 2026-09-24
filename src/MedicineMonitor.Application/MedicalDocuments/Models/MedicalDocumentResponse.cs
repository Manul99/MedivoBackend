using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicineMonitor.Application.MedicalDocuments.Models;

public sealed record MedicalDocumentResponse(
    Guid Id,
    string DocumentType,
    string DocumentName,
    string OriginalFileName,
    string ContentType,
    long FileSizeBytes,
    DateOnly? DocumentDate,
    string? Description,
    DateTime CreatedAtUtc);