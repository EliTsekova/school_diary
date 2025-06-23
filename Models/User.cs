using Microsoft.AspNetCore.Identity;
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

    public class User : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = null!;

        [Required]
        public Role Role { get; set; }

        public Teacher Teacher { get; set; } = null!;
        public Parent Parent { get; set; } = null!;
        public Student Student { get; set; } = null!;
        public Director Director { get; set; } = null!;
    }
}