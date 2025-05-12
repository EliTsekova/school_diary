using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace school_diary.Models
{
    public class Teacher
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }

        [ForeignKey("School")]
        public int SchoolId { get; set; }
        public School School { get; set; }

        public ICollection<TeacherSubject> TeacherSubjects { get; set; }
        public ICollection<Grade> Grades { get; set; }
        public ICollection<Absence> Absences { get; set; }
    }

}
