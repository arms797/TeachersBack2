namespace TeachersBack2.Models
{
    public class ComponentFeature
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!; // مثال: "examSeat", "teacherSchedule"
        public string Description { get; set; }
        public bool IsActive { get; set; } = false;
    }
}
