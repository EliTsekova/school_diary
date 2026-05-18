using System.ComponentModel.DataAnnotations;
using school_diary.Models;

namespace school_diary.school_diary_tests.Tests.Models;

public class DirectorTests
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
    public void Director_IsValid_WhenRequiredFieldsAreSet()
    {
        var model = new Director
        {
            UserId = "user-1",
            SchoolId = 1
        };

        var results = ValidateModel(model);

        Assert.Empty(results);
    }

    [Fact]
    public void Director_IsInvalid_WhenUserIdIsMissing()
    {
        var model = new Director
        {
            UserId = "",
            SchoolId = 1
        };

        var results = ValidateModel(model);

        Assert.NotEmpty(results);
    }

    [Fact]
    public void Director_DefaultValues_AreCorrect()
    {
        var model = new Director();

        Assert.Equal(0, model.Id);
        Assert.Null(model.UserId);
        Assert.Equal(0, model.SchoolId);
    }

    [Fact]
    public void Director_CanStoreUserNavigationProperty()
    {
        var user = new User
        {
            Id = "user-1",
            FirstName = "Ivan",
            LastName = "Ivanov",
            Role = Role.Director
        };

        var model = new Director
        {
            UserId = user.Id,
            User = user,
            SchoolId = 1
        };

        Assert.Equal(user, model.User);
        Assert.Equal("Ivan", model.User.FirstName);
    }

    [Fact]
    public void Director_CanStoreSchoolNavigationProperty()
    {
        var school = new School
        {
            Id = 1,
            Name = "Test School",
            Address = "Sofia"
        };

        var model = new Director
        {
            UserId = "user-1",
            SchoolId = school.Id,
            School = school
        };

        Assert.Equal(school, model.School);
        Assert.Equal("Test School", model.School.Name);
    }
}