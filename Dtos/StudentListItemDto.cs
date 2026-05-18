namespace school_diary.Dtos
{
    public record StudentListItemDto(
        int Id,
        string FullName,
        string Email,
        string SchoolName,
        string ClassName
    );
}
