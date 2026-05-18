using System.ComponentModel.DataAnnotations;
using school_diary.Models;

namespace school_diary.school_diary_tests.Tests.Models;

public class SchoolTests
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
    public void School_IsValid_WhenRequiredFieldsAreSet()
    {
        var model = new School
        {
            Name = "Test School",
            Address = "Sofia Center"
        };

        var results = ValidateModel(model);

        Assert.Empty(results);
    }

    [Fact]
    public void School_IsInvalid_WhenNameIsMissing()
    {
        var model = new School
        {
            Name = "",
            Address = "Sofia Center"
        };

        var results = ValidateModel(model);

        Assert.NotEmpty(results);
    }

    [Fact]
    public void School_IsInvalid_WhenNameIsTooShort()
    {
        var model = new School
        {
            Name = "A",
            Address = "Sofia Center"
        };

        var results = ValidateModel(model);

        Assert.NotEmpty(results);
    }

    [Fact]
    public void School_IsInvalid_WhenAddressIsTooShort()
    {
        var model = new School
        {
            Name = "Test School",
            Address = "1234"
        };

        var results = ValidateModel(model);

        Assert.NotEmpty(results);
    }

    [Fact]
    public void School_DefaultCollections_AreInitialized()
    {
        var model = new School();

        Assert.NotNull(model.Teachers);
        Assert.NotNull(model.Students);
        Assert.NotNull(model.Classes);

        Assert.Empty(model.Teachers);
        Assert.Empty(model.Students);
        Assert.Empty(model.Classes);
    }

    [Fact]
    public void School_CanStoreTeachers()
    {
        var teacher = new Teacher
        {
            Id = 1
        };

        var model = new School
        {
            Name = "Test School",
            Address = "Sofia Center"
        };

        model.Teachers.Add(teacher);

        Assert.Single(model.Teachers);
        Assert.Equal(1, model.Teachers.First().Id);
    }

    [Fact]
    public void School_CanStoreStudents()
    {
        var student = new Student
        {
            Id = 1
        };

        var model = new School
        {
            Name = "Test School",
            Address = "Sofia Center"
        };

        model.Students.Add(student);

        Assert.Single(model.Students);
        Assert.Equal(1, model.Students.First().Id);
    }

    [Fact]
    public void School_CanStoreClasses()
    {
        var classEntity = new Class
        {
            Id = 1,
            SchoolId = 1,
            Name = "8A"
        };

        var model = new School
        {
            Name = "Test School",
            Address = "Sofia Center"
        };

        model.Classes.Add(classEntity);

        Assert.Single(model.Classes);
        Assert.Equal("8A", model.Classes.First().Name);
    }

    [Fact]
    public void School_CanStoreDirector()
    {
        var director = new Director
        {
            Id = 1,
            UserId = "director-user-1",
            SchoolId = 1
        };

        var model = new School
        {
            Name = "Test School",
            Address = "Sofia Center",
            Director = director
        };

        Assert.Equal(director, model.Director);
        Assert.Equal(1, model.Director!.Id);
    }
}