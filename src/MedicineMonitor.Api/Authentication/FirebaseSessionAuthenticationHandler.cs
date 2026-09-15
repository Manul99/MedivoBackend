using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using MedicineMonitor.Application.Interfaces;

namespace MedicineMonitor.Api.Authentication;

public sealed class FirebaseSessionAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IIdentityService identityService)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var cookie = Request.Cookies["mm_session"];
        if (string.IsNullOrWhiteSpace(cookie))
            return AuthenticateResult.NoResult();

        var identity = await identityService.VerifySessionCookieAsync(cookie, true, Context.RequestAborted);
        if (identity is null)
            return AuthenticateResult.Fail("Invalid or expired session.");

        var claims = new List<Claim>
        {
            new("firebase_uid", identity.UserId),
            new(ClaimTypes.NameIdentifier, identity.UserId)
        };

        if (!string.IsNullOrWhiteSpace(identity.Email))
            claims.Add(new Claim(ClaimTypes.Email, identity.Email));

        if (identity.Claims.TryGetValue("email_verified", out var verified) && Convert.ToBoolean(verified))
            claims.Add(new Claim("email_verified", "true"));

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme.Name));
        return AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name));
    }
}
