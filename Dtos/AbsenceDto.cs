namespace school_diary.Dtos;

public record AbsenceDto(
    int      Id,
    int      StudentId,
    int      SubjectId,
    string   TeacherId,
    DateTime Date,
    bool     IsExcused,
    DateTime? ExcusedOn);