using System.ComponentModel.DataAnnotations;

namespace school_diary.Dtos;

public record CreateStudentDto(
    [Required] string FirstName,
    [Required] string LastName,
    [Required] string ClassName,
    [Required, EmailAddress] string Email,
    [Required] int SchoolId
);