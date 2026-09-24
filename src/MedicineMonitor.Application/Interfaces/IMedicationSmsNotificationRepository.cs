using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MedicineMonitor.Domain.Entities;

namespace MedicineMonitor.Application.Interfaces;

public interface IMedicationSmsNotificationRepository
{
    Task<bool> ExistsAsync(
        string medicationId,
        DateOnly scheduledDate,
        TimeOnly scheduledTime,
        CancellationToken cancellationToken);

    Task AddAsync(
        MedicationSmsNotification notification,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(
        CancellationToken cancellationToken);
}
