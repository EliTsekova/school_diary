using System.ComponentModel.DataAnnotations;

namespace school_diary.Dtos;

public record UpdateAbsenceDto(
    [Required] int      SubjectId,
    [Required] DateTime Date,
    bool                        IsExcused);