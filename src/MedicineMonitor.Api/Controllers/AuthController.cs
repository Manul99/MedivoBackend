using MedicineMonitor.Api.Middleware;
using MedicineMonitor.Application.DTOs.Auth;
using MedicineMonitor.Application.Interfaces;
using MedicineMonitor.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MedicineMonitor.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    IIdentityService identityService,
    UserService userService,
    IConfiguration configuration) : ControllerBase
{
    [HttpGet("csrf")]
    [AllowAnonymous]
    public IActionResult GetCsrfToken()
    {
        var token = CsrfMiddleware.CreateToken();

        CsrfMiddleware.AppendTokenCookie(
            Response,
            token,
            IsSecureCookies());

        return Ok(new
        {
            token
        });
    }

    [HttpPost("session")]
    [AllowAnonymous]
    public async Task<IActionResult> CreateSession(
        [FromBody] CreateSessionRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.IdToken))
            return BadRequest(new { error = "Firebase ID token is required." });

        var identity = await identityService.VerifyIdTokenAsync(request.IdToken, cancellationToken);
        if (identity is null)
            return Unauthorized(new { error = "Invalid Firebase ID token." });

        if (!IsRecentlyAuthenticated(identity))
            return Unauthorized(new { error = "Recent sign-in is required." });

        var days = configuration.GetValue("Authentication:SessionDays", 5);
        days = Math.Clamp(days, 1, 14);
        var expiresIn = TimeSpan.FromDays(days);

        var sessionCookie = await identityService.CreateSessionCookieAsync(
            request.IdToken,
            expiresIn,
            cancellationToken);

        Response.Cookies.Append("mm_session", sessionCookie, new CookieOptions
        {
            HttpOnly = true,
            Secure = IsSecureCookies(),
            SameSite = IsSecureCookies() ? SameSiteMode.None : SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.Add(expiresIn),
            IsEssential = true,
            Path = "/"
        });

        return Ok(new { authenticated = true });
    }

    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("mm_session", new CookieOptions
        {
            Secure = IsSecureCookies(),
            SameSite = IsSecureCookies() ? SameSiteMode.None : SameSiteMode.Lax,
            Path = "/"
        });

        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var profile = await userService.GetMeAsync(cancellationToken);
        return profile is null ? NotFound() : Ok(profile);
    }

    [HttpPost("profile")]
    [Authorize]
    public async Task<IActionResult> CompleteProfile(
        [FromBody] CompleteProfileRequest request,
        CancellationToken cancellationToken)
    {
        var profile = await userService.CompleteProfileAsync(request, cancellationToken);
        return Ok(profile);
    }

    private bool IsSecureCookies() => configuration.GetValue("Authentication:SecureCookies", false);

    private static bool IsRecentlyAuthenticated(AuthenticatedIdentity identity)
    {
        if (!identity.Claims.TryGetValue("auth_time", out var raw)) return false;
        if (!long.TryParse(raw?.ToString(), out var unixSeconds)) return false;

        var authTime = DateTimeOffset.FromUnixTimeSeconds(unixSeconds);
        return DateTimeOffset.UtcNow - authTime <= TimeSpan.FromMinutes(5);
    }
}

public sealed class CreateSessionRequest
{
    public string IdToken { get; init; } = string.Empty;
}
