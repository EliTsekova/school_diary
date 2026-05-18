using System.ComponentModel.DataAnnotations;
using school_diary.Models;

namespace school_diary.school_diary_tests.Tests.Models;

public class AbsenceTests
{
    private static IList<ValidationResult> ValidateModel(object model)
    {
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();

        Validator.TryValidateObject(
            model,
            context,
            results,
            true);

        return results;
    }

    [Fact]
    public void Absence_IsValid_WhenRequiredFieldsAreSet()
    {
        var absence = new Absence
        {
            StudentId = 1,
            TeacherId = 1,
            SubjectId = 1,
            Date = DateTime.Today,
            IsExcused = false
        };

        var results = ValidateModel(absence);

        Assert.Empty(results);
    }

    [Fact]
    public void Absence_HasDefaultValues_WhenCreated()
    {
        var absence = new Absence();

        Assert.Equal(0, absence.Id);
        Assert.Equal(0, absence.StudentId);
        Assert.Equal(0, absence.TeacherId);
        Assert.Equal(0, absence.SubjectId);
        Assert.False(absence.IsExcused);
        Assert.Null(absence.ExcusedOn);
    }

    [Fact]
    public void Absence_CanBeExcused()
    {
        var date = DateTime.Today;

        var absence = new Absence
        {
            StudentId = 1,
            TeacherId = 1,
            SubjectId = 1,
            Date = date,
            IsExcused = true,
            ExcusedOn = date
        };

        Assert.True(absence.IsExcused);
        Assert.Equal(date, absence.ExcusedOn);
    }

    [Fact]
    public void Absence_StoresStudentAndSubjectNavigationProperties()
    {
        var student = new Student
        {
            Id = 1
        };

        var subject = new Subject
        {
            Id = 1,
            Name = "Math"
        };

        var absence = new Absence
        {
            StudentId = student.Id,
            TeacherId = 1,
            SubjectId = subject.Id,
            Student = student,
            Subject = subject,
            Date = DateTime.Today,
            IsExcused = false
        };

        Assert.Equal(student, absence.Student);
        Assert.Equal(subject, absence.Subject);
        Assert.Equal("Math", absence.Subject.Name);
    }
}