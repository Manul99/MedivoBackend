using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicineMonitor.Application.MedicationHistory.Models;

public sealed record MedicationHistoryResponse(
    string Id,
    string Date,
    string Time,
    string BoxId,
    int SlotNumber,
    string CompartmentId,
    string MedicineName,
    bool MedicineMatched);
