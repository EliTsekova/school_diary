namespace school_diary.Dtos;

public record CurriculumDto(
    int Id,
    string Term,
    int SchoolId,              
    int ClassId,
    string ClassName,
    List<CurriculumEntryDto> Entries);