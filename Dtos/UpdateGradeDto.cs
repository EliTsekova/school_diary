using System.ComponentModel.DataAnnotations;

namespace school_diary.Dtos;

public record UpdateGradeDto(
    [Required, Range(2, 6)] int Value);