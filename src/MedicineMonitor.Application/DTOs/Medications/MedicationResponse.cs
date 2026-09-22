namespace MedicineMonitor.Application.DTOs.Medications;

public sealed record MedicationResponse(
    string Id,
    string MedicineName,
    IReadOnlyList<string> CompartmentIds,
    IReadOnlyList<MedicationScheduleResponse> Schedules,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record MedicationScheduleResponse(
    string Day,
    string Time);