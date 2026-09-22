namespace MedicineMonitor.Domain.Entities;

public sealed class Box
{
    private Box()
    {
    }

    public Box(
        Guid userId,
        string boxId,
        string name)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        BoxId = boxId;
        Name = name;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string BoxId { get; private set; } =
        string.Empty;

    public string Name { get; private set; } =
        string.Empty;

    public DateTime CreatedAtUtc { get; private set; }

    public void Update(
        string boxId,
        string name)
    {
        BoxId = boxId;
        Name = name;
    }
}