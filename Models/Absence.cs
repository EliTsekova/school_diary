using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace school_diary.Models;

public class Absence
{
    [Key]
    public int Id { get; set; }

    [Required]
    [ForeignKey(nameof(Student))]
    public int StudentId { get; set; }

    [Required]
    public int TeacherId { get; set; }

    [Required]
    [ForeignKey(nameof(Subject))]
    public int SubjectId { get; set; }

    public Student Student { get; set; } = null!;
    public Subject Subject { get; set; } = null!;

    [Required]
    [DataType(DataType.Date)]
    public DateTime Date { get; set; }

    [Required]
    public bool IsExcused { get; set; }

    [DataType(DataType.Date)]
    public DateTime? ExcusedOn { get; set; }
}