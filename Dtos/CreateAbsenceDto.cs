using System;
using System.ComponentModel.DataAnnotations;

namespace school_diary.Dtos;

public record CreateAbsenceDto(
    [Required]                      int      StudentId,
    [Required]                      int      SubjectId,
    [Required, DataType(DataType.Date)]      DateTime Date,
    bool                            IsExcused);