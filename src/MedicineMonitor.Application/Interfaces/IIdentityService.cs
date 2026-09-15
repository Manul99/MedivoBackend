namespace MedicineMonitor.Application.Interfaces;

public interface IIdentityService
{
    Task<string> CreateSessionCookieAsync(string idToken, TimeSpan expiresIn, CancellationToken cancellationToken);
    Task<AuthenticatedIdentity?> VerifyIdTokenAsync(string idToken, CancellationToken cancellationToken);
    Task<AuthenticatedIdentity?> VerifySessionCookieAsync(string sessionCookie, bool checkRevoked, CancellationToken cancellationToken);
    Task RevokeRefreshTokensAsync(string userId, CancellationToken cancellationToken);
}

public sealed record AuthenticatedIdentity(string UserId, string? Email, IReadOnlyDictionary<string, object> Claims);
