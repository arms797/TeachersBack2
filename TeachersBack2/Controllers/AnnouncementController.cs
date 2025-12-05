using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.RegularExpressions;
using TeachersBack2.Data;
using TeachersBack2.Models;

namespace TeachersBack2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnnouncementController : ControllerBase
    {
        private readonly AppDbContext _db;
        public AnnouncementController(AppDbContext db)
        {
            _db = db;
        }

        // متد کمکی برای تولید تاریخ امروز شمسی به صورت رشته
        private string GetTodayPersian()
        {
            var pc = new PersianCalendar();
            var now = DateTime.Now;
            return $"{pc.GetYear(now):0000}/{pc.GetMonth(now):00}/{pc.GetDayOfMonth(now):00}";
        }

        // متد کمکی برای اعتبارسنجی فرمت تاریخ رشته‌ای
        private bool IsValidPersianDate(string? date)
        {
            if (string.IsNullOrWhiteSpace(date)) return false;
            return Regex.IsMatch(date, @"^\d{4}/\d{2}/\d{2}$");
        }

        // خواندن همه اطلاعیه‌ها
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var list = await _db.Announcements.OrderByDescending(x=>x.CreateDate).ToListAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"خطا در دریافت اطلاعیه‌ها: {ex.Message}");
            }
        }

        // خواندن اطلاعیه با آیدی
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var announcement = await _db.Announcements.FindAsync(id);
                if (announcement == null) return NotFound("اطلاعیه یافت نشد");
                return Ok(announcement);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"خطا در دریافت اطلاعیه: {ex.Message}");
            }
        }

        // ایجاد اطلاعیه جدید
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Announcement announcement)
        {
            try
            {
                if (!string.IsNullOrEmpty(announcement.StartDate) && !IsValidPersianDate(announcement.StartDate))
                    return BadRequest("فرمت تاریخ شروع معتبر نیست (yyyy/MM/dd)");

                if (!string.IsNullOrEmpty(announcement.EndDate) && !IsValidPersianDate(announcement.EndDate))
                    return BadRequest("فرمت تاریخ پایان معتبر نیست (yyyy/MM/dd)");
                announcement.CreateDate=GetTodayPersian();
                _db.Announcements.Add(announcement);
                await _db.SaveChangesAsync();
                return CreatedAtAction(nameof(GetById), new { id = announcement.Id }, announcement);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"خطا در ایجاد اطلاعیه: {ex.Message}");
            }
        }

        // ویرایش اطلاعیه
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Announcement updated)
        {
            try
            {
                var announcement = await _db.Announcements.FindAsync(id);
                if (announcement == null) return NotFound("اطلاعیه یافت نشد");

                if (!string.IsNullOrEmpty(updated.StartDate) && !IsValidPersianDate(updated.StartDate))
                    return BadRequest("فرمت تاریخ شروع معتبر نیست (yyyy/MM/dd)");

                if (!string.IsNullOrEmpty(updated.EndDate) && !IsValidPersianDate(updated.EndDate))
                    return BadRequest("فرمت تاریخ پایان معتبر نیست (yyyy/MM/dd)");

                announcement.Title = updated.Title;
                announcement.Body = updated.Body;
                announcement.StartDate = updated.StartDate;
                announcement.EndDate = updated.EndDate;
                announcement.IsActive = updated.IsActive;
                announcement.CreatedBy = updated.CreatedBy;                

                await _db.SaveChangesAsync();
                return Ok(announcement);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"خطا در ویرایش اطلاعیه: {ex.Message}");
            }
        }

        // حذف اطلاعیه
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var announcement = await _db.Announcements.FindAsync(id);
                if (announcement == null) return NotFound("اطلاعیه یافت نشد");

                _db.Announcements.Remove(announcement);
                await _db.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"خطا در حذف اطلاعیه: {ex.Message}");
            }
        }

        // فعال/غیرفعال کردن اطلاعیه
        [HttpPut("{id:int}/toggle")]
        public async Task<IActionResult> Toggle(int id, [FromBody] bool isActive)
        {
            try
            {
                var announcement = await _db.Announcements.FindAsync(id);
                if (announcement == null) return NotFound("اطلاعیه یافت نشد");

                announcement.IsActive = isActive;
                await _db.SaveChangesAsync();
                return Ok(announcement);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"خطا در تغییر وضعیت اطلاعیه: {ex.Message}");
            }
        }

        // اطلاعیه‌های فعال و معتبر (برای صفحه Home)
        [HttpGet("active")]
        public async Task<IActionResult> GetActive()
        {
            try
            {
                var today = GetTodayPersian();

                var list = await _db.Announcements
                    .Where(a => a.IsActive
                                && (string.IsNullOrEmpty(a.StartDate) || string.Compare(a.StartDate, today) <= 0)
                                && (string.IsNullOrEmpty(a.EndDate) || string.Compare(a.EndDate, today) >= 0))
                    .OrderByDescending(a => a.StartDate ?? a.Id.ToString())
                    .ToListAsync();

                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"خطا در دریافت اطلاعیه‌های فعال: {ex.Message}");
            }
        }
    }
}
