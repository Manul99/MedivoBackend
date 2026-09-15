namespace MedicineMonitor.Domain.Entities;

public sealed class Medication
{
    private Medication() { }

    public Medication(
        string userId,
        string medicineName,
        string? notes,
        string boxId,
        IEnumerable<string> compartmentIds,
        IEnumerable<MedicationSchedule> schedules)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        MedicineName = medicineName.Trim();
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        BoxId = boxId.Trim();
        CompartmentIds = compartmentIds.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        Schedules = schedules.ToList();
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public string MedicineName { get; private set; } = string.Empty;
    public string? Notes { get; private set; }
    public string BoxId { get; private set; } = string.Empty;
    public List<string> CompartmentIds { get; private set; } = [];
    public List<MedicationSchedule> Schedules { get; private set; } = [];
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public void Update(
        string medicineName,
        string? notes,
        string boxId,
        IEnumerable<string> compartmentIds,
        IEnumerable<MedicationSchedule> schedules)
    {
        MedicineName = medicineName.Trim();
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        BoxId = boxId.Trim();
        CompartmentIds = compartmentIds.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        Schedules = schedules.ToList();
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
