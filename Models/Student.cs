using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace school_diary.Models
{
    public class Student
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey(nameof(User))]
        public string UserId { get; set; } = null!;

        public User User { get; set; } = null!;

        [ForeignKey(nameof(School))]
        public int? SchoolId { get; set; }

        public School? School { get; set; }

        [ForeignKey(nameof(Class))]
        public int? ClassId { get; set; }

        public Class? Class { get; set; }

        public ICollection<ParentStudent> ParentStudents { get; set; } = new List<ParentStudent>();
        public ICollection<Grade> Grades { get; set; } = new List<Grade>();
        public ICollection<Absence> Absences { get; set; } = new List<Absence>();
    }
}