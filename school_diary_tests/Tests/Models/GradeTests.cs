using System.ComponentModel.DataAnnotations;
using school_diary.Models;

namespace school_diary.school_diary_tests.Tests.Models;

public class GradeTests
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
    public void Grade_IsValid_WhenAllRequiredFieldsAreSet()
    {
        var model = new Grade
        {
            Value = 6,
            StudentId = 1,
            SubjectId = 1,
            TeacherId = 1,
            CreatedOn = DateTime.UtcNow
        };

        var results = ValidateModel(model);

        Assert.Empty(results);
    }

    [Fact]
    public void Grade_IsInvalid_WhenValueIsBelowRange()
    {
        var model = new Grade
        {
            Value = 1,
            StudentId = 1,
            SubjectId = 1,
            CreatedOn = DateTime.UtcNow
        };

        var results = ValidateModel(model);

        Assert.NotEmpty(results);
    }

    [Fact]
    public void Grade_IsInvalid_WhenValueIsAboveRange()
    {
        var model = new Grade
        {
            Value = 7,
            StudentId = 1,
            SubjectId = 1,
            CreatedOn = DateTime.UtcNow
        };

        var results = ValidateModel(model);

        Assert.NotEmpty(results);
    }

    [Fact]
    public void Grade_DefaultCreatedOn_IsSet()
    {
        var model = new Grade();

        Assert.NotEqual(default, model.CreatedOn);
    }

    [Fact]
    public void Grade_TeacherId_CanBeNull()
    {
        var model = new Grade
        {
            Value = 5,
            StudentId = 1,
            SubjectId = 1,
            TeacherId = null,
            CreatedOn = DateTime.UtcNow
        };

        var results = ValidateModel(model);

        Assert.Empty(results);
    }

    [Fact]
    public void Grade_CanStoreStudentNavigationProperty()
    {
        var student = new Student
        {
            Id = 1
        };

        var model = new Grade
        {
            Value = 6,
            StudentId = student.Id,
            SubjectId = 1,
            Student = student,
            CreatedOn = DateTime.UtcNow
        };

        Assert.Equal(student, model.Student);
        Assert.Equal(1, model.Student.Id);
    }

    [Fact]
    public void Grade_CanStoreTeacherNavigationProperty()
    {
        var teacher = new Teacher
        {
            Id = 1
        };

        var model = new Grade
        {
            Value = 6,
            StudentId = 1,
            SubjectId = 1,
            TeacherId = teacher.Id,
            Teacher = teacher,
            CreatedOn = DateTime.UtcNow
        };

        Assert.Equal(teacher, model.Teacher);
        Assert.Equal(1, model.Teacher!.Id);
    }

    [Fact]
    public void Grade_CanStoreSubjectNavigationProperty()
    {
        var subject = new Subject
        {
            Id = 1,
            Name = "Math"
        };

        var model = new Grade
        {
            Value = 6,
            StudentId = 1,
            SubjectId = subject.Id,
            Subject = subject,
            CreatedOn = DateTime.UtcNow
        };

        Assert.Equal(subject, model.Subject);
        Assert.Equal("Math", model.Subject.Name);
    }
}