using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace school_diary.Models
{
    public class Director
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("User")]
        public string UserId { get; set; }

        public User User { get; set; }

        [Required]
        [ForeignKey("School")]
        public int SchoolId { get; set; }

        public School School { get; set; }
    }
}