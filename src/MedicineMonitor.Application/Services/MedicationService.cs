using MedicineMonitor.Application.Abstractions;
using MedicineMonitor.Application.DTOs.Medications;
using MedicineMonitor.Application.Interfaces;

namespace MedicineMonitor.Application.Services;

public sealed class MedicationService(
    IMedicationRepository repository,
    ICurrentUser currentUser)
{
    public async Task<MedicationResponse> CreateAsync(
        CreateMedicationRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetRequiredUserId();

        var document = BuildDocument(
            Guid.NewGuid().ToString("N"),
            userId,
            request.MedicineName,
            request.CompartmentIds,
            request.Days,
            request.Hour,
            request.Minute,
            DateTime.UtcNow,
            DateTime.UtcNow);

        await repository.SaveAsync(document, cancellationToken);

        return ToResponse(document);
    }

    public async Task<IReadOnlyList<MedicationResponse>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        var userId = GetRequiredUserId();

        var documents = await repository.GetForUserAsync(
            userId,
            cancellationToken);

        return documents
            .Select(ToResponse)
            .ToList();
    }

    public async Task<MedicationResponse?> GetAsync(
        string id,
        CancellationToken cancellationToken)
    {
        var userId = GetRequiredUserId();

        var document = await repository.GetAsync(
            userId,
            id,
            cancellationToken);

        return document is null
            ? null
            : ToResponse(document);
    }

    public async Task<MedicationResponse?> UpdateAsync(
        string id,
        UpdateMedicationRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetRequiredUserId();

        var existing = await repository.GetAsync(
            userId,
            id,
            cancellationToken);

        if (existing is null)
            return null;

        var updated = BuildDocument(
            existing.Id,
            userId,
            request.MedicineName,
            request.CompartmentIds,
            request.Days,
            request.Hour,
            request.Minute,
            existing.CreatedAtUtc,
            DateTime.UtcNow);

        await repository.SaveAsync(
            updated,
            cancellationToken);

        return ToResponse(updated);
    }

    public async Task<bool> DeleteAsync(
        string id,
        CancellationToken cancellationToken)
    {
        var userId = GetRequiredUserId();

        var existing = await repository.GetAsync(
            userId,
            id,
            cancellationToken);

        if (existing is null)
            return false;

        await repository.DeleteAsync(
            userId,
            id,
            cancellationToken);

        return true;
    }

    private string GetRequiredUserId()
    {
        if (!currentUser.IsAuthenticated ||
            string.IsNullOrWhiteSpace(currentUser.UserId))
        {
            throw new UnauthorizedAccessException();
            //return "test-user-001";
        }

        return currentUser.UserId;
    }

    private static MedicineDocument BuildDocument(
        string id,
        string userId,
        string medicineName,
        IEnumerable<string> compartmentIds,
        IEnumerable<string> days,
        int hour,
        int minute,
        DateTime createdAtUtc,
        DateTime updatedAtUtc)
    {
        var normalizedCompartments = compartmentIds
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim().ToUpperInvariant())
            .Distinct()
            .ToList();

        if (normalizedCompartments.Count is < 1 or > 21)
        {
            throw new ArgumentException(
                "A medication must use between 1 and 21 compartments.");
        }

        if (normalizedCompartments.Any(
                x => !IsValidCompartment(x)))
        {
            throw new ArgumentException(
                "Compartment IDs must be C01 through C21.");
        }

        var normalizedDays = days
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim().ToUpperInvariant())
            .Distinct()
            .ToList();

        if (normalizedDays.Count == 0)
        {
            throw new ArgumentException(
                "At least one medication day is required.");
        }

        if (normalizedDays.Any(x => !IsValidDay(x)))
        {
            throw new ArgumentException(
                "Invalid medication day.");
        }

        if (hour is < 0 or > 23)
        {
            throw new ArgumentException(
                "Hour must be between 0 and 23.");
        }

        if (minute is < 0 or > 59)
        {
            throw new ArgumentException(
                "Minute must be between 0 and 59.");
        }

        var normalizedSchedules = normalizedDays
            .Select(day => new ScheduleDocument(
                day,
                hour,
                minute))
            .ToList();

        return new MedicineDocument(
            id,
            userId,
            medicineName.Trim(),
            normalizedCompartments,
            normalizedSchedules,
            true,
            createdAtUtc,
            updatedAtUtc);
    }

    private static bool IsValidCompartment(string value)
    {
        if (value.Length != 3 || value[0] != 'C')
            return false;

        return int.TryParse(
            value[1..],
            out var number) &&
            number is >= 1 and <= 21;
    }

    private static bool IsValidDay(string value)
    {
        return value switch
        {
            "MONDAY" => true,
            "TUESDAY" => true,
            "WEDNESDAY" => true,
            "THURSDAY" => true,
            "FRIDAY" => true,
            "SATURDAY" => true,
            "SUNDAY" => true,
            _ => false
        };
    }

    private static MedicationResponse ToResponse(
        MedicineDocument document)
    {
        return new MedicationResponse(
            document.Id,
            document.MedicineName,
            document.CompartmentIds,
            document.Schedules
                .Select(x => new MedicationScheduleResponse(
                    x.Day,
                    x.Hour,
                    x.Minute))
                .ToList(),
            document.IsActive,
            document.CreatedAtUtc,
            document.UpdatedAtUtc);
    }
}