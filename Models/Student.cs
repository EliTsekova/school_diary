using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace school_diary.Models
{
    public class Student
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }

        [ForeignKey("School")]
        public int SchoolId { get; set; }
        public School School { get; set; }

        public string ClassName { get; set; }

        public ICollection<ParentStudent> ParentStudents { get; set; }
        public ICollection<Grade> Grades { get; set; }
        public ICollection<Absence> Absences { get; set; }
    }

}
