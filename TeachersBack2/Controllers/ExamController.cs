using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeachersBack2.Data;
using TeachersBack2.Models;

namespace TeachersBack2.Controllers
{
    [ApiController]
    [Route("api/exams")]
    [Authorize(Roles = "admin,centerAdmin,programmer,teacher")]
    public class ExamController : Controller
    {
        private readonly AppDbContext _context;

        public ExamController(AppDbContext context)
        {
            _context = context;
        }

        //آپلود فایل امتحانات
        [HttpPost("upload-excel")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UploadExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("فایل معتبر نیست");

            try
            {
                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                stream.Position = 0;

                using var workbook = new XLWorkbook(stream);
                var worksheet = workbook.Worksheet(1);

                var examsToAdd = new List<Exam>();

                // برای بررسی تکراری بودن: ترکیب کد مرکز + شماره و گروه درس
                var existingKeys = new HashSet<string>();

                // بارگذاری کلیدهای موجود از دیتابیس (برای جلوگیری از ثبت تکراری)
                var allExisting = await _context.Exams
                    .Select(x => new { x.CenterCode, x.LessonNoGrp })
                    .ToListAsync();

                foreach (var item in allExisting)
                {
                    var key = $"{item.CenterCode}_{item.LessonNoGrp}";
                    existingKeys.Add(key);
                }

                int addedCount = 0;
                int duplicateCount = 0;
                int errorCount = 0;

                var rows = worksheet.RowsUsed().Skip(1).ToList();
                int totalRows = rows.Count;

                for (int i = 0; i < rows.Count; i++)
                {
                    try
                    {
                        var row = rows[i];

                        // خواندن فیلدها بر اساس ترتیب ستون‌های اکسل
                        var centerCode = row.Cell(1).GetString().Trim();
                        var center = row.Cell(2).GetString().Trim();
                        var department = row.Cell(3).GetString().Trim();
                        var eduGrp = row.Cell(4).GetString().Trim();
                        var teacherCode = row.Cell(5).GetString().Trim();
                        var teacher = row.Cell(6).GetString().Trim();
                        var lessonNoGrp = row.Cell(7).GetString().Trim();
                        var lessonNo = row.Cell(8).GetString().Trim();
                        var lesson = row.Cell(9).GetString().Trim();
                        var totalUnit = row.Cell(10).GetString().Trim();
                        var practicalUnit = row.Cell(11).GetString().Trim();
                        var registered = row.Cell(12).GetString().Trim();
                        var sourceNo = row.Cell(13).GetString().Trim();
                        var attachNo = row.Cell(14).GetString().Trim();
                        var degree = row.Cell(15).GetString().Trim();
                        var teachersCenterCode = row.Cell(16).GetString().Trim();
                        var teachersCenter = row.Cell(17).GetString().Trim();
                        var mobile = row.Cell(18).GetString().Trim();
                        var cooperationType = row.Cell(19).GetString().Trim();
                        var examType = row.Cell(20).GetString().Trim();
                        var examDate = row.Cell(21).GetString().Trim();
                        var dayOfWeek = row.Cell(22).GetString().Trim();
                        var start = row.Cell(23).GetString().Trim();
                        var end = row.Cell(24).GetString().Trim();
                        var groupManager = row.Cell(25).GetString().Trim();
                        var questionDesigner = row.Cell(26).GetString().Trim();
                        var questionType = row.Cell(27).GetString().Trim();
                        var support = row.Cell(28).GetString().Trim();

                        // بررسی خالی بودن ردیف
                        if (string.IsNullOrWhiteSpace(centerCode) &&
                            string.IsNullOrWhiteSpace(teacherCode) &&
                            string.IsNullOrWhiteSpace(lessonNoGrp))
                        {
                            errorCount++;
                            continue;
                        }

                        // ✅ کلید یکتا: کد مرکز + شماره و گروه درس
                        var uniqueKey = $"{centerCode}_{lessonNoGrp}";

                        if (existingKeys.Contains(uniqueKey))
                        {
                            duplicateCount++;
                            continue;
                        }

                        var exam = new Exam
                        {
                            CenterCode = centerCode,
                            Center = center,
                            Department = department,
                            EduGrp = eduGrp,
                            TeacherCode = teacherCode,
                            Teacher = teacher,
                            LessonNoGrp = lessonNoGrp,
                            LessonNo = lessonNo,
                            Lesson = lesson,
                            TotalUnit = totalUnit,
                            PracticalUnit = practicalUnit,
                            Registered = string.IsNullOrWhiteSpace(registered) ? 0 : int.Parse(registered),
                            SourceNo = sourceNo,
                            AttachNo = attachNo,
                            Degree = degree,
                            TeachersCenterCode = teachersCenterCode,
                            TeachersCenter = teachersCenter,
                            Mobile = mobile,
                            CooperationType = cooperationType,
                            ExamType = examType,
                            ExamDate = examDate,
                            DayOfWeek = dayOfWeek,
                            Start = start,
                            End = end,
                            GroupManager = groupManager,
                            QuestionDesigner = questionDesigner,
                            QuestionType = questionType,
                            Support = support,
                            QuestionDesignerCode = ""
                        };

                        examsToAdd.Add(exam);
                        existingKeys.Add(uniqueKey);
                        addedCount++;

                        // ✅ ذخیره دسته‌ای هر 200 رکورد
                        if (examsToAdd.Count >= 200)
                        {
                            await _context.Exams.AddRangeAsync(examsToAdd);
                            await _context.SaveChangesAsync();
                            examsToAdd.Clear();
                        }
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        // خطا را لاگ کن اما ادامه بده
                        Console.WriteLine($"خطا در ردیف {i + 2}: {ex.Message}");
                    }
                }

                // ذخیره رکوردهای باقیمانده
                if (examsToAdd.Any())
                {
                    await _context.Exams.AddRangeAsync(examsToAdd);
                    await _context.SaveChangesAsync();
                }

                return Ok(new
                {
                    message = "بارگذاری فایل با موفقیت انجام شد",
                    totalRows,
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
        
        //پیدا کردن کد استادی طراح سوال
        [HttpPost("update-question-designer-code")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateQuestionDesignerCode()
        {
            try
            {
                // 1. دریافت تمام رکوردهایی که QuestionDesignerCode خالی دارند
                var exams = await _context.Exams
                    .Where(e => e.QuestionDesignerCode == "")
                    .ToListAsync();

                if (exams.Count == 0)
                    return Ok(new { message = "همه رکوردها قبلاً به‌روز شده‌اند", updatedCount = 0 });

                // 2. دریافت تمام اساتید برای مقایسه
                var teachers = await _context.Teachers
                    .Select(t => new { t.Code, t.Fname, t.Lname })
                    .ToListAsync();

                int updatedCount = 0;
                int notFoundCount = 0;
                int matchedFromSameTable = 0;
                int matchedFromTeachersTable = 0;

                foreach (var exam in exams)
                {
                    string foundTeacherCode = "";
                    string questionDesigner = exam.QuestionDesigner;

                    if (string.IsNullOrWhiteSpace(questionDesigner))
                        continue;

                    // مرحله 1: مقایسه با فیلد Teacher از همین جدول Exam
                    var matchFromExam = await _context.Exams
                        .Where(e => e.Teacher == questionDesigner && e.TeacherCode != "")
                        .Select(e => e.TeacherCode)
                        .FirstOrDefaultAsync();

                    if (!string.IsNullOrEmpty(matchFromExam))
                    {
                        foundTeacherCode = matchFromExam;
                        matchedFromSameTable++;
                    }
                    else
                    {
                        // مرحله 2: مقایسه با جدول Teachers (فیلدهای Fname و Lname)
                        // تطابق دقیق با "نام نام‌خانوادگی" یا "نام‌خانوادگی نام"
                        var matchedTeacher = teachers.FirstOrDefault(t =>
                            $"{t.Fname} {t.Lname}" == questionDesigner ||
                            $"{t.Lname} {t.Fname}" == questionDesigner ||
                            t.Fname == questionDesigner ||
                            t.Lname == questionDesigner);

                        if (matchedTeacher != null)
                        {
                            foundTeacherCode = matchedTeacher.Code;
                            matchedFromTeachersTable++;
                        }
                        else
                        {
                            notFoundCount++;
                        }
                    }

                    // به‌روزرسانی فیلد QuestionDesignerCode
                    if (!string.IsNullOrEmpty(foundTeacherCode))
                    {
                        exam.QuestionDesignerCode = foundTeacherCode;
                        updatedCount++;
                    }
                }

                // ذخیره تغییرات در دیتابیس
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "به‌روزرسانی کد طراح سوال انجام شد",
                    totalExams = exams.Count,
                    updatedCount,
                    matchedFromSameTable,
                    matchedFromTeachersTable,
                    notFoundCount
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "خطا در به‌روزرسانی", error = ex.Message });
            }
        }

        // نرمال کردن حروف فارسی
        [HttpGet("normalize")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> NormalizeExams()
        {
            try
            {
                var exams = await _context.Exams.ToListAsync();
                int updatedCount = 0;

                foreach (var e in exams)
                {
                    bool changed = false;

                    // نرمال کردن هر فیلد رشته‌ای
                    if (!string.IsNullOrEmpty(e.CenterCode))
                    {
                        var normalized = NormalizePersian(e.CenterCode);
                        if (e.CenterCode != normalized) { e.CenterCode = normalized; changed = true; }
                    }

                    if (!string.IsNullOrEmpty(e.Center))
                    {
                        var normalized = NormalizePersian(e.Center);
                        if (e.Center != normalized) { e.Center = normalized; changed = true; }
                    }

                    if (!string.IsNullOrEmpty(e.Department))
                    {
                        var normalized = NormalizePersian(e.Department);
                        if (e.Department != normalized) { e.Department = normalized; changed = true; }
                    }

                    if (!string.IsNullOrEmpty(e.EduGrp))
                    {
                        var normalized = NormalizePersian(e.EduGrp);
                        if (e.EduGrp != normalized) { e.EduGrp = normalized; changed = true; }
                    }

                    if (!string.IsNullOrEmpty(e.TeacherCode))
                    {
                        var normalized = NormalizePersian(e.TeacherCode);
                        if (e.TeacherCode != normalized) { e.TeacherCode = normalized; changed = true; }
                    }

                    if (!string.IsNullOrEmpty(e.Teacher))
                    {
                        var normalized = NormalizePersian(e.Teacher);
                        if (e.Teacher != normalized) { e.Teacher = normalized; changed = true; }
                    }

                    if (!string.IsNullOrEmpty(e.LessonNoGrp))
                    {
                        var normalized = NormalizePersian(e.LessonNoGrp);
                        if (e.LessonNoGrp != normalized) { e.LessonNoGrp = normalized; changed = true; }
                    }

                    if (!string.IsNullOrEmpty(e.LessonNo))
                    {
                        var normalized = NormalizePersian(e.LessonNo);
                        if (e.LessonNo != normalized) { e.LessonNo = normalized; changed = true; }
                    }

                    if (!string.IsNullOrEmpty(e.Lesson))
                    {
                        var normalized = NormalizePersian(e.Lesson);
                        if (e.Lesson != normalized) { e.Lesson = normalized; changed = true; }
                    }

                    if (!string.IsNullOrEmpty(e.TotalUnit))
                    {
                        var normalized = NormalizePersian(e.TotalUnit);
                        if (e.TotalUnit != normalized) { e.TotalUnit = normalized; changed = true; }
                    }

                    if (!string.IsNullOrEmpty(e.PracticalUnit))
                    {
                        var normalized = NormalizePersian(e.PracticalUnit);
                        if (e.PracticalUnit != normalized) { e.PracticalUnit = normalized; changed = true; }
                    }

                    if (!string.IsNullOrEmpty(e.SourceNo))
                    {
                        var normalized = NormalizePersian(e.SourceNo);
                        if (e.SourceNo != normalized) { e.SourceNo = normalized; changed = true; }
                    }

                    if (!string.IsNullOrEmpty(e.AttachNo))
                    {
                        var normalized = NormalizePersian(e.AttachNo);
                        if (e.AttachNo != normalized) { e.AttachNo = normalized; changed = true; }
                    }

                    if (!string.IsNullOrEmpty(e.Degree))
                    {
                        var normalized = NormalizePersian(e.Degree);
                        if (e.Degree != normalized) { e.Degree = normalized; changed = true; }
                    }

                    if (!string.IsNullOrEmpty(e.TeachersCenterCode))
                    {
                        var normalized = NormalizePersian(e.TeachersCenterCode);
                        if (e.TeachersCenterCode != normalized) { e.TeachersCenterCode = normalized; changed = true; }
                    }

                    if (!string.IsNullOrEmpty(e.TeachersCenter))
                    {
                        var normalized = NormalizePersian(e.TeachersCenter);
                        if (e.TeachersCenter != normalized) { e.TeachersCenter = normalized; changed = true; }
                    }

                    if (!string.IsNullOrEmpty(e.Mobile))
                    {
                        var normalized = NormalizePersian(e.Mobile);
                        if (e.Mobile != normalized) { e.Mobile = normalized; changed = true; }
                    }

                    if (!string.IsNullOrEmpty(e.CooperationType))
                    {
                        var normalized = NormalizePersian(e.CooperationType);
                        if (e.CooperationType != normalized) { e.CooperationType = normalized; changed = true; }
                    }

                    if (!string.IsNullOrEmpty(e.ExamType))
                    {
                        var normalized = NormalizePersian(e.ExamType);
                        if (e.ExamType != normalized) { e.ExamType = normalized; changed = true; }
                    }

                    if (!string.IsNullOrEmpty(e.ExamDate))
                    {
                        var normalized = NormalizePersian(e.ExamDate);
                        if (e.ExamDate != normalized) { e.ExamDate = normalized; changed = true; }
                    }

                    if (!string.IsNullOrEmpty(e.DayOfWeek))
                    {
                        var normalized = NormalizePersian(e.DayOfWeek);
                        if (e.DayOfWeek != normalized) { e.DayOfWeek = normalized; changed = true; }
                    }

                    if (!string.IsNullOrEmpty(e.Start))
                    {
                        var normalized = NormalizePersian(e.Start);
                        if (e.Start != normalized) { e.Start = normalized; changed = true; }
                    }

                    if (!string.IsNullOrEmpty(e.End))
                    {
                        var normalized = NormalizePersian(e.End);
                        if (e.End != normalized) { e.End = normalized; changed = true; }
                    }

                    if (!string.IsNullOrEmpty(e.GroupManager))
                    {
                        var normalized = NormalizePersian(e.GroupManager);
                        if (e.GroupManager != normalized) { e.GroupManager = normalized; changed = true; }
                    }

                    if (!string.IsNullOrEmpty(e.QuestionDesigner))
                    {
                        var normalized = NormalizePersian(e.QuestionDesigner);
                        if (e.QuestionDesigner != normalized) { e.QuestionDesigner = normalized; changed = true; }
                    }

                    if (!string.IsNullOrEmpty(e.QuestionType))
                    {
                        var normalized = NormalizePersian(e.QuestionType);
                        if (e.QuestionType != normalized) { e.QuestionType = normalized; changed = true; }
                    }

                    if (!string.IsNullOrEmpty(e.Support))
                    {
                        var normalized = NormalizePersian(e.Support);
                        if (e.Support != normalized) { e.Support = normalized; changed = true; }
                    }

                    if (!string.IsNullOrEmpty(e.QuestionDesignerCode))
                    {
                        var normalized = NormalizePersian(e.QuestionDesignerCode);
                        if (e.QuestionDesignerCode != normalized) { e.QuestionDesignerCode = normalized; changed = true; }
                    }

                    if (changed)
                        updatedCount++;
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "نرمال‌سازی فیلدهای رشته‌ای انجام شد",
                    totalExams = exams.Count,
                    updatedCount
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "خطا در نرمال‌سازی", error = ex.Message });
            }
        }

        // تابع کمکی نرمال‌سازی حروف فارسی
        private string NormalizePersian(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            return input
                .Replace('ي', 'ی')  // ی عربی → ی فارسی
                .Replace('ك', 'ک')  // ک عربی → ک فارسی
                .Replace('ة', 'ه')  // تاء گرد → ه
                .Replace('أ', 'ا')  // الف با همزه → ا
                .Replace('إ', 'ا')  // الف با همزه → ا
                .Replace('ؤ', 'و')  // واو با همزه → و
                .Replace('ئ', 'ی'); // ی با همزه → ی
        }
             
        // ================================
        // 1. دریافت همه امتحانات
        // ================================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var exams = await _context.Exams
                    .OrderBy(e => e.ExamDate)
                    .ThenBy(e => e.Start)
                    .ToListAsync();

                return Ok(new
                {
                    totalCount = exams.Count,
                    items = exams
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "خطا در دریافت لیست امتحانات", error = ex.Message });
            }
        }

        // ================================
        // 2. صفحه‌بندی شده با فیلترهای پیشرفته
        // ================================
        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged(
            int page = 1,
            int pageSize = 50,
            string search = "",
            string centerCode = "",
            string teacherCode = "",
            string lessonNo = "",
            string examDate = "",
            string examType = "",
            string questionDesigner = "",
            string sourceNo = "",
            string dayOfWeek = "",
            bool hidePastDates = false  // پارامتر جدید
        )
        {
            try
            {
                var query = _context.Exams.AsQueryable();

                // فیلتر جستجو (فقط کد استاد یا نام استاد)
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(e =>
                        e.TeacherCode.Contains(search) ||
                        e.Teacher.Contains(search)
                    );
                }

                // فیلتر بر اساس مرکز (کد مرکز یا نام مرکز)
                if (!string.IsNullOrWhiteSpace(centerCode))
                {
                    query = query.Where(e =>
                        e.CenterCode.Contains(centerCode) ||
                        e.Center.Contains(centerCode)
                    );
                }

                // فیلتر بر اساس کد استاد
                if (!string.IsNullOrWhiteSpace(teacherCode))
                {
                    query = query.Where(e => e.TeacherCode.Contains(teacherCode));
                }

                // فیلتر بر اساس کد درس
                if (!string.IsNullOrWhiteSpace(lessonNo))
                {
                    query = query.Where(e => e.LessonNoGrp.Contains(lessonNo));
                }

                // فیلتر بر اساس تاریخ امتحان
                if (!string.IsNullOrWhiteSpace(examDate))
                {
                    query = query.Where(e => e.ExamDate.Contains(examDate));
                }

                // فیلتر بر اساس نوع امتحان
                if (!string.IsNullOrWhiteSpace(examType))
                {
                    query = query.Where(e => e.ExamType.Contains(examType));
                }

                // فیلتر بر اساس طراح سوال
                if (!string.IsNullOrWhiteSpace(questionDesigner))
                {
                    query = query.Where(e => e.QuestionDesigner.Contains(questionDesigner));
                }

                // فیلتر بر اساس شماره منبع
                if (!string.IsNullOrWhiteSpace(sourceNo))
                {
                    query = query.Where(e => e.SourceNo.Contains(sourceNo));
                }

                // فیلتر بر اساس روز هفته
                if (!string.IsNullOrWhiteSpace(dayOfWeek))
                {
                    query = query.Where(e => e.DayOfWeek.Contains(dayOfWeek));
                }

                // ✅ فیلتر عدم نمایش تاریخ‌های گذشته
                if (hidePastDates)
                {
                    var today = DateTime.Now.ToString("yyyy/MM/dd");
                    query = query.Where(e =>
                        e.ExamDate != null &&
                        string.Compare(e.ExamDate, today) >= 0
                    );
                }

                var totalCount = await query.CountAsync();

                // مرتب‌سازی: تاریخ‌های معتبر اول، سپس بر اساس تاریخ و ساعت
                var items = await query
                    .OrderByDescending(e => e.ExamDate != null && e.ExamDate.Length >= 8)
                    .ThenBy(e => e.ExamDate)
                    .ThenBy(e => e.Start)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return Ok(new
                {
                    totalCount,
                    page,
                    pageSize,
                    totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                    items
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "خطا در دریافت لیست صفحه‌بندی شده", error = ex.Message });
            }
        }

        // ================================
        // 3. دریافت امتحانات یک استاد (بر اساس کد استاد)
        // ================================
        [HttpGet("teacher/{teacherCode}")]
        public async Task<IActionResult> GetByTeacherCode(string teacherCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(teacherCode))
                    return BadRequest(new { message = "کد استاد الزامی است" });

                var exams = await _context.Exams
                    .Where(e => e.TeacherCode == teacherCode)
                    .OrderBy(e => e.ExamDate)
                    .ThenBy(e => e.Start)
                    .ToListAsync();

                return Ok(new
                {
                    teacherCode,
                    count = exams.Count,
                    items = exams
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "خطا در دریافت امتحانات استاد", error = ex.Message });
            }
        }

        // ================================
        // 4. دریافت امتحانات یک مرکز (بر اساس کد مرکز)
        // ================================
        [HttpGet("center/{centerCode}")]
        public async Task<IActionResult> GetByCenterCode(string centerCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(centerCode))
                    return BadRequest(new { message = "کد مرکز الزامی است" });

                var exams = await _context.Exams
                    .Where(e => e.CenterCode == centerCode)
                    .OrderBy(e => e.ExamDate)
                    .ThenBy(e => e.Start)
                    .ToListAsync();

                return Ok(new
                {
                    centerCode,
                    count = exams.Count,
                    items = exams
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "خطا در دریافت امتحانات مرکز", error = ex.Message });
            }
        }

        // ================================
        // 5. دریافت یک امتحان با شناسه
        // ================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var exam = await _context.Exams.FindAsync(id);

                if (exam == null)
                    return NotFound(new { message = "امتحان یافت نشد" });

                return Ok(exam);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "خطا در دریافت امتحان", error = ex.Message });
            }
        }

        // ================================
        // 6. ویرایش امتحان
        // ================================
        [HttpPut("{id}")]
        [Authorize(Roles = "admin,centerAdmin")]
        public async Task<IActionResult> Update(int id, [FromBody] Exam updatedExam)
        {
            try
            {
                var exam = await _context.Exams.FindAsync(id);

                if (exam == null)
                    return NotFound(new { message = "امتحان یافت نشد" });

                // به‌روزرسانی فیلدها
                exam.CenterCode = updatedExam.CenterCode ?? "";
                exam.Center = updatedExam.Center ?? "";
                exam.Department = updatedExam.Department ?? "";
                exam.EduGrp = updatedExam.EduGrp ?? "";
                exam.TeacherCode = updatedExam.TeacherCode ?? "";
                exam.Teacher = updatedExam.Teacher ?? "";
                exam.LessonNoGrp = updatedExam.LessonNoGrp ?? "";
                exam.LessonNo = updatedExam.LessonNo ?? "";
                exam.Lesson = updatedExam.Lesson ?? "";
                exam.TotalUnit = updatedExam.TotalUnit ?? "";
                exam.PracticalUnit = updatedExam.PracticalUnit ?? "";
                exam.Registered = updatedExam.Registered;
                exam.SourceNo = updatedExam.SourceNo ?? "";
                exam.AttachNo = updatedExam.AttachNo ?? "";
                exam.Degree = updatedExam.Degree ?? "";
                exam.TeachersCenterCode = updatedExam.TeachersCenterCode ?? "";
                exam.TeachersCenter = updatedExam.TeachersCenter ?? "";
                exam.Mobile = updatedExam.Mobile ?? "";
                exam.CooperationType = updatedExam.CooperationType ?? "";
                exam.ExamType = updatedExam.ExamType ?? "";
                exam.ExamDate = updatedExam.ExamDate ?? "";
                exam.DayOfWeek = updatedExam.DayOfWeek ?? "";
                exam.Start = updatedExam.Start ?? "";
                exam.End = updatedExam.End ?? "";
                exam.GroupManager = updatedExam.GroupManager ?? "";
                exam.QuestionDesigner = updatedExam.QuestionDesigner ?? "";
                exam.QuestionType = updatedExam.QuestionType ?? "";
                exam.Support = updatedExam.Support ?? "";
                exam.QuestionDesignerCode = updatedExam.QuestionDesignerCode ?? "";

                await _context.SaveChangesAsync();

                return Ok(new { message = "امتحان با موفقیت به‌روزرسانی شد", exam });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "خطا در به‌روزرسانی امتحان", error = ex.Message });
            }
        }

        // ================================
        // 7. حذف امتحان
        // ================================
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var exam = await _context.Exams.FindAsync(id);

                if (exam == null)
                    return NotFound(new { message = "امتحان یافت نشد" });

                _context.Exams.Remove(exam);
                await _context.SaveChangesAsync();

                return Ok(new { message = "امتحان با موفقیت حذف شد" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "خطا در حذف امتحان", error = ex.Message });
            }
        }

        // ================================
        // 8. حذف گروهی امتحانات (اختیاری)
        // ================================
        [HttpDelete("truncate")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> TruncateExams()
        {
            try
            {
                // اجرای دستور TRUNCATE TABLE
                await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE Exams");

                return Ok(new { message = "تمامی رکوردهای امتحانات با موفقیت حذف شدند و شمارنده Identity بازنشانی گردید" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"خطا در حذف کامل: {ex.Message}" });
            }
        }
        // ================================
        // دریافت امتحانات طراح سوال  
        // ================================
        [HttpGet("question-designer")]
        public async Task<IActionResult> GetByQuestionDesigner(
            string designer,      // الزامی - کد استاد یا نام طراح سوال
            string examDate = "") // اختیاری - اگر وارد شود فقط آن تاریخ را نشان می‌دهد
        {
            try
            {
                if (string.IsNullOrWhiteSpace(designer))
                    return BadRequest(new { message = "کد یا نام طراح سوال الزامی است" });

                IQueryable<Exam> query = _context.Exams.AsQueryable();

                // تشخیص اینکه ورودی کد استاد است یا نام
                // اگر ورودی 6 کاراکتر و فقط عدد باشد => کد استاد
                bool isTeacherCode = designer.Length == 6 && designer.All(char.IsDigit);

                if (isTeacherCode)
                {
                    // جستجوی دقیق بر اساس کد استاد (QuestionDesignerCode)
                    query = query.Where(e => e.QuestionDesignerCode == designer);
                }
                else
                {
                    // جستجوی جزئی بر اساس نام طراح سوال (QuestionDesigner)
                    query = query.Where(e => e.QuestionDesigner.Contains(designer));
                }

                // فیلتر بر اساس تاریخ (اگر وارد شده باشد)
                if (!string.IsNullOrWhiteSpace(examDate))
                {
                    query = query.Where(e => e.ExamDate == examDate);
                }

                // مرتب‌سازی بر اساس تاریخ و ساعت شروع
                var exams = await query
                    .OrderBy(e => e.ExamDate)
                    .ThenBy(e => e.Start)
                    .ToListAsync();

                if (!exams.Any())
                {
                    string message = string.IsNullOrWhiteSpace(examDate)
                        ? $"هیچ امتحانی برای طراح سوال '{designer}' یافت نشد"
                        : $"هیچ امتحانی برای طراح سوال '{designer}' در تاریخ '{examDate}' یافت نشد";

                    return NotFound(new { message });
                }

                return Ok(new
                {
                    searchValue = designer,
                    searchType = isTeacherCode ? "teacherCode" : "name",
                    examDate = string.IsNullOrWhiteSpace(examDate) ? "همه تاریخ‌ها" : examDate,
                    count = exams.Count,
                    items = exams
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "خطا در دریافت امتحانات طراح سوال", error = ex.Message });
            }
        }

        // ================================
        // دریافت دروسی که طراح سوال دارند ولی کد طراح سوال ندارند
        // ================================
        [HttpGet("missing-designer-code/simple")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetMissingDesignerCodeSimple()
        {
            try
            {
                var exams = await _context.Exams
                    .Where(e => e.QuestionDesigner != "" && e.QuestionDesignerCode == "")
                    .OrderBy(e => e.QuestionDesigner)
                    .ThenBy(e => e.ExamDate)
                    .ToListAsync();

                return Ok(new
                {
                    count = exams.Count,
                    items = exams
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "خطا در دریافت اطلاعات", error = ex.Message });
            }
        }
    }
}


