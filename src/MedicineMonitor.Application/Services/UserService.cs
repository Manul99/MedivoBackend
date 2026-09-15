using MedicineMonitor.Application.Abstractions;
using MedicineMonitor.Application.DTOs.Auth;
using MedicineMonitor.Application.Interfaces;
using MedicineMonitor.Domain.Entities;

namespace MedicineMonitor.Application.Services;

public sealed class UserService(IUserRepository userRepository, ICurrentUser currentUser)
{
    public async Task<UserProfileResponse> CompleteProfileAsync(
        CompleteProfileRequest request,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || string.IsNullOrWhiteSpace(currentUser.UserId))
            throw new UnauthorizedAccessException();

        var firebaseUid = currentUser.UserId;
        var email = currentUser.Email ?? string.Empty;
        var existing = await userRepository.GetByFirebaseUidAsync(firebaseUid, cancellationToken);

        User user;
        if (existing is null)
        {
            user = new User(firebaseUid, email, request.FirstName, request.LastName, request.PhoneNumber);
            await userRepository.UpsertAsync(user, cancellationToken);
        }
        else
        {
            existing.UpdateProfile(request.FirstName, request.LastName, request.PhoneNumber, email);
            user = existing;
            await userRepository.UpsertAsync(existing, cancellationToken);
        }

        await userRepository.SaveChangesAsync(cancellationToken);
        return ToResponse(user);
    }

    public async Task<UserProfileResponse?> GetMeAsync(CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || string.IsNullOrWhiteSpace(currentUser.UserId))
            return null;

        var user = await userRepository.GetByFirebaseUidAsync(currentUser.UserId, cancellationToken);
        return user is null ? null : ToResponse(user);
    }

    private static UserProfileResponse ToResponse(User user) => new(
        user.Id,
        user.FirebaseUid,
        user.Email,
        user.FirstName,
        user.LastName,
        user.PhoneNumber,
        user.CreatedAtUtc,
        user.UpdatedAtUtc);
}
