using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeachersBack2.Data;
using TeachersBack2.Models;


[ApiController]
[Route("api/[controller]")]
public class TeacherTermController : ControllerBase
{
    private readonly AppDbContext _context;

    public TeacherTermController(AppDbContext context)
    {
        _context = context;
    }

    [Authorize]
    [HttpGet("{code}/{term}")]
    public async Task<IActionResult> GetByCodeAndTerm(string code, string term)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(term))
                return BadRequest(new { message = "کد استاد یا ترم معتبر نیست." });

            var item = await _context.TeacherTerms
                .FirstOrDefaultAsync(t => t.Code == code && t.Term == term);

            if (item == null)
                return NotFound(new { message = "رکوردی با این مشخصات یافت نشد." });

            return Ok(item);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "خطا در دریافت اطلاعات ترمی.", detail = ex.Message });
        }
    }

    [Authorize(Roles = "admin,teacher")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateByCodeAndTerm(int id, [FromBody] TeacherTerm updated)
    {
        try
        {
            //if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(term))
            //    return BadRequest(new { message = "کد استاد یا ترم معتبر نیست." });

            var item = await _context.TeacherTerms.FirstOrDefaultAsync(t => t.Id==id);

            if (item == null)
                return NotFound(new { message = "رکوردی با این مشخصات یافت نشد." });

            // فقط فیلدهای قابل ویرایش
            item.IsNeighborTeaching = updated.IsNeighborTeaching;
            item.NeighborTeaching = updated.NeighborTeaching;
            item.NeighborCenters = updated.NeighborCenters;
            item.Suggestion = updated.Suggestion;
            item.Projector = updated.Projector;
            item.Whiteboard2 = updated.Whiteboard2;

            await _context.SaveChangesAsync();
            return Ok(new { message = "اطلاعات ترمی با موفقیت ویرایش شد." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "خطا در ویرایش اطلاعات ترمی.", detail = ex.Message });
        }
    }

    [HttpPost("generate/{term}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GenerateTeacherTerms(string term)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(term))
                return BadRequest(new { message = "ترم معتبر ارسال نشده است." });

            var teachers = await _context.Teachers.ToListAsync();
            int successCount = 0;
            int errorCount = 0;

            foreach (var teacher in teachers)
            {
                try
                {
                    var newTerm = new TeacherTerm
                    {
                        Code = teacher.Code,
                        Term = term,
                        IsNeighborTeaching = false,
                        NeighborTeaching = "",
                        NeighborCenters = "",
                        Suggestion = "",
                        Projector = false,
                        Whiteboard2 = false
                    };

                    _context.TeacherTerms.Add(newTerm);
                    await _context.SaveChangesAsync();
                    successCount++;
                }
                catch
                {
                    errorCount++;
                }
            }

            return Ok(new
            {
                message = "ایجاد اطلاعات ترمی برای همه اساتید انجام شد.",
                successCount,
                errorCount
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "خطای کلی در عملیات ایجاد اطلاعات ترمی.", detail = ex.Message });
        }
    }
    


}
