using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MedicineMonitor.Application.MedicalDocuments.Models;

namespace MedicineMonitor.Application.MedicalDocuments;

public interface IMedicalDocumentService
{
    Task<IReadOnlyList<MedicalDocumentResponse>>
        GetAllAsync(
            CancellationToken cancellationToken);

    Task<MedicalDocumentResponse>
        UploadAsync(
            CreateMedicalDocumentRequest request,
            Stream fileStream,
            string originalFileName,
            string contentType,
            long fileSizeBytes,
            CancellationToken cancellationToken);

    Task<string?>
        GetViewUrlAsync(
            Guid id,
            CancellationToken cancellationToken);

    Task<bool>
        DeleteAsync(
            Guid id,
            CancellationToken cancellationToken);
}
