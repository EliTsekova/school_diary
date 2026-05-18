using System.ComponentModel.DataAnnotations;
using school_diary.Models;

namespace school_diary.school_diary_tests.Tests.Models;

public class TeacherSubjectTests
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
    public void TeacherSubject_IsValid_WhenRequiredFieldsAreSet()
    {
        var model = new TeacherSubject
        {
            TeacherId = 1,
            SubjectId = 1
        };

        var results = ValidateModel(model);

        Assert.Empty(results);
    }

    [Fact]
    public void TeacherSubject_DefaultValues_AreCorrect()
    {
        var model = new TeacherSubject();

        Assert.Equal(0, model.Id);
        Assert.Equal(0, model.TeacherId);
        Assert.Equal(0, model.SubjectId);
    }

    [Fact]
    public void TeacherSubject_CanStoreTeacherNavigationProperty()
    {
        var teacher = new Teacher
        {
            Id = 1,
            UserId = "teacher-user-1",
            SchoolId = 1
        };

        var model = new TeacherSubject
        {
            TeacherId = teacher.Id,
            SubjectId = 1,
            Teacher = teacher
        };

        Assert.Equal(teacher, model.Teacher);
        Assert.Equal(1, model.Teacher.Id);
    }

    [Fact]
    public void TeacherSubject_CanStoreSubjectNavigationProperty()
    {
        var subject = new Subject
        {
            Id = 1,
            Name = "Math"
        };

        var model = new TeacherSubject
        {
            TeacherId = 1,
            SubjectId = subject.Id,
            Subject = subject
        };

        Assert.Equal(subject, model.Subject);
        Assert.Equal("Math", model.Subject.Name);
    }
}