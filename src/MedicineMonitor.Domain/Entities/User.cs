namespace MedicineMonitor.Domain.Entities;

public sealed class User
{
    public Guid Id { get; private set; }

    public string FirebaseUid { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string FirstName { get; private set; } = string.Empty;

    public string LastName { get; private set; } = string.Empty;

    public string PhoneNumber { get; private set; } = string.Empty;

    public DateOnly? DateOfBirth { get; private set; }

    public string? BloodType { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    private User()
    {
    }

    public User(
        string firebaseUid,
        string email,
        string firstName,
        string lastName,
        string phoneNumber,
        DateOnly dateOfBirth,
        string bloodType)
    {
        Id = Guid.NewGuid();

        FirebaseUid = firebaseUid;
        Email = email;

        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;

        DateOfBirth = dateOfBirth;
        BloodType = bloodType;

        IsActive = true;

        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UpdateProfile(
        string firstName,
        string lastName,
        string phoneNumber,
        string email,
        DateOnly dateOfBirth,
        string bloodType)
    {
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        Email = email;

        DateOfBirth = dateOfBirth;
        BloodType = bloodType;

        UpdatedAtUtc = DateTime.UtcNow;
    }
}