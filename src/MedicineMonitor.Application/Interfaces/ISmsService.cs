using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicineMonitor.Application.Interfaces
{
    public interface ISmsService
    {
      Task<string?> SendMedicationReminderAsync(
       string phoneNumber,
       string medicineName,
       string scheduledTime,
       CancellationToken cancellationToken);
    }
}
