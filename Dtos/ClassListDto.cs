namespace school_diary.DTOs;

public class ClassListDto
{
    public int Id { get; set; }
    public int SchoolId { get; set; }
    public string Name { get; set; } = null!;

    public string? SchoolName { get; set; }  
    public int StudentsCount { get; set; }   
}