using System.ComponentModel.DataAnnotations;

namespace TeachersBack2.Models
{
    public class TermCalender
    {
        [Key]
        public string Term {  get; set; }
        public string Title { get; set; }
        public string Start {  get; set; }
        public string End { get; set; }
    }
}
