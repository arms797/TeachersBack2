namespace TeachersBack2.Models
{
    public class ChangeHistory
    {
        public int Id { get; set; }

        public string TableName { get; set; }   // نام جدول اصلی
        public int RecordId { get; set; }       // شناسه رکورد اصلی

        public string DayOfWeek { get; set; } = "";//روز برنامه هفتگی استاد

        public string ColumnName { get; set; }  // ستون تغییر یافته
        public string OldValue { get; set; }    // مقدار قبلی
        public string NewValue { get; set; }    // مقدار جدید

        public string ChangedBy { get; set; }   // شخص تغییر دهنده
        public DateTime ChangedAt { get; set; } // زمان تغییر
    }
}
