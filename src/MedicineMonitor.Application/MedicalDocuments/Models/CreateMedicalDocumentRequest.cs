using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicineMonitor.Application.MedicalDocuments.Models
{
    public sealed record CreateMedicalDocumentRequest(
    string DocumentType,
    string DocumentName,
    DateOnly? DocumentDate,
    string? Description);
}
