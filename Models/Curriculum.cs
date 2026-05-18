using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace school_diary.Models;

public class Curriculum
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Term { get; set; } = null!;

    [Required]
    [ForeignKey(nameof(Class))]
    public int ClassId { get; set; }

    public Class Class { get; set; } = null!;

    public ICollection<CurriculumEntry> Entries { get; set; } = new List<CurriculumEntry>();
}