namespace school_diary.school_diary_tests.Tests.Models;
using System.ComponentModel.DataAnnotations;
using school_diary.Models;


public class ClassModelTests
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
    public void Class_IsValid_WhenRequiredFieldsAreSet()
    {
        var model = new Class
        {
            SchoolId = 1,
            Name = "8A"
        };

        var results = ValidateModel(model);

        Assert.Empty(results);
    }

    [Fact]
    public void Class_IsInvalid_WhenNameIsMissing()
    {
        var model = new Class
        {
            SchoolId = 1,
            Name = ""
        };

        var results = ValidateModel(model);

        Assert.NotEmpty(results);
    }

    [Fact]
    public void Class_IsInvalid_WhenNameIsTooLong()
    {
        var model = new Class
        {
            SchoolId = 1,
            Name = "12345678901"
        };

        var results = ValidateModel(model);

        Assert.NotEmpty(results);
    }

    [Fact]
    public void Class_DefaultCollections_AreInitialized()
    {
        var model = new Class();

        Assert.NotNull(model.Students);
        Assert.NotNull(model.Curricula);
        Assert.NotNull(model.Teachers);

        Assert.Empty(model.Students);
        Assert.Empty(model.Curricula);
        Assert.Empty(model.Teachers);
    }

    [Fact]
    public void Class_CanStoreStudents()
    {
        var student = new Student
        {
            Id = 1
        };

        var model = new Class
        {
            SchoolId = 1,
            Name = "8A"
        };

        model.Students.Add(student);

        Assert.Single(model.Students);
        Assert.Equal(1, model.Students.First().Id);
    }

    [Fact]
    public void Class_CanStoreTeachers()
    {
        var teacher = new Teacher
        {
            Id = 1
        };

        var model = new Class
        {
            SchoolId = 1,
            Name = "8A"
        };

        model.Teachers.Add(teacher);

        Assert.Single(model.Teachers);
        Assert.Equal(1, model.Teachers.First().Id);
    }

    [Fact]
    public void Class_CanStoreCurricula()
    {
        var curriculum = new Curriculum
        {
            Id = 1,
            Term = "First Term"
        };

        var model = new Class
        {
            SchoolId = 1,
            Name = "8A"
        };

        model.Curricula.Add(curriculum);

        Assert.Single(model.Curricula);
        Assert.Equal("First Term", model.Curricula.First().Term);
    }
}