using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicineMonitor.Application.Interfaces;

public interface ISupabaseStorageService
{
    Task UploadAsync(
        Stream fileStream,
        string storagePath,
        string contentType,
        CancellationToken cancellationToken);

    Task<string> CreateSignedUrlAsync(
        string storagePath,
        int expiresInSeconds,
        CancellationToken cancellationToken);

    Task DeleteAsync(
        string storagePath,
        CancellationToken cancellationToken);
}
