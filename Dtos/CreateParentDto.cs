using System.ComponentModel.DataAnnotations;

namespace school_diary.Dtos;

public record CreateParentDto(
    [Required] string FirstName,
    [Required] string LastName,
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password,
    List<int> StudentIds
);