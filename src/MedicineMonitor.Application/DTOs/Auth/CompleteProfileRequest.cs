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
}
