namespace MedicineMonitor.Application.DTOs.Auth;

public sealed record UserProfileResponse(
    Guid Id,
    string FirebaseUid,
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    DateOnly? DateOfBirth,
    string? BloodType,
    int? Age,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);