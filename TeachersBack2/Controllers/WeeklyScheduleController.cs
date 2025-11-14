using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Transactions;
using TeachersBack2.Data;
using TeachersBack2.Models;

namespace TeachersBack2.Controllers;


[ApiController]
[Route("api/[controller]")]
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
    [HttpPut("updateSchedule/{id}")]
    public async Task<IActionResult> UpdateWeeklySchedule(int id, [FromBody] WeeklySchedule updated)
    {
        try
        {
            var code = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(code))
                return Unauthorized(new { message = "کد استاد معتبر نیست." });

            var schedule = await _db.WeeklySchedules
                .FirstOrDefaultAsync(ws => ws.Id == id);

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

    // 📥 بارگذاری دسته‌جمعی از طریق فایل اکسل
    [HttpPost("upload-excel")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> UploadExcel(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest("فایل معتبر نیست");

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);

            int addedCount = 0;
            int duplicateCount = 0;
            int errorCount = 0;
            int skippedTermCount = 0;

            foreach (var row in worksheet.RowsUsed().Skip(1))
            {
                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                try
                {
                    var code = row.Cell(1).GetString().Trim();
                    var term = row.Cell(12).GetString().Trim();
                    var center = row.Cell(3).GetString().Trim();



                    bool isEmpty = string.IsNullOrWhiteSpace(code)
                        && string.IsNullOrWhiteSpace(term)
                        && string.IsNullOrWhiteSpace(center);
                    if (isEmpty)
                    {
                        errorCount++;
                        continue;
                    }

                    var existingTeacher = await _db.Teachers.FirstOrDefaultAsync(t => t.Code == code);
                    var isterm = await _db.TermCalenders.FirstOrDefaultAsync(x => x.Term == term);
                    if (center != "0")
                    {
                        var iscenter = await _db.Centers.FirstOrDefaultAsync(x => x.CenterCode == center);
                        if (iscenter == null)
                        {
                            center = existingTeacher.Center;
                            //errorCount++;
                            //continue;
                        }
                    }

                    if (existingTeacher == null || isterm == null)
                    {
                        errorCount++;
                        continue;
                    }
                    else
                    {
                        var dow = row.Cell(2).GetString().Trim();
                        var isws = await _db.WeeklySchedules.FirstOrDefaultAsync(
                            x => x.TeacherCode == code && x.Term == term && x.DayOfWeek == dow);
                        if (isws == null)
                        {
                            var ws = new WeeklySchedule
                            {
                                TeacherCode = code,
                                DayOfWeek = row.Cell(2).GetString().Trim(),
                                Center = center,
                                A = row.Cell(4).GetString().Trim(),
                                B = row.Cell(5).GetString().Trim(),
                                C = row.Cell(6).GetString().Trim(),
                                D = row.Cell(7).GetString().Trim(),
                                E = row.Cell(8).GetString().Trim(),
                                Description = row.Cell(9).GetString().Trim(),
                                AlternativeHours = row.Cell(10).GetString().Trim(),
                                ForbiddenHours = row.Cell(11).GetString().Trim(),
                                Term = term
                            };
                            _db.WeeklySchedules.Add(ws);
                        }
                        else
                        {
                            isws.Center = center;
                            isws.A = row.Cell(4).GetString().Trim() != "" ? row.Cell(4).GetString().Trim() : isws.A;
                            isws.B = row.Cell(5).GetString().Trim() != "" ? row.Cell(5).GetString().Trim() : isws.B;
                            isws.C = row.Cell(6).GetString().Trim() != "" ? row.Cell(6).GetString().Trim() : isws.C;
                            isws.D = row.Cell(7).GetString().Trim() != "" ? row.Cell(7).GetString().Trim() : isws.D;
                            isws.E = row.Cell(8).GetString().Trim() != "" ? row.Cell(8).GetString().Trim() : isws.E;
                            isws.Description = row.Cell(9).GetString().Trim();
                            isws.AlternativeHours = row.Cell(10).GetString().Trim();
                            isws.ForbiddenHours = row.Cell(11).GetString().Trim();
                        }
                        await _db.SaveChangesAsync();
                        addedCount++;
                    }
                    scope.Complete();
                }
                catch
                {
                    errorCount++;
                    // تراکنش لغو می‌شود
                }
            }

            return Ok(new
            {
                addedCount,
                duplicateCount,
                skippedTermCount,
                errorCount
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"خطا در پردازش فایل: {ex.Message}");
        }
    }


}
