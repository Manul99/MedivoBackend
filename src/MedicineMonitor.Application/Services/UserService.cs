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

    public async Task<UserProfileResponse> CompleteProfileAsync(
        CompleteProfileRequest request,
        CancellationToken cancellationToken)
    {
        if (
            !currentUser.IsAuthenticated ||
            string.IsNullOrWhiteSpace(
                currentUser.UserId))
        {
            throw new UnauthorizedAccessException();
        }

        var firebaseUid =
            currentUser.UserId;

        var email =
            currentUser.Email ?? string.Empty;

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
                request.FirstName,
                request.LastName,
                request.PhoneNumber);

            await userRepository.UpsertAsync(
                user,
                cancellationToken);
        }
        else
        {
            existing.UpdateProfile(
                request.FirstName,
                request.LastName,
                request.PhoneNumber,
                email);

            user = existing;

            await userRepository.UpsertAsync(
                existing,
                cancellationToken);
        }

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

    private static UserProfileResponse ToResponse(
        User user) =>
        new(
            user.Id,
            user.FirebaseUid,
            user.Email,
            user.FirstName,
            user.LastName,
            user.PhoneNumber,
            user.CreatedAtUtc,
            user.UpdatedAtUtc);
}