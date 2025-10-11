using Microsoft.AspNetCore.Http;

namespace TeachersBack2.Helpers;

public static class CookieHelper
{
    public static void SetAuthCookie(HttpResponse response, string cookieName, string jwtToken)
    {
        response.Cookies.Append(cookieName, jwtToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.AddDays(1) // قابل تنظیم
        });
    }

    public static void ClearCookie(HttpResponse response, string cookieName)
    {
        response.Cookies.Delete(cookieName, new CookieOptions
        {
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/"
        });
    }

    public static string GenerateCsrfToken()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    }

    public static void SetCsrfCookie(HttpResponse response, string csrfCookieName, string csrfToken)
    {
        response.Cookies.Append(csrfCookieName, csrfToken, new CookieOptions
        {
            HttpOnly = false, // قابل خواندن توسط JS برای ارسال در Header
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/"
        });
    }
}
