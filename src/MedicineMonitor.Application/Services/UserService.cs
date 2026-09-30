using MedicineMonitor.Application.Abstractions;
using MedicineMonitor.Application.DTOs.Auth;
using MedicineMonitor.Application.Interfaces;
using MedicineMonitor.Domain.Entities;
using System.Text.RegularExpressions;

namespace MedicineMonitor.Application.Services;

public sealed class UserService(
    IUserRepository userRepository,
    IBoxRepository boxRepository,
    ICurrentUser currentUser)
{
    private static readonly Regex MacAddressRegex =
        new(
            @"^([0-9A-Fa-f]{2}:){5}[0-9A-Fa-f]{2}$",
            RegexOptions.Compiled);

    private static readonly HashSet<string> ValidBloodTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "A+",
            "A-",
            "B+",
            "B-",
            "AB+",
            "AB-",
            "O+",
            "O-"
        };

    /*
     * ==========================================
     * COMPLETE PROFILE
     * ==========================================
     */

    public async Task<UserProfileResponse> CompleteProfileAsync(
        CompleteProfileRequest request,
        CancellationToken cancellationToken)
    {
        EnsureAuthenticated();

        var firebaseUid =
            currentUser.UserId!;

        var email =
            currentUser.Email ?? string.Empty;

        var firstName =
            NormalizeRequiredText(
                request.FirstName,
                "First name");

        var lastName =
            NormalizeRequiredText(
                request.LastName,
                "Last name");

        var phoneNumber =
            NormalizeRequiredText(
                request.PhoneNumber,
                "Phone number");

        ValidateDateOfBirth(
            request.DateOfBirth);

        var bloodType =
            NormalizeBloodType(
                request.BloodType);

        var boxId =
            request.BoxId
                .Trim()
                .ToUpperInvariant();

        ValidateBoxId(boxId);

        var existing =
            await userRepository.GetByFirebaseUidAsync(
                firebaseUid,
                cancellationToken);

        User user;

        if (existing is null)
        {
            user = new User(
                firebaseUid,
                email,
                firstName,
                lastName,
                phoneNumber,
                request.DateOfBirth,
                bloodType);

            await userRepository.UpsertAsync(
                user,
                cancellationToken);
        }
        else
        {
            existing.UpdateProfile(
                firstName,
                lastName,
                phoneNumber,
                email,
                request.DateOfBirth,
                bloodType);

            user = existing;

            await userRepository.UpsertAsync(
                existing,
                cancellationToken);
        }

        /*
         * ==========================================
         * MEDICINE BOX
         * ==========================================
         */

        var existingBox =
            await boxRepository.GetByUserIdAsync(
                user.Id,
                cancellationToken);

        if (existingBox is null)
        {
            var boxAlreadyRegistered =
                await boxRepository.GetByBoxIdAsync(
                    boxId,
                    cancellationToken);

            if (boxAlreadyRegistered is not null)
            {
                throw new InvalidOperationException(
                    "This Medicine Monitor box is already registered.");
            }

            var box = new Box(
                user.Id,
                boxId,
                "My Medicine Box");

            await boxRepository.AddAsync(
                box,
                cancellationToken);
        }
        else
        {
            if (!string.Equals(
                    existingBox.BoxId,
                    boxId,
                    StringComparison.OrdinalIgnoreCase))
            {
                var boxAlreadyRegistered =
                    await boxRepository.GetByBoxIdAsync(
                        boxId,
                        cancellationToken);

                if (boxAlreadyRegistered is not null &&
                    boxAlreadyRegistered.Id != existingBox.Id)
                {
                    throw new InvalidOperationException(
                        "This Medicine Monitor box is already registered.");
                }
            }

            existingBox.Update(
                boxId,
                existingBox.Name);
        }

        await userRepository.SaveChangesAsync(
            cancellationToken);

        return ToResponse(user);
    }

    /*
     * ==========================================
     * GET CURRENT USER
     * ==========================================
     */

    public async Task<UserProfileResponse?> GetMeAsync(
        CancellationToken cancellationToken)
    {
        if (
            !currentUser.IsAuthenticated ||
            string.IsNullOrWhiteSpace(
                currentUser.UserId))
        {
            return null;
        }

        var user =
            await userRepository.GetByFirebaseUidAsync(
                currentUser.UserId,
                cancellationToken);

        return user is null
            ? null
            : ToResponse(user);
    }

    /*
     * ==========================================
     * UPDATE CURRENT USER PROFILE
     * ==========================================
     */

    public async Task<UserProfileResponse> UpdateProfileAsync(
        UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        EnsureAuthenticated();

        var firebaseUid =
            currentUser.UserId!;

        var user =
            await userRepository.GetByFirebaseUidAsync(
                firebaseUid,
                cancellationToken);

        if (user is null)
        {
            throw new InvalidOperationException(
                "Application user profile was not found.");
        }

        var firstName =
            NormalizeRequiredText(
                request.FirstName,
                "First name");

        var lastName =
            NormalizeRequiredText(
                request.LastName,
                "Last name");

        var phoneNumber =
            NormalizeRequiredText(
                request.PhoneNumber,
                "Phone number");

        ValidateDateOfBirth(
            request.DateOfBirth);

        var bloodType =
            NormalizeBloodType(
                request.BloodType);

        /*
         * Email comes from the authenticated
         * Firebase session.
         *
         * The client cannot change the
         * email through this endpoint.
         */

        var email =
            currentUser.Email
            ?? user.Email;

        user.UpdateProfile(
            firstName,
            lastName,
            phoneNumber,
            email,
            request.DateOfBirth,
            bloodType);

        await userRepository.UpsertAsync(
            user,
            cancellationToken);

        await userRepository.SaveChangesAsync(
            cancellationToken);

        return ToResponse(user);
    }

    /*
     * ==========================================
     * AUTHENTICATION
     * ==========================================
     */

    private void EnsureAuthenticated()
    {
        if (
            !currentUser.IsAuthenticated ||
            string.IsNullOrWhiteSpace(
                currentUser.UserId))
        {
            throw new UnauthorizedAccessException();
        }
    }

    /*
     * ==========================================
     * TEXT VALIDATION
     * ==========================================
     */

    private static string NormalizeRequiredText(
        string value,
        string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                $"{fieldName} is required.");
        }

        var normalized =
            value.Trim();

        if (normalized.Length > 100)
        {
            throw new ArgumentException(
                $"{fieldName} cannot exceed 100 characters.");
        }

        return normalized;
    }

    /*
     * ==========================================
     * DATE OF BIRTH VALIDATION
     * ==========================================
     */

    private static void ValidateDateOfBirth(
        DateOnly dateOfBirth)
    {
        var today =
            DateOnly.FromDateTime(
                DateTime.UtcNow);

        if (dateOfBirth > today)
        {
            throw new ArgumentException(
                "Date of birth cannot be in the future.");
        }
    }

    /*
     * ==========================================
     * BLOOD TYPE VALIDATION
     * ==========================================
     */

    private static string NormalizeBloodType(
        string bloodType)
    {
        if (string.IsNullOrWhiteSpace(bloodType))
        {
            throw new ArgumentException(
                "Blood type is required.");
        }

        var normalized =
            bloodType
                .Trim()
                .ToUpperInvariant();

        if (!ValidBloodTypes.Contains(
                normalized))
        {
            throw new ArgumentException(
                "Invalid blood type.");
        }

        return normalized;
    }

    /*
     * ==========================================
     * BOX ID VALIDATION
     * ==========================================
     */

    private static void ValidateBoxId(
        string boxId)
    {
        if (!MacAddressRegex.IsMatch(boxId))
        {
            throw new ArgumentException(
                "Invalid Medicine Box ID / MAC address. " +
                "Expected format: 68:09:47:28:0E:B0");
        }
    }

    /*
     * ==========================================
     * BUILD RESPONSE
     * ==========================================
     */

    private static UserProfileResponse ToResponse(
        User user) =>
        new(
            user.Id,
            user.FirebaseUid,
            user.Email,
            user.FirstName,
            user.LastName,
            user.PhoneNumber,
            user.DateOfBirth,
            user.BloodType,
            CalculateAge(user.DateOfBirth),
            user.CreatedAtUtc,
            user.UpdatedAtUtc);

    /*
     * ==========================================
     * CALCULATE AGE
     * ==========================================
     */

    private static int? CalculateAge(
        DateOnly? dateOfBirth)
    {
        if (!dateOfBirth.HasValue)
        {
            return null;
        }

        var today =
            DateOnly.FromDateTime(
                DateTime.UtcNow);

        var age =
            today.Year -
            dateOfBirth.Value.Year;

        if (
            today <
            dateOfBirth.Value.AddYears(age))
        {
            age--;
        }

        return age;
    }
}