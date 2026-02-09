namespace TeachersBack2.Models
{
    public class ScheduleLock
    {
        public int Id { get; set; } // آیدی اصلی رکورد قفل

        // اطلاعات کاربر قفل‌کننده
        public string Username { get; set; } = default!; // یوزرنیم کاربر قفل‌کننده
        public string CenterCode { get; set; } = default!; // مرکز کاربر قفل‌کننده
        public string FullName { get; set; } = default!; // نام و نام خانوادگی قفل‌کننده

        // اطلاعات استاد
        public string TeacherCode { get; set; } = default!; // کد استاد قفل‌شده
        public string DayOfWeek { get; set; } = default!;   // روز هفته‌ای که قفل شده

        // جزئیات قفل
        public DateTime LockedAt { get; set; } = DateTime.UtcNow; // تاریخ و ساعت قفل کردن
        public string? Description { get; set; } // توضیحات
        // ترم قفل گذاری
        public string Term  { get; set; }  
    }
}
