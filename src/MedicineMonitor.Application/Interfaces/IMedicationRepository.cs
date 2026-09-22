namespace MedicineMonitor.Application.Interfaces;

public interface IMedicationRepository
{
    Task SaveAsync(MedicineDocument medication, CancellationToken cancellationToken);
    Task<MedicineDocument?> GetAsync(string userId, string medicationId, CancellationToken cancellationToken);
    Task<IReadOnlyList<MedicineDocument>> GetForUserAsync(string userId, CancellationToken cancellationToken);
    Task DeactivateAsync(string userId,string medicationId,CancellationToken cancellationToken);
}


public sealed record MedicineDocument(
    string Id,
    string UserId,
    string BoxId,
    string MedicineName,
    IReadOnlyList<string> CompartmentIds,
    IReadOnlyList<ScheduleDocument> Schedules,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record ScheduleDocument(string Day, int Hour, int Minute);
