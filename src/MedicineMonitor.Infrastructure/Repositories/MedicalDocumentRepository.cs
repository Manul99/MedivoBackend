using Google;
using MedicineMonitor.Domain.Entities;
using MedicineMonitor.Domain.Repositories;
using MedicineMonitor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MedicineMonitor.Infrastructure.Repositories;

public sealed class MedicalDocumentRepository
    : IMedicalDocumentRepository
{
    private readonly MedicineMonitorDbContext _dbContext;

    public MedicalDocumentRepository(
        MedicineMonitorDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<MedicalDocument>>
        GetByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken)
    {
        return await _dbContext.MedicalDocuments
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<MedicalDocument?> GetByIdAsync(
        Guid userId,
        Guid id,
        CancellationToken cancellationToken)
    {
        return await _dbContext.MedicalDocuments
            .FirstOrDefaultAsync(
                x =>
                    x.Id == id &&
                    x.UserId == userId,
                cancellationToken);
    }

    public async Task AddAsync(
        MedicalDocument document,
        CancellationToken cancellationToken)
    {
        await _dbContext.MedicalDocuments.AddAsync(
            document,
            cancellationToken);
    }

    public Task DeleteAsync(
        MedicalDocument document,
        CancellationToken cancellationToken)
    {
        _dbContext.MedicalDocuments.Remove(document);

        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(
            cancellationToken);
    }
}