
namespace school_diary.Dtos
{
    public record CreatedStudentDto(
        int Id,
        string FullName,
        string Email,
        int SchoolId,
        string ClassName,
        string InitialPassword  
    );
}