using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TeachersBack2.Data;
using TeachersBack2.DTOs;
using TeachersBack2.Helpers;
using TeachersBack2.Services;

namespace TeachersBack2.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly JwtService _jwt;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext db, JwtService jwt, IConfiguration config)
    {
        _db = db; _jwt = jwt; _config = config;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        try
        {
            var validTypes = new[] { "teacher", "user" };
            if (!validTypes.Contains(dto.UserType))
                return BadRequest(new { message = "نوع کاربر نامعتبر است." });

            if (dto.UserType == "teacher")
            {
                var teacher = await _db.Teachers.FirstOrDefaultAsync(t => t.Code == dto.Username);
                if (teacher is null)
                    return Unauthorized(new { message = "استاد یافت نشد." });

                if (!BCrypt.Net.BCrypt.Verify(dto.Password, teacher.PasswordHash))
                    return Unauthorized(new { message = "نام کاربری یا رمز عبور اشتباه است." });
                //var fullName = teacher.Fname + teacher.Lname;

                var token = _jwt.GenerateToken(teacher.Id, teacher.Code, new List<string> { "teacher" });

                // Set HttpOnly auth cookie
                var cookieName = _config["Jwt:CookieName"]!;
                CookieHelper.SetAuthCookie(Response, cookieName, token);

                // CSRF token
                var csrfToken = CookieHelper.GenerateCsrfToken();
                CookieHelper.SetCsrfCookie(Response, _config["Jwt:CsrfCookieName"]!, csrfToken);

                return Ok(new { message = "ورود استاد موفق بود.", roles = new[] { "teacher" } });
            }

            // ورود کاربران سیستم
            var user = await _db.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == dto.Username);

            if (user is null || !user.IsActive)
                return Unauthorized(new { message = "کاربر یافت نشد یا غیرفعال است." });

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized(new { message = "نام کاربری یا رمز عبور اشتباه است." });

            var roles = user.UserRoles.Select(ur => ur.Role.Title).ToList();
            var tokenUser = _jwt.GenerateToken(user.Id, user.Username, roles);

            var cookieNameUser = _config["Jwt:CookieName"]!;
            CookieHelper.SetAuthCookie(Response, cookieNameUser, tokenUser);

            var csrfTokenUser = CookieHelper.GenerateCsrfToken();
            CookieHelper.SetCsrfCookie(Response, _config["Jwt:CsrfCookieName"]!, csrfTokenUser);

            return Ok(new { message = "ورود موفق.", roles });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "خطا در ورود.", detail = ex.Message });
        }
    }

    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        try
        {
            CookieHelper.ClearCookie(Response, _config["Jwt:CookieName"]!);
            CookieHelper.ClearCookie(Response, _config["Jwt:CsrfCookieName"]!);
            return Ok(new { message = "خروج موفق." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "خطا در خروج.", detail = ex.Message });
        }
    }

    // Middleware ساده CSRF: بررسی هدر با کوکی
    private bool ValidateCsrf(HttpRequest request)
    {
        try
        {
            var headerName = _config["Jwt:CsrfHeaderName"]!;
            var cookieName = _config["Jwt:CsrfCookieName"]!;
            var headerToken = request.Headers[headerName].FirstOrDefault();
            var cookieToken = request.Cookies[cookieName];
            return !string.IsNullOrEmpty(headerToken) && !string.IsNullOrEmpty(cookieToken) && headerToken == cookieToken;
        }
        catch (Exception ex)
        {
            return false;
        }
        
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        try
        {
            if (!ValidateCsrf(Request))
                return Forbid("CSRF validation failed.");

            var username = User.Identity?.Name;
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user is null) return NotFound(new { message = "کاربر یافت نشد." });

            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
                return BadRequest(new { message = "رمز فعلی نادرست است." });

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _db.SaveChangesAsync();

            return Ok(new { message = "رمز عبور با موفقیت تغییر کرد." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "خطا در تغییر رمز.", detail = ex.Message });
        }
    }

    [Authorize]
    [HttpPost("update-contact")]
    public async Task<IActionResult> UpdateContact([FromBody] UpdateContactDto dto)
    {
        try
        {
            if (!ValidateCsrf(Request))
                return Forbid("CSRF validation failed.");

            var username = User.Identity?.Name;
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user is null) return NotFound(new { message = "کاربر یافت نشد." });

            if (!string.IsNullOrWhiteSpace(dto.Mobile))
                user.Mobile = dto.Mobile!;
            if (!string.IsNullOrWhiteSpace(dto.Email))
                user.Email = dto.Email!;

            await _db.SaveChangesAsync();
            return Ok(new { message = "اطلاعات تماس بروزرسانی شد." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "خطا در بروزرسانی تماس.", detail = ex.Message });
        }
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = User.Identity.Name;
            var roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            return Ok(new
            {
                id = userId,
                username = username,
                roles = roles
            });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
        
    }

}
