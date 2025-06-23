namespace school_diary.Dtos;

public record StudentDto(
    int    Id,
    string FullName,
    string Email,
    string ClassName,
    int    SchoolId);