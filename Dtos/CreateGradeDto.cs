using System.ComponentModel.DataAnnotations;

namespace school_diary.Dtos;

public record CreateGradeDto(
    [Required, Range(2, 6)]   int    Value,
    [Required]                int    StudentId,
    [Required]                int    SubjectId,
    int?   TeacherId = null);