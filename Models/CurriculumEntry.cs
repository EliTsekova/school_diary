using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace school_diary.Models;

public class CurriculumEntry
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int CurriculumId { get; set; }

    public Curriculum Curriculum { get; set; } = null!;

    [Required]
    public int SubjectId { get; set; }

    public Subject Subject { get; set; } = null!;

    [Required]
    public int TeacherId { get; set; }

    public Teacher Teacher { get; set; } = null!;

    [Required]
    [StringLength(20, MinimumLength = 3)]
    public string DayOfWeek { get; set; } = "";

    [Required]
    [Range(1, 10)]
    public int Period { get; set; }
}