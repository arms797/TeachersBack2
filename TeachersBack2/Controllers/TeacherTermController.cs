using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeachersBack2.Data;
using TeachersBack2.Models;

[Authorize(Roles = "admin")]
[ApiController]
[Route("api/teacher-terms")]
public class TeacherTermController : ControllerBase
{
    private readonly AppDbContext _context;

    public TeacherTermController(AppDbContext context)
    {
        _context = context;
    }

    // 📄 دریافت همه اطلاعات ترمی
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var terms = await _context.TeacherTerms
                .Include(t => t.Teacher)
                .OrderByDescending(t => t.Term)
                .ToListAsync();

            return Ok(terms);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"خطا در دریافت اطلاعات ترمی: {ex.Message}");
        }
    }

    // 🔍 دریافت اطلاعات ترمی یک استاد خاص
    [HttpGet("by-teacher/{teacherId}")]
    public async Task<IActionResult> GetByTeacher(int teacherId)
    {
        try
        {
            var items = await _context.TeacherTerms
                .Where(t => t.TeacherId == teacherId)
                .OrderByDescending(t => t.Term)
                .ToListAsync();

            return Ok(items);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"خطا در دریافت اطلاعات ترمی استاد: {ex.Message}");
        }
    }

    // ➕ افزودن اطلاعات ترمی
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TeacherTerm model)
    {
        try
        {
            _context.TeacherTerms.Add(model);
            await _context.SaveChangesAsync();
            return Ok(model);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"خطا در افزودن اطلاعات ترمی: {ex.Message}");
        }
    }

    // ✏️ ویرایش اطلاعات ترمی
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] TeacherTerm model)
    {
        try
        {
            var term = await _context.TeacherTerms.FindAsync(id);
            if (term == null)
                return NotFound("رکورد ترمی یافت نشد");

            term.Term = model.Term;
            term.IsNeighborTeaching = model.IsNeighborTeaching;
            term.NeighborTeaching = model.NeighborTeaching;
            term.NeighborCenters = model.NeighborCenters;
            term.Suggestion = model.Suggestion;
            term.Projector = model.Projector;
            term.Whiteboard2 = model.Whiteboard2;

            await _context.SaveChangesAsync();
            return Ok(term);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"خطا در بروزرسانی اطلاعات ترمی: {ex.Message}");
        }
    }

    // ❌ حذف اطلاعات ترمی
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var term = await _context.TeacherTerms.FindAsync(id);
            if (term == null)
                return NotFound("رکورد ترمی یافت نشد");

            _context.TeacherTerms.Remove(term);
            await _context.SaveChangesAsync();
            return Ok("رکورد ترمی حذف شد");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"خطا در حذف اطلاعات ترمی: {ex.Message}");
        }
    }
}
