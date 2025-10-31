using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using TeachersBack2.Data;
using TeachersBack2.Models;
using System.Transactions;


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
                var fname = row.Cell(2).GetString().Trim();
                var lname = row.Cell(3).GetString().Trim();

                bool isEmpty = string.IsNullOrWhiteSpace(code)
                            && string.IsNullOrWhiteSpace(fname)
                            && string.IsNullOrWhiteSpace(lname);

                if (isEmpty)
                {
                    errorCount++;
                    continue;
                }

                var existingTeacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Code == code);
                int teacherId;

                if (existingTeacher != null)
                {
                    teacherId = existingTeacher.Id;
                    duplicateCount++;
                }
                else
                {
                        string p = row.Cell(4).GetString().Trim();
                        string pass = p != "" ? p : "Spnu123";
                    var teacher = new Teacher
                    {
                        Code = code,
                        Fname = fname,
                        Lname = lname,
                        NationalCode = row.Cell(4).GetString().Trim(),
                        Email = row.Cell(5).GetString().Trim(),
                        Mobile = row.Cell(6).GetString().Trim(),
                        FieldOfStudy = row.Cell(7).GetString().Trim(),
                        Center = row.Cell(8).GetString().Trim(),
                        CooperationType = row.Cell(9).GetString().Trim(),
                        AcademicRank = row.Cell(10).GetString().Trim(),
                        ExecutivePosition = row.Cell(11).GetString().Trim(),
                        PasswordHash=BCrypt.Net.BCrypt.HashPassword(pass)
                        
                    };

                    _context.Teachers.Add(teacher);
                    await _context.SaveChangesAsync();
                    teacherId = teacher.Id;
                    addedCount++;
               
                    var termCode = row.Cell(18).GetString().Trim();
                    bool termExists = await _context.TeacherTerms.AnyAsync(tt =>
                        tt.Code == code && tt.Term == termCode);

                    if (termExists)
                    {
                        skippedTermCount++;
                        continue;
                    }

                    var teacherTerm = new TeacherTerm
                    {
                        Code = code,
                        Term = termCode,
                        IsNeighborTeaching = row.Cell(12).GetString().ToLower().Trim() == "false",
                        NeighborTeaching = row.Cell(13).GetString().Trim(),
                        NeighborCenters = row.Cell(14).GetString().Trim(),
                        Suggestion = row.Cell(15).GetString().Trim(),
                        Projector = row.Cell(16).GetString().ToLower().Trim() == "false",
                        Whiteboard2 = row.Cell(17).GetString().ToLower().Trim() == "false"
                    };

                    _context.TeacherTerms.Add(teacherTerm);
                    await _context.SaveChangesAsync();
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

    // 🔍 خواندن استاد بر اساس کد
    /*
    [HttpGet("by-code/{code}")]
    [Authorize(Roles = "admin,centerAdmin,programmer")]
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
    */
    // 📄 دریافت همه اساتید
    [HttpGet]
    [Authorize(Roles = "admin,centerAdmin,programmer")]
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
    [Authorize(Roles = "admin,centerAdmin,programmer")]
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
    /*
    
    [HttpGet("{id}")]
    [Authorize(Roles = "admin,centerAdmin,programmer")]
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
    */
    // ➕ افزودن استاد جدید
    [HttpPost]
    [Authorize(Roles = "admin,centerAdmin,programmer")]
    public async Task<IActionResult> Create([FromBody] TeacherCreateDto dto)
    {

        string pass = dto.NationalCode!=null ? dto.NationalCode : "Spnu123";
        var teacher = new Teacher
        {
            Code = dto.Code,
            Fname = dto.Fname,
            Lname = dto.Lname,
            Email = dto.Email,
            Mobile = dto.Mobile,
            FieldOfStudy = dto.FieldOfStudy,
            Center = dto.Center,
            CooperationType = dto.CooperationType,
            AcademicRank = dto.AcademicRank,
            ExecutivePosition = dto.ExecutivePosition,
            NationalCode=dto.NationalCode,
            PasswordHash=BCrypt.Net.BCrypt.HashPassword(pass)
        };

        _context.Teachers.Add(teacher);
        await _context.SaveChangesAsync();
        return Ok(teacher);
    }


    // ✏️ ویرایش استاد
    [HttpPut("{id}")]
    [Authorize(Roles = "admin,centerAdmin")]
    public async Task<IActionResult> Update(int id, [FromBody] Teacher model)
    {
        try
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null) return NotFound("استاد یافت نشد");

            teacher.Code = model.Code;
            teacher.Fname = model.Fname;
            teacher.Lname = model.Lname;
            //teacher.FullName = model.FullName;
            teacher.Email = model.Email;
            teacher.Mobile = model.Mobile;
            teacher.FieldOfStudy = model.FieldOfStudy;
            teacher.Center = model.Center;
            teacher.CooperationType = model.CooperationType;
            teacher.AcademicRank = model.AcademicRank;
            teacher.ExecutivePosition = model.ExecutivePosition;
            teacher.NationalCode = model.NationalCode;
            //teacher.Degree = model.Degree;

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
    [Authorize(Roles = "admin")]
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
    /*
    [HttpGet("search-by-name/{name}")]
    [Authorize(Roles = "admin,centerAdmin,programmer")]
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
    */
}
