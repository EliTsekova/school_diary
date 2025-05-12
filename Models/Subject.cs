using System.ComponentModel.DataAnnotations;

namespace school_diary.Models
{
    public class Subject
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public ICollection<TeacherSubject> TeacherSubjects { get; set; }
    }

}
