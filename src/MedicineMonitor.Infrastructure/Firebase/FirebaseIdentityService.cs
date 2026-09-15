using MedicineMonitor.Application.Interfaces;
using FirebaseAdmin.Auth;

namespace MedicineMonitor.Infrastructure.Firebase;

public sealed class FirebaseIdentityService(FirebaseClient client) : IIdentityService
{
    public async Task<string> CreateSessionCookieAsync(
        string idToken,
        TimeSpan expiresIn,
        CancellationToken cancellationToken)
    {
        var options = new SessionCookieOptions { ExpiresIn = expiresIn };
        return await client.Auth.CreateSessionCookieAsync(idToken, options, cancellationToken);
    }

    public async Task<AuthenticatedIdentity?> VerifyIdTokenAsync(
        string idToken,
        CancellationToken cancellationToken)
    {
        try
        {
            var token = await client.Auth.VerifyIdTokenAsync(idToken, cancellationToken);
            var email = token.Claims.TryGetValue("email", out var emailValue)
                ? emailValue?.ToString()
                : null;

            return new AuthenticatedIdentity(
                token.Uid,
                email,
                token.Claims.ToDictionary(x => x.Key, x => x.Value));
        }
        catch (FirebaseAuthException)
        {
            return null;
        }
    }

    public async Task<AuthenticatedIdentity?> VerifySessionCookieAsync(
        string sessionCookie,
        bool checkRevoked,
        CancellationToken cancellationToken)
    {
        try
        {
            var token = await client.Auth.VerifySessionCookieAsync(sessionCookie, checkRevoked, cancellationToken);
            var email = token.Claims.TryGetValue("email", out var emailValue)
                ? emailValue?.ToString()
                : null;

            return new AuthenticatedIdentity(
                token.Uid,
                email,
                token.Claims.ToDictionary(x => x.Key, x => x.Value));
        }
        catch (FirebaseAuthException)
        {
            return null;
        }
    }

    public async Task RevokeRefreshTokensAsync(string userId, CancellationToken cancellationToken)
    {
        await client.Auth.RevokeRefreshTokensAsync(userId, cancellationToken);
    }
}
