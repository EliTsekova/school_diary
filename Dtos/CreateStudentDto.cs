// Dtos/CreateStudentDto.cs
using System.ComponentModel.DataAnnotations;

namespace school_diary.Dtos
{
    public record CreateStudentDto
    {
        [Required, StringLength(100)]
        public string FirstName { get; init; } = default!;

        [Required, StringLength(100)]
        public string LastName { get; init; } = default!;

        [Required, EmailAddress]
        public string Email { get; init; } = default!;

        [Required]
        public int SchoolId { get; init; }

        [Required, StringLength(10)]
        public string ClassName { get; init; } = default!;

        [Required, StringLength(100, MinimumLength = 6)]
        public string Password { get; init; } = default!;
    }
}