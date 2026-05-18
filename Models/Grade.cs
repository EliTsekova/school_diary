using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace school_diary.Models;

public class Grade
{
    [Key]
    public int Id { get; set; }

    [Required]
    [Range(2, 6)]
    public int Value { get; set; }

    [Required]
    [ForeignKey(nameof(Student))]
    public int StudentId { get; set; }

    public Student Student { get; set; } = null!;

    [ForeignKey(nameof(Teacher))]
    public int? TeacherId { get; set; }

    [InverseProperty(nameof(Models.Teacher.Grades))]
    public Teacher? Teacher { get; set; }

    [Required]
    [ForeignKey(nameof(Subject))]
    public int SubjectId { get; set; }

    public Subject Subject { get; set; } = null!;

    [Required]
    [DataType(DataType.DateTime)]
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
}