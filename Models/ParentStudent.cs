using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace school_diary.Models
{
    public class ParentStudent
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Parent")]
        public int ParentId { get; set; }

        public Parent Parent { get; set; }

        [Required]
        [ForeignKey("Student")]
        public int StudentId { get; set; }

        public Student Student { get; set; }
    }
}
