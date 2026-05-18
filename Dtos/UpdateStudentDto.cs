using System.ComponentModel.DataAnnotations;

namespace school_diary.Dtos
{
    public record UpdateStudentDto
    {
        [Required]
        public int Id { get; init; }

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
    }
}