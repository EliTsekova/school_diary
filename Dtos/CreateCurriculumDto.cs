using System.ComponentModel.DataAnnotations;

namespace school_diary.Dtos;

public record CreateCurriculumDto(
    [Required, StringLength(100)] string Term,
    [Required, Range(1, int.MaxValue)] int ClassId,
    [Required, MinLength(1)] List<CreateCurriculumEntryDto> Entries
);