using System.ComponentModel.DataAnnotations;

namespace MedicineMonitor.Application.DTOs.Auth;

public sealed class CompleteProfileRequest
{
    [Required, MaxLength(100)]
    public string FirstName { get; init; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; init; } = string.Empty;

    [MaxLength(30)]
    public string? PhoneNumber { get; init; }

    [Required]
    public DateOnly DateOfBirth { get; init; }

    [Required, MaxLength(10)]
    public string BloodType { get; init; } = string.Empty;

    [Required]
    public string BoxId { get; init; } = string.Empty;
}