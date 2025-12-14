using Microsoft.AspNetCore.Mvc;
using TeachersBack2.Models;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using TeachersBack2.Data;
using Microsoft.AspNetCore.Authorization;

namespace TeachersBack2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles ="admin,centerAdmin")]
    public class ExamSeatController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ExamSeatController(AppDbContext context)
        {
            _context = context;
        }

        // 1. خواندن خلاصه (روز، ساعت، تعداد افراد)
        [HttpGet]
        public IActionResult GetExams()
        {
            try
            {
                var summary = _context.ExamSeats
                .GroupBy(e => new { e.ExamDate, e.ExamTime })
                .Select(g => new
                {
                    Date = g.Key.ExamDate,
                    Time = g.Key.ExamTime,
                    Count = g.Count()
                })
                .OrderBy(s => s.Date).ThenBy(s => s.Time)
                .ToList();

                return Ok(summary);
            }
            catch (Exception ex) 
            {
                     return StatusCode(500, $"خطا در دریافت اطلاعات: {ex.Message}");
            }
            
        }

        // 2. افزودن رکوردها از فایل Excel با ClosedXML
        [HttpPost]
        public async Task<IActionResult> UploadExcel(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("فایل معتبر ارسال نشده است.");

                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);

                using var workbook = new XLWorkbook(stream);
                var worksheet = workbook.Worksheet(1); // اولین شیت
                var newRecords = new List<ExamSeat>();

                // فرض: ردیف اول هدر است
                foreach (var row in worksheet.RowsUsed().Skip(1))
                {
                    var record = new ExamSeat
                    {
                        StdNo = row.Cell(1).GetString(),
                        ShSh = row.Cell(2).GetString(),
                        SourceCenter = row.Cell(3).GetString(),
                        DestCenter = row.Cell(4).GetString(),
                        FName = row.Cell(5).GetString(),
                        LName = row.Cell(6).GetString(),
                        Degree = row.Cell(7).GetString(),
                        LessonCode = row.Cell(8).GetString(),
                        LessonTitle = row.Cell(9).GetString(),
                        ExamDate = row.Cell(10).GetString(),
                        ExamTime = row.Cell(11).GetString(),
                        SeatNumber = row.Cell(12).GetString(),
                        ExamType = row.Cell(13).GetString(),
                        LessonType = row.Cell(14).GetString(),
                        BuildingNo = row.Cell(15).GetString(),
                        Classroom = row.Cell(16).GetString(),
                        Row = row.Cell(17).GetString()
                    };
                    newRecords.Add(record);
                }

                _context.ExamSeats.AddRange(newRecords);
                await _context.SaveChangesAsync();

                return Ok($"{newRecords.Count} رکورد اضافه شد.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"خطا در دریافت اطلاعات: {ex.Message}");
            }

        }

        // 3. حذف رکوردها بر اساس تاریخ و ساعت
        [HttpDelete("{examDate}/{examTime}")]
        public async Task<IActionResult> DeleteByDateTime(string date, string time)
        {
            try
            {
                var records = _context.ExamSeats
                    .Where(e => e.ExamDate == date && e.ExamTime == time);

                if (!records.Any())
                    return NotFound("رکوردی یافت نشد.");

                _context.ExamSeats.RemoveRange(records);
                await _context.SaveChangesAsync();

                return Ok("رکوردهای متناظر حذف شدند.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"خطا در دریافت اطلاعات: {ex.Message}");
            }

        }

        // 4. دریافت رکوردهای یک دانشجو
        [HttpGet("{stdNo}")]
        [AllowAnonymous]
        public IActionResult GetByStudent(string stdNo)
        {
            try
            {
                var records = _context.ExamSeats
                .Where(e => e.StdNo == stdNo)
                .OrderBy(e => e.ExamDate).ThenBy(e => e.ExamTime)
                .ToList();

                if (!records.Any())
                    return NotFound("هیچ رکوردی برای این دانشجو یافت نشد.");

                return Ok(records);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"خطا در دریافت اطلاعات: {ex.Message}");
            }
            
        }
    }
}
