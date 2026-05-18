using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace school_diary.Models
{
    public class Teacher
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = null!;

        [NotMapped]
        public User? User { get; set; }

        [Required]
        public int SchoolId { get; set; }

        public School School { get; set; } = null!;

        [StringLength(200)]
        public string AssignedClasses { get; set; } = "";

        public ICollection<TeacherSubject> TeacherSubjects { get; set; } = new List<TeacherSubject>();

        public ICollection<Grade> Grades { get; set; } = new List<Grade>();
    }
}