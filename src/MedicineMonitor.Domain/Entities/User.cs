namespace MedicineMonitor.Domain.Entities;

public sealed class User
{
    private User() { }

    public User(string firebaseUid, string email, string firstName, string lastName, string? phoneNumber)
    {
        Id = Guid.NewGuid();
        FirebaseUid = firebaseUid;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
        IsActive = true;
    }

    public Guid Id { get; private set; }
    public string FirebaseUid { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string? PhoneNumber { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public void UpdateProfile(string firstName, string lastName, string? phoneNumber, string email)
    {
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        Email = email;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
