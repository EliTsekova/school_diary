using System.ComponentModel.DataAnnotations;

namespace school_diary.Models
{
    public class School
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = null!;

        [Required]
        [StringLength(200, MinimumLength = 5)]
        public string Address { get; set; } = null!;

        public ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();
        public ICollection<Student> Students { get; set; } = new List<Student>();

        public ICollection<Class> Classes { get; set; } = new List<Class>();

        public Director? Director { get; set; }
    }
}