namespace school_diary.Dtos;

public class ClassDto
{
    public int Id { get; set; }
    public int SchoolId { get; set; }
    public string Name { get; set; } = null!;

    public string? SchoolName { get; set; }
    public int StudentsCount { get; set; }
}