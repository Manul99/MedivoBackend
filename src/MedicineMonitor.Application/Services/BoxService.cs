using MedicineMonitor.Application.Abstractions;
using MedicineMonitor.Application.Interfaces;

namespace MedicineMonitor.Application.Services;

public sealed class BoxService(
    IUserRepository userRepository,
    IBoxRepository boxRepository,
    ICurrentUser currentUser,
    IFirebaseRealtimeDatabaseService firebaseDatabaseService)
{
    public async Task SetPowerAsync(
        bool isOn,
        CancellationToken cancellationToken)
    {
        var firebaseUserId =
            GetRequiredUserId();

        var user =
            await userRepository.GetByFirebaseUidAsync(
                firebaseUserId,
                cancellationToken);

        if (user is null)
        {
            throw new InvalidOperationException(
                "Application user profile was not found.");
        }

        var box =
            await boxRepository.GetByUserIdAsync(
                user.Id,
                cancellationToken);

        if (box is null)
        {
            throw new InvalidOperationException(
                "No Medicine Monitor box is associated with this user.");
        }

        var powerValue =
            isOn ? 1 : 0;

        await firebaseDatabaseService.SetBoxPowerAsync(
            box.BoxId,
            new FirebaseBoxPowerConfiguration(
                user.Id.ToString(),
                powerValue),
            cancellationToken);
    }

    private string GetRequiredUserId()
    {
        if (
            !currentUser.IsAuthenticated ||
            string.IsNullOrWhiteSpace(
                currentUser.UserId))
        {
            throw new UnauthorizedAccessException();
        }

        return currentUser.UserId;
    }
}