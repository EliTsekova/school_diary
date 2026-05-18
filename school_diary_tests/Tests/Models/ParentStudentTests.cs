using System.ComponentModel.DataAnnotations;
using school_diary.Models;

namespace school_diary.school_diary_tests.Tests.Models;

public class ParentStudentTests
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
    public void ParentStudent_IsValid_WhenRequiredFieldsAreSet()
    {
        var model = new ParentStudent
        {
            ParentId = 1,
            StudentId = 1
        };

        var results = ValidateModel(model);

        Assert.Empty(results);
    }

    [Fact]
    public void ParentStudent_DefaultValues_AreCorrect()
    {
        var model = new ParentStudent();

        Assert.Equal(0, model.Id);
        Assert.Equal(0, model.ParentId);
        Assert.Equal(0, model.StudentId);
    }

    [Fact]
    public void ParentStudent_CanStoreParentNavigationProperty()
    {
        var parent = new Parent
        {
            Id = 1,
            UserId = "parent-user-1"
        };

        var model = new ParentStudent
        {
            ParentId = parent.Id,
            StudentId = 1,
            Parent = parent
        };

        Assert.Equal(parent, model.Parent);
        Assert.Equal(1, model.Parent.Id);
    }

    [Fact]
    public void ParentStudent_CanStoreStudentNavigationProperty()
    {
        var student = new Student
        {
            Id = 1
        };

        var model = new ParentStudent
        {
            ParentId = 1,
            StudentId = student.Id,
            Student = student
        };

        Assert.Equal(student, model.Student);
        Assert.Equal(1, model.Student.Id);
    }
}