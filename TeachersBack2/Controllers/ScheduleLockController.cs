using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeachersBack2.Data;
using TeachersBack2.Models;

namespace TeachersBack2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScheduleLockController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ScheduleLockController(AppDbContext context)
        {
            _context = context;
        }

        // ------------------ خواندن ------------------
        [HttpGet("{term}/{teacherCode?}")]
        public async Task<IActionResult> GetLocks(string term, string? teacherCode)
        {
            try
            {
                IQueryable<ScheduleLock> query = _context.ScheduleLocks.Where(l => l.Term == term);

                if (!string.IsNullOrEmpty(teacherCode))
                    query = query.Where(l => l.TeacherCode == teacherCode);

                var result = await query.ToListAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "خطا در دریافت رکوردها", error = ex.Message });
            }
        }

        // ------------------ ایجاد ------------------
        [HttpPost]
        public async Task<IActionResult> CreateLock([FromBody] ScheduleLock lockRecord)
        {
            try
            {
                lockRecord.LockedAt = DateTime.UtcNow;
                _context.ScheduleLocks.Add(lockRecord);
                await _context.SaveChangesAsync();
                return Ok(lockRecord);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "خطا در ایجاد رکورد", error = ex.Message });
            }
        }

        // ------------------ حذف ------------------
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLock(int id)
        {
            try
            {
                var lockRecord = await _context.ScheduleLocks.FindAsync(id);
                if (lockRecord == null)
                    return NotFound(new { message = "رکورد یافت نشد" });

                _context.ScheduleLocks.Remove(lockRecord);
                await _context.SaveChangesAsync();
                return Ok(new { message = "رکورد با موفقیت حذف شد" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "خطا در حذف رکورد", error = ex.Message });
            }
        }

        // ------------------ قفل‌گذاری گروهی بر اساس نوع همکاری ------------------
        [HttpPost("lock-by-cooperation")]
        public async Task<IActionResult> LockByCooperation([FromBody] LockByCooperationRequest req)
        {
            try
            {
                if (string.IsNullOrEmpty(req.Term))
                    return BadRequest(new { message = "ترم الزامی است" });

                // اگر نوع همکاری = همه اساتید بود → بدون فیلتر
                IQueryable<Teacher> query = _context.Teachers;

                if (!string.IsNullOrEmpty(req.CooperationType) && req.CooperationType != "همه اساتید")
                {
                    query = query.Where(t => t.CooperationType == req.CooperationType);
                }

                var teachers = await query.ToListAsync();

                if (teachers.Count == 0)
                    return NotFound(new { message = "هیچ استادی یافت نشد" });

                var days = new[] { "شنبه", "یکشنبه", "دوشنبه", "سه‌شنبه", "چهارشنبه", "پنجشنبه", "جمعه" };

                var locks = new List<ScheduleLock>();

                foreach (var teacher in teachers)
                {
                    foreach (var day in days)
                    {
                        locks.Add(new ScheduleLock
                        {
                            Username = req.Username,
                            FullName = req.FullName,
                            CenterCode = req.CenterCode,
                            TeacherCode = teacher.Code,
                            DayOfWeek = day,
                            Term = req.Term,
                            Description = req.Description,
                            LockedAt = DateTime.UtcNow
                        });
                    }
                }

                await _context.ScheduleLocks.AddRangeAsync(locks);
                await _context.SaveChangesAsync();

                return Ok(new { message = "قفل‌گذاری گروهی انجام شد", count = locks.Count });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "خطا در قفل‌گذاری گروهی", error = ex.Message });
            }
        }


        [HttpPost("unlock-by-cooperation")]
        public async Task<IActionResult> UnlockByCooperation([FromBody] UnlockByCooperationRequest req)
        {
            try
            {
                if (string.IsNullOrEmpty(req.Term))
                    return BadRequest(new { message = "ترم الزامی است" });

                IQueryable<Teacher> query = _context.Teachers;

                // اگر نوع همکاری = همه اساتید بود → بدون فیلتر
                if (!string.IsNullOrEmpty(req.CooperationType) && req.CooperationType != "همه اساتید")
                {
                    query = query.Where(t => t.CooperationType == req.CooperationType);
                }

                var teacherCodes = await query
                    .Select(t => t.Code)
                    .ToListAsync();

                if (teacherCodes.Count == 0)
                    return NotFound(new { message = "هیچ استادی یافت نشد" });

                var locks = await _context.ScheduleLocks
                    .Where(l => l.Term == req.Term && teacherCodes.Contains(l.TeacherCode))
                    .ToListAsync();

                if (locks.Count == 0)
                    return NotFound(new { message = "هیچ قفلی برای حذف یافت نشد" });

                _context.ScheduleLocks.RemoveRange(locks);
                await _context.SaveChangesAsync();

                return Ok(new { message = "حذف گروهی قفل‌ها انجام شد", count = locks.Count });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "خطا در حذف گروهی قفل‌ها", error = ex.Message });
            }
        }



        public class LockByCooperationRequest
        {
            public string CooperationType { get; set; }
            public string Term { get; set; }
            public string Username { get; set; }
            public string FullName { get; set; }
            public string CenterCode { get; set; }
            public string? Description { get; set; }
        }

        public class UnlockByCooperationRequest
        {
            public string CooperationType { get; set; }
            public string Term { get; set; }
        }
    }

    

}
