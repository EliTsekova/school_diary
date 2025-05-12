using System.ComponentModel.DataAnnotations;
using System.IO;

namespace school_diary.Models
{
    public class School
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Address { get; set; }

        public ICollection<Teacher> Teachers { get; set; }
        public ICollection<Student> Students { get; set; }
        public Director Director { get; set; }
    }


}
