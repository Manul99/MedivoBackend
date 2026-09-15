namespace MedicineMonitor.Domain.Entities;

public sealed class Box
{
    private Box() { }

    public Box(string boxId, string userId, string? name)
    {
        Id = Guid.NewGuid();
        BoxId = boxId.Trim();
        UserId = userId;
        Name = string.IsNullOrWhiteSpace(name) ? BoxId : name.Trim();
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public string BoxId { get; private set; } = string.Empty;
    public string UserId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public DateTime CreatedAtUtc { get; private set; }
}
