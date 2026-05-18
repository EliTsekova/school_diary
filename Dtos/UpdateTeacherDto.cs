using System.ComponentModel.DataAnnotations;

namespace school_diary.Dtos;

public record UpdateTeacherDto(
    [Required] string FirstName,
    [Required] string LastName,
    [Required, EmailAddress] string Email,
    [Required] int SchoolId,
    List<int> SubjectIds,
    List<int> ClassIds,
    string? NewPassword
);