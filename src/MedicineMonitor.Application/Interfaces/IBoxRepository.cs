using MedicineMonitor.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MedicineMonitor.Domain.Entities;

namespace MedicineMonitor.Application.Interfaces;

public interface IBoxRepository
{
    Task<Box?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<Box?> GetByBoxIdAsync(
        string boxId,
        CancellationToken cancellationToken);

    Task AddAsync(
        Box box,
        CancellationToken cancellationToken);
}
