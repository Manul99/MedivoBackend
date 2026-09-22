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
        /*
         * ==========================================
         * MEDICINE NAME
         * ==========================================
         */

        if (string.IsNullOrWhiteSpace(MedicineName))
        {
            yield return new ValidationResult(
                "Medicine name is required.",
                [nameof(MedicineName)]);
        }

        /*
         * ==========================================
         * COMPARTMENTS
         * ==========================================
         */

        if (CompartmentIds.Count == 0)
        {
            yield return new ValidationResult(
                "At least one compartment must be selected.",
                [nameof(CompartmentIds)]);
        }

        var normalizedCompartments =
            CompartmentIds
                .Where(x =>
                    !string.IsNullOrWhiteSpace(x))
                .Select(x =>
                    x.Trim().ToUpperInvariant())
                .ToList();

        if (
            normalizedCompartments.Any(
                x => !IsValidCompartment(x))
        )
        {
            yield return new ValidationResult(
                "Compartment IDs must be C01 through C21.",
                [nameof(CompartmentIds)]);
        }

        if (
            normalizedCompartments.Count !=
            normalizedCompartments
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count()
        )
        {
            yield return new ValidationResult(
                "Compartment IDs cannot contain duplicates.",
                [nameof(CompartmentIds)]);
        }

        /*
         * ==========================================
         * DAYS
         * ==========================================
         */

        if (Days.Count == 0)
        {
            yield return new ValidationResult(
                "At least one day must be selected.",
                [nameof(Days)]);
        }

        var validDays =
            new HashSet<string>(
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

        var normalizedDays =
            Days
                .Where(x =>
                    !string.IsNullOrWhiteSpace(x))
                .Select(x =>
                    x.Trim().ToUpperInvariant())
                .ToList();

        if (
            normalizedDays.Any(
                x => !validDays.Contains(x))
        )
        {
            yield return new ValidationResult(
                "Days must contain valid weekdays.",
                [nameof(Days)]);
        }

        /*
         * ==========================================
         * TIME
         * ==========================================
         */

        if (Hour is < 0 or > 23)
        {
            yield return new ValidationResult(
                "Hour must be between 0 and 23.",
                [nameof(Hour)]);
        }

        if (Minute is < 0 or > 59)
        {
            yield return new ValidationResult(
                "Minute must be between 0 and 59.",
                [nameof(Minute)]);
        }
    }

    private static bool IsValidCompartment(
        string value)
    {
        if (
            value.Length != 3 ||
            value[0] != 'C'
        )
        {
            return false;
        }

        return int.TryParse(
            value[1..],
            out var number
        )
        && number is >= 1 and <= 21;
    }
}