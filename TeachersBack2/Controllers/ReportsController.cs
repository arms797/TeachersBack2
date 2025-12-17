using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeachersBack2.Data;
using TeachersBack2.Models;

namespace TeachersBack2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "admin,centerAdmin")]
    public class ReportsController : Controller
    {
        private readonly AppDbContext _context;

        public ReportsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("WeeklyChangesByTerm/{term}")]
        public async Task<IActionResult> WeeklyChangesByTerm(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return BadRequest("ترم معتبر نیست");

            // تغییرات برنامه هفتگی در ترم مورد نظر
            var changes = await _context.ChangeHistory
                .Where(ch => ch.TableName == "WeeklySchedules"
                             && ch.ChangedBy != "admin"
                             && _context.WeeklySchedules
                                 .Any(ws => ws.Id == ch.RecordId && ws.Term == term))
                .Select(ch => ch.RecordId)
                .Distinct()
                .ToListAsync();

            // پیدا کردن کد اساتید مربوط به این تغییرات
            var teacherCodes = await _context.WeeklySchedules
                .Where(ws => changes.Contains(ws.Id))
                .Select(ws => ws.TeacherCode)
                .Distinct()
                .ToListAsync();

            // گرفتن اطلاعات استادان تغییر داده
            var changedTeachers = await _context.Teachers
                .Where(t => teacherCodes.Contains(t.Code))
                .ToListAsync();

            // گرفتن تعداد کل اساتید به تفکیک نوع همکاری
            var totalByCooperation = await _context.Teachers
                .GroupBy(t => t.CooperationType)
                .Select(g => new { CooperationType = g.Key, TotalCount = g.Count() })
                .ToListAsync();

            // ترکیب نتایج
            var result = changedTeachers
                .GroupBy(t => t.CooperationType)
                .Select(g => new
                {
                    CooperationType = g.Key,
                    ChangedCount = g.Count(),
                    TotalCount = totalByCooperation
                        .FirstOrDefault(tc => tc.CooperationType == g.Key)?.TotalCount ?? 0
                })
                .ToList();

            return Ok(result);
        }

        [HttpGet("TeachersByCooperation/{term}/{cooperationType}/{completed}")]
        public async Task<IActionResult> TeachersByCooperation(string term, string cooperationType, bool completed)
        {
            if (string.IsNullOrWhiteSpace(term) || string.IsNullOrWhiteSpace(cooperationType))
                return BadRequest("پارامترهای ورودی معتبر نیستند");

            // پیدا کردن کد اساتیدی که تغییر داده‌اند (یعنی فرم تکمیل کرده‌اند)
            var changedTeacherCodes = await _context.ChangeHistory
                .Where(ch => ch.TableName == "WeeklySchedules"
                             && ch.ChangedBy != "admin"
                             && _context.WeeklySchedules
                                 .Any(ws => ws.Id == ch.RecordId && ws.Term == term))
                .Select(ch => ch.RecordId)
                .Distinct()
                .Join(_context.WeeklySchedules,
                      recordId => recordId,
                      ws => ws.Id,
                      (recordId, ws) => ws.TeacherCode)
                .Distinct()
                .ToListAsync();

            // همه اساتید با نوع همکاری مورد نظر
            var allTeachers = await _context.Teachers
                .Where(t => t.CooperationType == cooperationType)
                .ToListAsync();

            // فیلتر بر اساس completed
            List<Teacher> result;
            if (completed)
            {
                result = allTeachers.Where(t => changedTeacherCodes.Contains(t.Code)).ToList();
            }
            else
            {
                result = allTeachers.Where(t => !changedTeacherCodes.Contains(t.Code)).ToList();
            }

            // Join با جدول Centers برای گرفتن عنوان مرکز
            var output = result
                .Join(_context.Centers,
                      teacher => teacher.Center,   // فرض: Teacher.Center همان CenterCode است
                      center => center.CenterCode,
                      (teacher, center) => new
                      {
                          teacher.Code,
                          teacher.Fname,
                          teacher.Lname,
                          teacher.Mobile,
                          CenterTitle = center.Title,   // عنوان مرکز
                          teacher.CooperationType,
                          teacher.NationalCode
                      });

            return Ok(output);
        }

        [HttpGet("TeacherChanges/{term}/{teacherCode}")]
        public async Task<IActionResult> TeacherChanges(string term, string teacherCode)
        {
            if (string.IsNullOrWhiteSpace(term) || string.IsNullOrWhiteSpace(teacherCode))
                return BadRequest("پارامترهای ورودی معتبر نیستند");

            // آی‌دی‌های برنامه‌های هفتگی استاد در ترم مشخص
            var scheduleIds = await _context.WeeklySchedules
                .Where(ws => ws.Term == term && ws.TeacherCode == teacherCode)
                .Select(ws => ws.Id)
                .ToListAsync();

            if (!scheduleIds.Any())
                return NotFound("هیچ برنامه‌ای برای این استاد در این ترم یافت نشد");

            // تغییرات مرتبط در ChangeHistory (به جز تغییرات admin)
            var changes = await _context.ChangeHistory
                .Where(ch => ch.TableName == "WeeklySchedules"
                             && scheduleIds.Contains(ch.RecordId)
                             && ch.ChangedBy != "admin")
                .OrderByDescending(ch => ch.ChangedAt) // فقط بر اساس زمان تغییرات مرتب شود
                .Select(ch => new
                {
                    ch.Id,
                    ch.RecordId,
                    ch.ColumnName,
                    ch.OldValue,
                    ch.NewValue,
                    ch.ChangedBy,
                    ch.ChangedAt
                })
                .ToListAsync();

            return Ok(changes);
        }

        [HttpGet("TeachersByCenterDay/{term}/{centerCode}/{dayOfWeek}/{cooperationType}")]
        public async Task<IActionResult> TeachersByCenterDay(
      string term,
      string centerCode,
      string dayOfWeek,
      string cooperationType)
        {
            if (string.IsNullOrWhiteSpace(term) ||
                string.IsNullOrWhiteSpace(centerCode) ||
                string.IsNullOrWhiteSpace(dayOfWeek) ||
                string.IsNullOrWhiteSpace(cooperationType))
            {
                return BadRequest("پارامترهای ورودی معتبر نیستند");
            }

            const string AbsentText = "عدم حضور در دانشگاه";

            var schedules = await _context.WeeklySchedules
                .Where(ws =>
                    ws.Term == term &&
                    ws.Center == centerCode &&
                    ws.DayOfWeek == dayOfWeek &&
                    (
                        (!string.IsNullOrWhiteSpace(ws.A) && ws.A.Trim() != AbsentText) ||
                        (!string.IsNullOrWhiteSpace(ws.B) && ws.B.Trim() != AbsentText) ||
                        (!string.IsNullOrWhiteSpace(ws.C) && ws.C.Trim() != AbsentText) ||
                        (!string.IsNullOrWhiteSpace(ws.D) && ws.D.Trim() != AbsentText) ||
                        (!string.IsNullOrWhiteSpace(ws.E) && ws.E.Trim() != AbsentText)
                    )
                )
                .ToListAsync();

            var result = (from ws in schedules
                          join t in _context.Teachers on ws.TeacherCode equals t.Code
                          where t.CooperationType == cooperationType
                          select new
                          {
                              t.Code,
                              t.Fname,
                              t.Lname,
                              t.Mobile,
                              ws.A,
                              ws.B,
                              ws.C,
                              ws.D,
                              ws.E
                          })
                          .OrderBy(r => r.Lname).ThenBy(r => r.Fname)
                          .ToList();

            return Ok(result);
        }



    }
}
