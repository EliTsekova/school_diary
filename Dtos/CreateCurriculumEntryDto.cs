using System.ComponentModel.DataAnnotations;

namespace school_diary.Dtos;

public record CreateCurriculumEntryDto(
    [Required, Range(1, int.MaxValue)]
    int SubjectId,

    [Required, Range(1, int.MaxValue)]
    int TeacherId,

    [Required]
    string DayOfWeek,

    [Required, Range(1, 12)]
    int Period
);