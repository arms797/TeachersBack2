using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using TeachersBack2.Data;
using TeachersBack2.Models;

[Authorize(Roles = "admin")]
[ApiController]
[Route("api/weeklyschedule")]
public class WeeklyScheduleController : ControllerBase
{
    private readonly AppDbContext _context;

    public WeeklyScheduleController(AppDbContext context)
    {
        _context = context;
    }

    // 📥 بارگذاری دسته‌جمعی از طریق فایل اکسل
    [HttpPost("upload-excel")]
    public async Task<IActionResult> UploadExcel(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest("فایل معتبر نیست");

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets[0];
            int rowCount = worksheet.Dimension.Rows;

            for (int row = 2; row <= rowCount; row++)
            {
                var schedule = new WeeklySchedule
                {
                    TeacherCode = worksheet.Cells[row, 1].Text,
                    DayOfWeek = worksheet.Cells[row, 2].Text,
                    Center = worksheet.Cells[row, 3].Text,
                    A = worksheet.Cells[row, 4].Text,
                    B = worksheet.Cells[row, 5].Text,
                    C = worksheet.Cells[row, 6].Text,
                    D = worksheet.Cells[row, 7].Text,
                    E = worksheet.Cells[row, 8].Text,
                    Description = worksheet.Cells[row, 9].Text,
                    AlternativeHours = worksheet.Cells[row, 10].Text,
                    ForbiddenHours = worksheet.Cells[row, 11].Text,
                    Term = worksheet.Cells[row, 12].Text
                };

                _context.WeeklySchedules.Add(schedule);
            }

            await _context.SaveChangesAsync();
            return Ok("اطلاعات برنامه هفتگی با موفقیت ثبت شد");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"خطا در پردازش فایل: {ex.Message}");
        }
    }

    // 🔍 خواندن برنامه بر اساس کد استاد
    [HttpGet("by-code/{code}")]
    public async Task<IActionResult> GetByTeacherCode(string code)
    {
        try
        {
            var schedules = await _context.WeeklySchedules
                .Where(ws => ws.TeacherCode == code)
                .ToListAsync();

            return Ok(schedules);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"خطا در دریافت برنامه: {ex.Message}");
        }
    }

    // 📄 دریافت همه برنامه‌ها
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var schedules = await _context.WeeklySchedules.ToListAsync();
            return Ok(schedules);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"خطا در دریافت لیست: {ex.Message}");
        }
    }

    // 📄 دریافت برنامه با آیدی
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var schedule = await _context.WeeklySchedules.FindAsync(id);
            return schedule == null ? NotFound("برنامه یافت نشد") : Ok(schedule);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"خطا در دریافت اطلاعات: {ex.Message}");
        }
    }

    // ➕ افزودن برنامه جدید
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] WeeklySchedule model)
    {
        try
        {
            _context.WeeklySchedules.Add(model);
            await _context.SaveChangesAsync();
            return Ok(model);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"خطا در افزودن برنامه: {ex.Message}");
        }
    }

    // ✏️ ویرایش برنامه
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] WeeklySchedule model)
    {
        try
        {
            var schedule = await _context.WeeklySchedules.FindAsync(id);
            if (schedule == null) return NotFound("برنامه یافت نشد");

            schedule.TeacherCode = model.TeacherCode;
            schedule.DayOfWeek = model.DayOfWeek;
            schedule.Center = model.Center;
            schedule.A = model.A;
            schedule.B = model.B;
            schedule.C = model.C;
            schedule.D = model.D;
            schedule.E = model.E;
            schedule.Description = model.Description;
            schedule.AlternativeHours = model.AlternativeHours;
            schedule.ForbiddenHours = model.ForbiddenHours;
            schedule.Term = model.Term;

            await _context.SaveChangesAsync();
            return Ok(schedule);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"خطا در بروزرسانی برنامه: {ex.Message}");
        }
    }

    // ❌ حذف برنامه
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var schedule = await _context.WeeklySchedules.FindAsync(id);
            if (schedule == null) return NotFound("برنامه یافت نشد");

            _context.WeeklySchedules.Remove(schedule);
            await _context.SaveChangesAsync();
            return Ok("برنامه حذف شد");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"خطا در حذف برنامه: {ex.Message}");
        }
    }
}
