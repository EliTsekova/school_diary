using System.ComponentModel.DataAnnotations;

namespace school_diary.Dtos;

public record UpdateParentDto(
    [Required, StringLength(50)] string FirstName,
    [Required, StringLength(50)] string LastName,
    [Required, EmailAddress]     string Email);