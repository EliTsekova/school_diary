using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace school_diary.Models
{
    public class Parent
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("User")]
        public string UserId { get; set; }

        public User User { get; set; }

        public ICollection<ParentStudent> ParentStudents { get; set; } = new List<ParentStudent>();
    }
}