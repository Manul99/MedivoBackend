namespace MedicineMonitor.Application.DTOs.Auth;


public sealed record UpdateProfileRequest(
    string FirstName,
    string LastName,
    string PhoneNumber,
    DateOnly DateOfBirth,
    string BloodType);
