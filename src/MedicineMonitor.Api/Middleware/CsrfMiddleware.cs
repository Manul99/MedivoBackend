using System.Security.Cryptography;
using System.Text;

namespace MedicineMonitor.Api.Middleware;

public sealed class CsrfMiddleware(RequestDelegate next, IConfiguration configuration)
{
    private const string CookieName = "XSRF-TOKEN";
    private const string HeaderName = "X-XSRF-TOKEN";

    public async Task InvokeAsync(HttpContext context)
    {
        if (HttpMethods.IsOptions(context.Request.Method) || IsSafeMethod(context.Request.Method))
        {
            await next(context);
            return;
        }

        if (context.Request.Path.StartsWithSegments("/api/auth/session"))
        {
            if (!IsValidToken(context))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new { error = "CSRF validation failed." });
                return;
            }
        }
        else if (context.Request.Path.StartsWithSegments("/api"))
        {
            if (!IsValidToken(context))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new { error = "CSRF validation failed." });
                return;
            }
        }

        await next(context);
    }

    private static bool IsSafeMethod(string method) =>
        HttpMethods.IsGet(method) || HttpMethods.IsHead(method) || HttpMethods.IsTrace(method);

    private static bool IsValidToken(HttpContext context)
    {
        var cookieToken = context.Request.Cookies[CookieName];
        var headerToken = context.Request.Headers[HeaderName].ToString();

        if (string.IsNullOrWhiteSpace(cookieToken) || string.IsNullOrWhiteSpace(headerToken))
            return false;

        var a = Encoding.UTF8.GetBytes(cookieToken);
        var b = Encoding.UTF8.GetBytes(headerToken);
        return a.Length == b.Length && CryptographicOperations.FixedTimeEquals(a, b);
    }

    public static string CreateToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

    public static void AppendTokenCookie(HttpResponse response, string token, bool secure)
    {
        response.Cookies.Append(CookieName, token, new CookieOptions
        {
            HttpOnly = false,
            Secure = secure,
            SameSite = secure ? SameSiteMode.None : SameSiteMode.Lax,
            IsEssential = true,
            Path = "/"
        });
    }
}
