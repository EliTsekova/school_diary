using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace school_diary.Models
{
    public class TeacherSubject
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Teacher")]
        public int TeacherId { get; set; }

        public Teacher Teacher { get; set; }

        [Required]
        [ForeignKey("Subject")]
        public int SubjectId { get; set; }

        public Subject Subject { get; set; }
    }
}
