namespace TeachersBack2.Models
{
    public class Teacher
    {
        public int Id { get; set; } // آیدی اصلی
        public string Code { get; set; } // کد استاد
        public string Fname {  get; set; }//نام
        public string Lname {  get; set; }//نام خانوادگی
        public string FullName { get; set; } // نام کامل
        public string Email { get; set; }
        public string Mobile { get; set; }

        public string FieldOfStudy { get; set; } // رشته تحصیلی
        public string Center { get; set; } // مرکز

        public string CooperationType { get; set; } // نوع همکاری: "عضو هیات علمی" یا "مدرس مدعو"
        public string AcademicRank { get; set; } // مرتبه علمی
        public string ExecutivePosition { get; set; } // پست اجرایی

        public bool IsNeighborTeaching { get; set; } // تدریس همجوار
        public string NeighborCenters { get; set; } // مراکز همجوار

        public string Degree { get; set; } // مدرک تحصیلی
        public string Suggestion { get; set; } // پیشنهاد
        public string Term {  get; set; }//ترم تحصیلی
        public bool Projector {  get; set; }// ویدئو پروژکتور
        public bool Whiteboard2 {  get; set; } // دو وایت برده
    }

}
