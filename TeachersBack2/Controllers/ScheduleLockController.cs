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
    }
}
