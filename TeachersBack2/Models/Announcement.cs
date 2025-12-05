namespace TeachersBack2.Models
{
    public class Announcement
    {
        public int Id { get; set; }
        public string Title { get; set; } = default!;
        public string Body { get; set; } = default!;
        public string? StartDate { get; set; }   // تاریخ شروع
        public string? EndDate { get; set; }     // تاریخ پایان
        public bool IsActive { get; set; } = true; // فعال؟
        public string? CreatedBy { get; set; }     // ایجاد کننده
        public string? CreateDate {  get; set; }
    }
}
