using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

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
        [StringLength(100, MinimumLength = 2)]
        public string FirstName { get; set; } = null!;

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string LastName { get; set; } = null!;

        [Required]
        public Role Role { get; set; }
    }
}