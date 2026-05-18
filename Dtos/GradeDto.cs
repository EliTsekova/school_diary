namespace school_diary.Dtos;

public record GradeDto(
    int Id,
    int Value,
    int StudentId,
    int TeacherId,
    int SubjectId,
    DateTime CreatedOn);