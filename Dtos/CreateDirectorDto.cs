namespace school_diary.Dtos;

using System.ComponentModel.DataAnnotations;

public record CreateDirectorDto
{
    [Required] public string UserId  { get; init; } = default!;
    [Required] public int    SchoolId { get; init; }
}