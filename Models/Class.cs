using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace school_diary.Models;

public class Class
{
    [Key]
    public int Id { get; set; }

    [Required]
    [ForeignKey(nameof(School))]
    public int SchoolId { get; set; }

    public School School { get; set; } = null!;
    
    [Required]
    [StringLength(10, MinimumLength = 1)]
    public string Name { get; set; } = null!;

    public ICollection<Student> Students { get; set; } = new List<Student>();
    public ICollection<Curriculum> Curricula { get; set; } = new List<Curriculum>();
    public ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();
}