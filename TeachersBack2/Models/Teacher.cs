namespace TeachersBack2.Models
{
    public class Teacher
    {
        public int Id { get; set; } // آیدی اصلی
        public string Code { get; set; } // کد استاد
        public string Fname {  get; set; }//نام
        public string Lname {  get; set; }//نام خانوادگی
        public string Email { get; set; }
        public string Mobile { get; set; }

        public string FieldOfStudy { get; set; } // رشته تحصیلی
        public string Center { get; set; } // مرکز

        public string CooperationType { get; set; } // نوع همکاری: "عضو هیات علمی" یا "مدرس مدعو"
        public string AcademicRank { get; set; } // مرتبه علمی یا مدرک تحصیلی
        public string ExecutivePosition { get; set; } // پست اجرایی
        public string NationalCode { get; set; }//کد ملی
        public string PasswordHash {  get; set; }//رمز هش شده

        //public ICollection<TeacherTerm> TeacherTerms { get; set; }
    }

}
