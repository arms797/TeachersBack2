namespace TeachersBack2.Models
{
    public class WeeklySchedule
    {
        public int Id { get; set; } // آیدی اصلی
        public string TeacherCode { get; set; } // کد استاد
        public string DayOfWeek { get; set; } // روز هفته
        public string Center { get; set; } // مرکز
        public string A { get; set; } //08-10
        public string B { get; set; } //10-12
        public string C { get; set; } //12-14
        public string D { get; set; } //14-16
        public string E { get; set; } //16-18

        public string Description { get; set; } // توضیحات
        public string AlternativeHours { get; set; } // ساعات جایگزین
        public string ForbiddenHours { get; set; } // ساعات ممنوع
        public string Term {  get; set; } // ترم تحصیلی        
    }

}
