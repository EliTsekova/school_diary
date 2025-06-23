namespace school_diary.Dtos;
using System.ComponentModel.DataAnnotations;

public class UpdateStudentDto
{
    [Required, StringLength(100)] public string FirstName  { get; set; } = default!;
    [Required, StringLength(100)] public string LastName   { get; set; } = default!;
    [Required, EmailAddress]      public string Email      { get; set; } = default!;
    [Required, StringLength(50)]  public string ClassName  { get; set; } = default!;
    [Range(1,int.MaxValue)]       public int    SchoolId   { get; set; }
}