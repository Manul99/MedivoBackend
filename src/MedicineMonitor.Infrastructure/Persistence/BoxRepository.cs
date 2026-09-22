using MedicineMonitor.Application.Interfaces;
using MedicineMonitor.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedicineMonitor.Infrastructure.Persistence;

public sealed class BoxRepository(
    MedicineMonitorDbContext db)
    : IBoxRepository
{
    public Task<Box?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return db.Boxes
            .SingleOrDefaultAsync(
                x => x.UserId == userId,
                cancellationToken);
    }

    public Task<Box?> GetByBoxIdAsync(
        string boxId,
        CancellationToken cancellationToken)
    {
        return db.Boxes
            .SingleOrDefaultAsync(
                x => x.BoxId == boxId,
                cancellationToken);
    }

    public async Task AddAsync(
        Box box,
        CancellationToken cancellationToken)
    {
        await db.Boxes.AddAsync(
            box,
            cancellationToken);
    }
}