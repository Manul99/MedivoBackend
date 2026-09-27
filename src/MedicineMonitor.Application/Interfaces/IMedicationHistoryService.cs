using MedicineMonitor.Application.MedicationHistory.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicineMonitor.Application.Interfaces
{
    public interface IMedicationHistoryService
    {
        Task<IReadOnlyList<MedicationHistoryResponse>>
            GetHistoryAsync(
                DateOnly fromDate,
                DateOnly toDate,
                CancellationToken cancellationToken);
    }
}
