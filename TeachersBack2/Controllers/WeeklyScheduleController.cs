using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeachersBack2.Data;
using TeachersBack2.Models;

namespace TeachersBack2.Controllers;


[ApiController]
[Route("api/weekly-schedule")]
public class WeeklyScheduleController : ControllerBase
{
    private readonly AppDbContext _db;

    public WeeklyScheduleController(AppDbContext db)
    {
        _db = db;
    }

    // 📌 دریافت برنامه هفتگی استاد بر اساس کد و ترم
    [Authorize]
    [HttpGet("{teacherCode}/{term}")]    
    public async Task<IActionResult> GetWeeklySchedule(string teacherCode, string term)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(teacherCode) || string.IsNullOrWhiteSpace(term))
                return BadRequest(new { message = "کد استاد یا ترم معتبر نیست." });

            var schedule = await _db.WeeklySchedules
                .Where(ws => ws.TeacherCode == teacherCode && ws.Term == term)
                .ToListAsync();

            return Ok(schedule);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "خطا در دریافت برنامه هفتگی.", detail = ex.Message });
        }
    }

    [Authorize(Roles = "admin,teacher")]
    [HttpPut("weekly-schedule/{id}")]
    public async Task<IActionResult> UpdateWeeklySchedule(int id, [FromBody] WeeklySchedule updated)
    {
        try
        {
            var code = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(code))
                return Unauthorized(new { message = "کد استاد معتبر نیست." });

            var schedule = await _db.WeeklySchedules
                .FirstOrDefaultAsync(ws => ws.Id == id && ws.TeacherCode == code);

            if (schedule == null)
                return NotFound(new { message = "رکورد برنامه هفتگی یافت نشد یا متعلق به شما نیست." });

            // فقط فیلدهای قابل ویرایش
            schedule.Center = updated.Center;
            schedule.A = updated.A;
            schedule.B = updated.B;
            schedule.C = updated.C;
            schedule.D = updated.D;
            schedule.E = updated.E;
            schedule.Description = updated.Description;
            schedule.AlternativeHours = updated.AlternativeHours;
            schedule.ForbiddenHours = updated.ForbiddenHours;
            schedule.Term = updated.Term;

            await _db.SaveChangesAsync();
            return Ok(new { message = "برنامه هفتگی با موفقیت ویرایش شد." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "خطا در ویرایش برنامه هفتگی.", detail = ex.Message });
        }
    }
    
   
    [HttpPost("weekly-schedule/generate-for-all/{term}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GenerateWeeklyScheduleForAll(string term)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(term))
                return BadRequest(new { message = "ترم معتبر ارسال نشده است." });

            var teachers = await _db.Teachers.ToListAsync();
            var daysOfWeek = new[] { "شنبه", "یکشنبه", "دوشنبه", "سه‌شنبه", "چهارشنبه", "پنجشنبه", "جمعه" };

            int successCount = 0;
            int errorCount = 0;

            foreach (var teacher in teachers)
            {
                try
                {
                    foreach (var day in daysOfWeek)
                    {
                        var schedule = new WeeklySchedule
                        {
                            TeacherCode = teacher.Code,
                            DayOfWeek = day,
                            Center = "عدم حضور در مرکز",
                            A = "عدم حضور در مرکز",
                            B = "عدم حضور در مرکز",
                            C = "عدم حضور در مرکز",
                            D = "عدم حضور در مرکز",
                            E = "عدم حضور در مرکز",
                            Description = "",
                            AlternativeHours = "",
                            ForbiddenHours = "",
                            Term = term
                        };

                        _db.WeeklySchedules.Add(schedule);
                    }

                    await _db.SaveChangesAsync();
                    successCount++;
                }
                catch
                {
                    errorCount++;
                }
            }

            return Ok(new
            {
                message = "ایجاد برنامه هفتگی اولیه برای همه اساتید انجام شد.",
                successCount,
                errorCount
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "خطای کلی در عملیات ایجاد برنامه هفتگی.", detail = ex.Message });
        }
    }


}
