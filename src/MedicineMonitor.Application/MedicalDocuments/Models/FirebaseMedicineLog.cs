using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicineMonitor.Application.MedicalDocuments.Models
{
    public sealed record FirebaseMedicineLog(
     string Id,
     string Date,
     string MacAddress,
     int SlotNumber,
     string Time);
}
