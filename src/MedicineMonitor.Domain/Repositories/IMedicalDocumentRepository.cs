using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MedicineMonitor.Domain.Entities;

namespace MedicineMonitor.Domain.Repositories;

public interface IMedicalDocumentRepository
{
    Task<IReadOnlyList<MedicalDocument>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<MedicalDocument?> GetByIdAsync(
        Guid userId,
        Guid id,
        CancellationToken cancellationToken);

    Task AddAsync(
        MedicalDocument document,
        CancellationToken cancellationToken);

    Task DeleteAsync(
        MedicalDocument document,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(
        CancellationToken cancellationToken);
}
