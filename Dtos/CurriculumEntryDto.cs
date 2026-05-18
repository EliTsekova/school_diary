namespace school_diary.Dtos;

public record CurriculumEntryDto(
    int Id,
    int SubjectId,
    string SubjectName,
    int TeacherId,
    string TeacherName,
    string DayOfWeek,
    int Period
);