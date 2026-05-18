using System.ComponentModel.DataAnnotations;

namespace school_diary.Dtos;

public record CreateTeacherDto(
    [Required] string FirstName,
    [Required] string LastName,
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password,
    [Required] int SchoolId,
    List<int> SubjectIds,
    List<int> ClassIds
);