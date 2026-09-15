using MedicineMonitor.Application.DTOs.Medications;
using Xunit;

namespace MedicineMonitor.Application.Tests;

public sealed class MedicationValidationTests
{
    [Xunit.Fact]
    public void Request_accepts_c01()
    {
        var request = new CreateMedicationRequest
        {
            MedicineName = "Paracetamol",
           // BoxId = "BOX001",
            CompartmentIds = ["C01"],
           // Schedules = [new MedicationScheduleRequest { Day = 1, Hour = 8, Minute = 0 }]
        };

        var results = request.Validate(new System.ComponentModel.DataAnnotations.ValidationContext(request)).ToList();
        Assert.Empty(results);
    }
}
