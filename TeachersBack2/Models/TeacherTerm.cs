namespace TeachersBack2.Models
{
    public class TeacherTerm
    {
        public int Id { get; set; }
        public string Code {  get; set; }
        public string Term {  get; set; }//ترم تحصیلی
        public bool IsNeighborTeaching { get; set; } // تدریس همجوار
        public string NeighborTeaching { get;set; }//دلایل تدریس همجوار
        public string NeighborCenters { get; set; } // مراکز همجوار
        public string Suggestion { get; set; } // پیشنهاد
        public bool Projector { get; set; }// ویدئو پروژکتور
        public bool Whiteboard2 { get; set; } // دو وایت برده
    }
}
