namespace school_diary.Dtos;

using System.ComponentModel.DataAnnotations;

public record CreateSchoolDto
{
    [Required, StringLength(255)] public string Name    { get; init; } = default!;
    [Required, StringLength(255)] public string Address { get; init; } = default!;
}