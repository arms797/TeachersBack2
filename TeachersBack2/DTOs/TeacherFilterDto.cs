namespace TeachersBack2.DTOs
{
    public class TeacherFilterDto
    {
        public string? Search { get; set; }
        public string? CooperationType { get; set; }
        public string? Center { get; set; }
        public string? FieldOfStudy { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 30;
    }

}
