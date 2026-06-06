using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeachersBack2.Models
{
    [Table("Exams")]
    public class Exam
    {
        [Key]
        public int Id { get; set; } // شناسه داخلی

        [MaxLength(50)]
        public string CenterCode { get; set; } = ""; // کد مرکز

        [MaxLength(200)]
        public string Center { get; set; } = ""; // نام مرکز

        [MaxLength(200)]
        public string Department { get; set; } = ""; // دانشکده درس

        [MaxLength(200)]
        public string EduGrp { get; set; } = ""; // گروه آموزشی

        [MaxLength(50)]
        public string TeacherCode { get; set; } = ""; // کد استاد

        [MaxLength(200)]
        public string Teacher { get; set; } = ""; // نام استاد

        [MaxLength(100)]
        public string LessonNoGrp { get; set; } = ""; // شماره و گروه درس

        [MaxLength(50)]
        public string LessonNo { get; set; } = ""; // کد درس

        [MaxLength(200)]
        public string Lesson { get; set; } = ""; // نام درس

        [MaxLength(50)]
        public string TotalUnit { get; set; } = ""; // کل واحد
        [MaxLength(50)]
        public string PracticalUnit { get; set; } = ""; // واحد عملی

        public int Registered { get; set; } // تعداد ثبت‌نام شده

        [MaxLength(100)]
        public string SourceNo { get; set; } = ""; // شماره منبع

        [MaxLength(100)]
        public string AttachNo { get; set; } = ""; // کد شرح پیوست

        [MaxLength(50)]
        public string Degree { get; set; } = ""; // مقطع

        [MaxLength(50)]
        public string TeachersCenterCode { get; set; } = ""; // کد مرکز استاد

        [MaxLength(200)]
        public string TeachersCenter { get; set; } = ""; // نام مرکز استاد

        [MaxLength(20)]
        public string Mobile { get; set; } = ""; // تلفن همراه استاد

        [MaxLength(100)]
        public string CooperationType { get; set; } = ""; // وضعیت استخدام استاد (نوع همکاری)

        [MaxLength(100)]
        public string ExamType { get; set; } = ""; // نوع امتحان
        [MaxLength(10)]
        public string ExamDate { get; set; } = ""; // تاریخ امتحان

        [MaxLength(20)]
        public string DayOfWeek { get; set; } = ""; // روز هفته

        [MaxLength(10)]
        public string Start { get; set; } = ""; // ساعت شروع

        [MaxLength(10)]
        public string End { get; set; } = ""; // ساعت پایان

        [MaxLength(200)]
        public string GroupManager { get; set; } = ""; // نام مدیر گروه

        [MaxLength(200)]
        public string QuestionDesigner { get; set; } = ""; // طراح سوال

        [MaxLength(100)]
        public string QuestionType { get; set; } = ""; // نوع طراحی سوال

        [MaxLength(200)]
        public string Support { get; set; } = ""; // کارشناس پشتیبان

        [MaxLength(50)]
        public string QuestionDesignerCode { get; set; } = ""; // کد طراح سوال
    }
}