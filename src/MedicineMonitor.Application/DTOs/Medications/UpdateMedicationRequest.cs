using System.ComponentModel.DataAnnotations;

namespace MedicineMonitor.Application.DTOs.Medications;

public sealed class UpdateMedicationRequest : IValidatableObject
{
    [Required]
    [MaxLength(200)]
    public string MedicineName { get; init; } = string.Empty;

    [Required]
    [MinLength(1)]
    [MaxLength(21)]
    public List<string> CompartmentIds { get; init; } = [];

    [Required]
    [MinLength(1)]
    public List<string> Days { get; init; } = [];

    [Range(0, 23)]
    public int Hour { get; init; }

    [Range(0, 59)]
    public int Minute { get; init; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(MedicineName))
        {
            yield return new ValidationResult(
                "Medicine name is required.",
                [nameof(MedicineName)]);
        }

        if (CompartmentIds.Any(string.IsNullOrWhiteSpace))
        {
            yield return new ValidationResult(
                "Compartment IDs cannot be empty.",
                [nameof(CompartmentIds)]);
        }

        if (Days.Count == 0)
        {
            yield return new ValidationResult(
                "At least one day must be selected.",
                [nameof(Days)]);
        }

        var validDays = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase)
        {
            "MONDAY",
            "TUESDAY",
            "WEDNESDAY",
            "THURSDAY",
            "FRIDAY",
            "SATURDAY",
            "SUNDAY"
        };

        if (Days.Any(day => !validDays.Contains(day)))
        {
            yield return new ValidationResult(
                "Days must contain valid weekdays.",
                [nameof(Days)]);
        }
    }
}