using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicineMonitor.Application.Interfaces
{
    

    public interface IFirebaseRealtimeDatabaseService
    {
        Task SetAlarmAsync(
            string boxId,
            int compartmentId,
            FirebaseAlarmConfiguration alarm,
            CancellationToken cancellationToken);

        Task DisableAlarmAsync(
            string boxId,
            int compartmentId,
            CancellationToken cancellationToken);

        Task DeleteAlarmAsync(
            string boxId,
            int compartmentId,
            CancellationToken cancellationToken);
    }

    public sealed record FirebaseAlarmConfiguration(
        string Day,
        bool Enabled,
        string Time);
}
