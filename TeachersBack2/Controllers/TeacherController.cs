using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using TeachersBack2.Data;
using TeachersBack2.Models;

[Authorize(Roles = "admin,centerAdmin")]
[ApiController]
[Route("api/teachers")]
public class TeacherController : ControllerBase
{
    private readonly AppDbContext _context;

    public TeacherController(AppDbContext context)
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

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);

            int addedCount = 0;
            int duplicateCount = 0;
            int errorCount = 0;

            foreach (var row in worksheet.RowsUsed().Skip(1))
            {
                var code = row.Cell(1).GetString().Trim();
                var fname = row.Cell(2).GetString().Trim();
                var lname = row.Cell(3).GetString().Trim();
                var fullName = row.Cell(4).GetString().Trim();

                bool isEmpty = string.IsNullOrWhiteSpace(code)
                            && string.IsNullOrWhiteSpace(fname)
                            && string.IsNullOrWhiteSpace(lname)
                            && string.IsNullOrWhiteSpace(fullName);

                if (isEmpty)
                {
                    errorCount++;
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(code))
                {
                    bool exists = await _context.Teachers.AnyAsync(t => t.Code == code);
                    if (exists)
                    {
                        duplicateCount++;
                        continue;
                    }
                }

                var teacher = new Teacher
                {
                    Code = code,
                    Fname = fname,
                    Lname = lname,
                    FullName = fullName,
                    Email = row.Cell(5).GetString().Trim(),
                    Mobile = row.Cell(6).GetString().Trim(),
                    FieldOfStudy = row.Cell(7).GetString().Trim(),
                    Center = row.Cell(8).GetString().Trim(),
                    CooperationType = row.Cell(9).GetString().Trim(),
                    AcademicRank = row.Cell(10).GetString().Trim(),
                    ExecutivePosition = row.Cell(11).GetString().Trim(),
                    Degree = row.Cell(14).GetString().Trim()
                };

                _context.Teachers.Add(teacher);
                addedCount++;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                addedCount,
                duplicateCount,
                errorCount
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"خطا در پردازش فایل: {ex.Message}");
        }
    }

    // 🔍 خواندن استاد بر اساس کد
    [HttpGet("by-code/{code}")]
    public async Task<IActionResult> GetByCode(string code)
    {
        try
        {
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Code == code);
            return teacher == null ? NotFound("استاد یافت نشد") : Ok(teacher);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"خطا در دریافت اطلاعات: {ex.Message}");
        }
    }

    // 📄 دریافت همه اساتید
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var teachers = await _context.Teachers.ToListAsync();
            return Ok(teachers);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"خطا در دریافت لیست: {ex.Message}");
        }
    }

    // 📄 دریافت صفحه‌بندی‌شده
    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged(
        int page = 1,
        int pageSize = 30,
        string search = "",
        string cooperationType = "",
        string center = "",
        string fieldOfStudy = ""
    )
    {
        try
        {
            var query = _context.Teachers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string term = search.Trim();
                query = query.Where(t =>
                    (t.Code != null && t.Code.Contains(term)) ||
                    (t.Fname != null && t.Fname.Contains(term)) ||
                    (t.Lname != null && t.Lname.Contains(term))
                );
            }

            if (!string.IsNullOrWhiteSpace(cooperationType))
                query = query.Where(t => t.CooperationType == cooperationType);

            if (!string.IsNullOrWhiteSpace(center))
                query = query.Where(t => t.Center != null && t.Center.Contains(center));

            if (!string.IsNullOrWhiteSpace(fieldOfStudy))
                query = query.Where(t => t.FieldOfStudy != null && t.FieldOfStudy.Contains(fieldOfStudy));

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(t => t.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new { totalCount, items });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"خطا در دریافت اطلاعات: {ex.Message}");
        }
    }

    // 📄 دریافت استاد با آیدی
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var teacher = await _context.Teachers.FindAsync(id);
            return teacher == null ? NotFound("استاد یافت نشد") : Ok(teacher);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"خطا در دریافت اطلاعات: {ex.Message}");
        }
    }

    // ➕ افزودن استاد جدید
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Teacher model)
    {
        try
        {
            _context.Teachers.Add(model);
            await _context.SaveChangesAsync();
            return Ok(model);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"خطا در افزودن استاد: {ex.Message}");
        }
    }

    // ✏️ ویرایش استاد
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Teacher model)
    {
        try
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null) return NotFound("استاد یافت نشد");

            teacher.Code = model.Code;
            teacher.Fname = model.Fname;
            teacher.Lname = model.Lname;
            teacher.FullName = model.FullName;
            teacher.Email = model.Email;
            teacher.Mobile = model.Mobile;
            teacher.FieldOfStudy = model.FieldOfStudy;
            teacher.Center = model.Center;
            teacher.CooperationType = model.CooperationType;
            teacher.AcademicRank = model.AcademicRank;
            teacher.ExecutivePosition = model.ExecutivePosition;
            teacher.Degree = model.Degree;

            await _context.SaveChangesAsync();
            return Ok(teacher);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"خطا در بروزرسانی استاد: {ex.Message}");
        }
    }

    // ❌ حذف استاد
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null) return NotFound("استاد یافت نشد");

            _context.Teachers.Remove(teacher);
            await _context.SaveChangesAsync();
            return Ok("استاد حذف شد");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"خطا در حذف استاد: {ex.Message}");
        }
    }

    // 🔍 جستجو بر اساس نام
    [HttpGet("search-by-name/{name}")]
    public async Task<IActionResult> SearchByName(string name)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("عبارت جستجو نباید خالی باشد");

            var results = await _context.Teachers
                .Where(t => t.Fname.Contains(name) || t.Lname.Contains(name))
                .ToListAsync();

            return Ok(results);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"خطا در جستجو: {ex.Message}");
        }
    }
}
