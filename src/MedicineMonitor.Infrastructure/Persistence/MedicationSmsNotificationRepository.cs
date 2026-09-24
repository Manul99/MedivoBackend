using MedicineMonitor.Application.Interfaces;
using MedicineMonitor.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedicineMonitor.Infrastructure.Persistence.Repositories;

public sealed class MedicationSmsNotificationRepository(
    MedicineMonitorDbContext db)
    : IMedicationSmsNotificationRepository
{
    public Task<bool> ExistsAsync(
        string medicationId,
        DateOnly scheduledDate,
        TimeOnly scheduledTime,
        CancellationToken cancellationToken)
    {
        return db.MedicationSmsNotifications
            .AnyAsync(
                x =>
                    x.MedicationId == medicationId &&
                    x.ScheduledDate == scheduledDate &&
                    x.ScheduledTime == scheduledTime,
                cancellationToken);
    }

    public async Task AddAsync(
        MedicationSmsNotification notification,
        CancellationToken cancellationToken)
    {
        await db.MedicationSmsNotifications.AddAsync(
            notification,
            cancellationToken);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken)
    {
        return db.SaveChangesAsync(
            cancellationToken);
    }
}