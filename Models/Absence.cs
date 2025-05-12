 using System.ComponentModel.DataAnnotations;
 using System.ComponentModel.DataAnnotations.Schema;
namespace school_diary.Models
{
   

    public class Absence
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Student")]
        public int StudentId { get; set; }
        public Student Student { get; set; }

        [ForeignKey("Teacher")]
        public int TeacherId { get; set; }
        public Teacher Teacher { get; set; }

        [ForeignKey("Subject")]
        public int SubjectId { get; set; }
        public Subject Subject { get; set; }

        [Required]
        public DateTime AbsenceDate { get; set; }

        public bool IsExcused { get; set; }
    }


}
