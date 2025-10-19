using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using TeachersBack2.Data;
using TeachersBack2.Models;

[Authorize(Roles = "admin")]
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
            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets[0];
            int rowCount = worksheet.Dimension.Rows;

            for (int row = 2; row <= rowCount; row++)
            {
                var teacher = new Teacher
                {
                    Code = worksheet.Cells[row, 1].Text,
                    Fname = worksheet.Cells[row, 2].Text,
                    Lname = worksheet.Cells[row, 3].Text,
                    FullName = worksheet.Cells[row, 4].Text,
                    Email = worksheet.Cells[row, 5].Text,
                    Mobile = worksheet.Cells[row, 6].Text,
                    FieldOfStudy = worksheet.Cells[row, 7].Text,
                    Center = worksheet.Cells[row, 8].Text,
                    CooperationType = worksheet.Cells[row, 9].Text,
                    AcademicRank = worksheet.Cells[row, 10].Text,
                    ExecutivePosition = worksheet.Cells[row, 11].Text,
                    IsNeighborTeaching = worksheet.Cells[row, 12].Text.ToLower() == "false",
                    NeighborCenters = worksheet.Cells[row, 13].Text,
                    Degree = worksheet.Cells[row, 14].Text,
                    Suggestion = worksheet.Cells[row, 15].Text,
                    Term = worksheet.Cells[row, 16].Text,
                    Projector = worksheet.Cells[row, 17].Text.ToLower() == "false",
                    Whiteboard2 = worksheet.Cells[row, 18].Text.ToLower() == "false"
                };

                _context.Teachers.Add(teacher);
            }

            await _context.SaveChangesAsync();
            return Ok("اطلاعات با موفقیت ثبت شد");
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
            teacher.IsNeighborTeaching = model.IsNeighborTeaching;
            teacher.NeighborCenters = model.NeighborCenters;
            teacher.Degree = model.Degree;
            teacher.Suggestion = model.Suggestion;
            teacher.Term = model.Term;
            teacher.Projector = model.Projector;
            teacher.Whiteboard2 = model.Whiteboard2;

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
    [HttpGet("search-by-name/{name}")]
    public async Task<IActionResult> SearchByName( string name)
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
