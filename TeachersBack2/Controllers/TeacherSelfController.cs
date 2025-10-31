using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeachersBack2.Data;
using TeachersBack2.DTOs;
using TeachersBack2.Models;

namespace TeachersBack2.Controllers;

[ApiController]
[Route("api/teacher-self")]
[Authorize(Roles = "teacher")]
public class TeacherSelfController : ControllerBase
{
    private readonly AppDbContext _db;

    public TeacherSelfController(AppDbContext db)
    {
        _db = db;
    }

    // 📌 دریافت اطلاعات استاد لاگین‌شده
    [HttpGet("profile")]
    public async Task<IActionResult> GetOwnProfile()
    {
        try
        {
            var code = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(code))
                return Unauthorized(new { message = "کد استاد معتبر نیست." });

            var teacher = await _db.Teachers.FirstOrDefaultAsync(t => t.Code == code);
            if (teacher == null)
                return NotFound(new { message = "اطلاعات استاد یافت نشد." });

            return Ok(teacher);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "خطا در دریافت اطلاعات استاد.", detail = ex.Message });
        }
    }

    // 📌 ویرایش اطلاعات استاد
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] Teacher updated)
    {
        try
        {
            var code = User.Identity?.Name;
            var teacher = await _db.Teachers.FirstOrDefaultAsync(t => t.Code == code);
            if (teacher == null)
                return NotFound(new { message = "استاد یافت نشد." });

            teacher.Fname = updated.Fname;
            teacher.Lname = updated.Lname;
            teacher.Email = updated.Email;
            teacher.Mobile = updated.Mobile;
            teacher.FieldOfStudy = updated.FieldOfStudy;
            teacher.Center = updated.Center;
            teacher.CooperationType = updated.CooperationType;
            teacher.AcademicRank = updated.AcademicRank;
            teacher.ExecutivePosition = updated.ExecutivePosition;
            teacher.NationalCode = updated.NationalCode;

            await _db.SaveChangesAsync();
            return Ok(new { message = "اطلاعات با موفقیت ویرایش شد." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "خطا در ویرایش اطلاعات استاد.", detail = ex.Message });
        }
    }

    /*
    [HttpGet("teacher-terms/{term}")]
    public async Task<IActionResult> GetTeacherTermsByTerm(string term)
    {
        try
        {
            var code = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(code))
                return Unauthorized(new { message = "کد استاد معتبر نیست." });

            if (string.IsNullOrWhiteSpace(term))
                return BadRequest(new { message = "ترم معتبر ارسال نشده است." });

            var terms = await _db.TeacherTerms
                .Where(tt => tt.Code == code && tt.Term == term)
                .ToListAsync();

            return Ok(terms);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "خطا در دریافت اطلاعات ترمی استاد.", detail = ex.Message });
        }
    }

    [HttpPut("teacher-terms/{id}")]
    public async Task<IActionResult> UpdateTeacherTerm(int id, [FromBody] TeacherTerm updated)
    {
        try
        {
            var code = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(code))
                return Unauthorized(new { message = "کد استاد معتبر نیست." });

            var term = await _db.TeacherTerms.Where(t => t.Id == id && t.Code == code).FirstOrDefaultAsync();

            if (term == null)
                return NotFound(new { message = "رکورد ترمی مورد نظر یافت نشد یا متعلق به شما نیست." });

            // فقط فیلدهای قابل ویرایش
            term.IsNeighborTeaching = updated.IsNeighborTeaching;
            term.NeighborTeaching = updated.NeighborTeaching;
            term.NeighborCenters = updated.NeighborCenters;
            term.Suggestion = updated.Suggestion;
            term.Projector = updated.Projector;
            term.Whiteboard2 = updated.Whiteboard2;

            await _db.SaveChangesAsync();
            return Ok(new { message = "اطلاعات ترمی با موفقیت ویرایش شد." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "خطا در ویرایش اطلاعات ترمی.", detail = ex.Message });
        }
    }
    */

    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        try
        {
            var code = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(code))
                return Unauthorized(new { message = "کد استاد معتبر نیست." });

            var teacher = await _db.Teachers.FirstOrDefaultAsync(t => t.Code == code);
            if (teacher == null)
                return NotFound(new { message = "استاد یافت نشد." });

            // بررسی رمز فعلی
            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, teacher.PasswordHash))
                return Unauthorized(new { message = "رمز فعلی اشتباه است." });

            // هش رمز جدید
            teacher.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

            await _db.SaveChangesAsync();
            return Ok(new { message = "رمز عبور با موفقیت تغییر یافت." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "خطا در تغییر رمز عبور.", detail = ex.Message });
        }
    }

}
