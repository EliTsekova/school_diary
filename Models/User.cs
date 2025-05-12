    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
namespace school_diary.Models
{
    public enum Role
    {
        Admin,
        Teacher,
        Parent,
        Student,
        Director
    }

    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        public string LastName { get; set; }

        [Required]
        [StringLength(255)]
        public string Email { get; set; }

        public string PasswordHash { get; set; }

        [Required]
        public Role Role { get; set; }

        public Teacher Teacher { get; set; }
        public Parent Parent { get; set; }
        public Student Student { get; set; }
        public Director Director { get; set; }
    }

}
