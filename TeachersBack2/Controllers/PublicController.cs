using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeachersBack2.Data;
using TeachersBack2.Models;

namespace TeachersBack2.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class PublicController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PublicController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("allTeachersSummary")]
        [AllowAnonymous] // دسترسی عمومی
        public async Task<IActionResult> GetActiveTermTeachers()
        {
            try
            {
                // پیدا کردن ترم فعال
                var activeTerm = await _context.TermCalenders.FirstOrDefaultAsync(t => t.Active);
                if (activeTerm == null)
                    return NotFound("ترم فعال یافت نشد.");

                string term = activeTerm.Term;

                // گرفتن داده‌ها
                var teachers = await _context.Teachers.ToListAsync();
                var teacherTerms = await _context.TeacherTerms
                    .Where(tt => tt.Term == term)
                    .ToListAsync();
                var weeklySchedules = await _context.WeeklySchedules
                    .Where(ws => ws.Term == term)
                    .ToListAsync();

                // ترکیب داده‌ها
                var result = teachers.Select(t => new
                {
                    Teacher = t, // کلیه فیلدهای جدول Teacher
                    TeacherTerm = teacherTerms.FirstOrDefault(tt => tt.Code == t.Code), // رکورد ترمی استاد
                    WeeklySchedules = weeklySchedules.Where(ws => ws.TeacherCode == t.Code).ToList() // لیست برنامه هفتگی استاد
                });

                return Ok(new
                {
                    ActiveTerm = activeTerm,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }

}


