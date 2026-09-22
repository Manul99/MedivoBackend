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
            CompartmentIds = new System.Collections.Generic.List<string> { "C01" },
            Days = new System.Collections.Generic.List<string> { "MONDAY" }
        };

        var results = request.Validate(new System.ComponentModel.DataAnnotations.ValidationContext(request)).ToList();
        Assert.Empty(results);
    }
}
