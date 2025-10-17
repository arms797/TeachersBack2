namespace TeachersBack2.Models
{
    public class WeeklySchedule
    {
        public int Id { get; set; } // آیدی اصلی

        public string TeacherCode { get; set; } // کد استاد
        public string DayOfWeek { get; set; } // روز هفته
        public string Center { get; set; } // مرکز

        public string A { get; set; }
        public string B { get; set; }
        public string C { get; set; }
        public string D { get; set; }
        public string E { get; set; }

        public string Description { get; set; } // توضیحات
        public string AlternativeHours { get; set; } // ساعات جایگزین
        public string ForbiddenHours { get; set; } // ساعات ممنوع
    }

}
