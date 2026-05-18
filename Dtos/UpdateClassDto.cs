using System.ComponentModel.DataAnnotations;

namespace school_diary.DTOs;

public class UpdateClassDto
{
    [Required]
    [StringLength(10)]
    public string Name { get; set; } = null!;
}