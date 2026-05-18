using System.ComponentModel.DataAnnotations;

namespace school_diary.Dtos;

public class CreateClassDto
{
    [Required]
    public int SchoolId { get; set; }

    [Required]
    [StringLength(10)]
    public string Name { get; set; } = null!;
}