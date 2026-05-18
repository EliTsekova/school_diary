namespace school_diary.Dtos;

public record TeacherDto(
    int Id,
    string FullName,
    string Email,
    int SchoolId,
    IReadOnlyList<int> SubjectIds,
    IReadOnlyList<int> ClassIds
);