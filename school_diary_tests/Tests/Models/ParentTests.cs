using System.ComponentModel.DataAnnotations;
using school_diary.Models;

namespace school_diary.school_diary_tests.Tests.Models;

public class ParentTests
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
    public void Parent_IsValid_WhenUserIdIsSet()
    {
        var model = new Parent
        {
            UserId = "user-1"
        };

        var results = ValidateModel(model);

        Assert.Empty(results);
    }

    [Fact]
    public void Parent_IsInvalid_WhenUserIdIsMissing()
    {
        var model = new Parent
        {
            UserId = ""
        };

        var results = ValidateModel(model);

        Assert.NotEmpty(results);
    }

    [Fact]
    public void Parent_DefaultValues_AreCorrect()
    {
        var model = new Parent();

        Assert.Equal(0, model.Id);
        Assert.Null(model.UserId);
    }

    [Fact]
    public void Parent_ParentStudentsCollection_IsInitialized()
    {
        var model = new Parent();

        Assert.NotNull(model.ParentStudents);
        Assert.Empty(model.ParentStudents);
    }

    [Fact]
    public void Parent_CanStoreUserNavigationProperty()
    {
        var user = new User
        {
            Id = "user-1",
            FirstName = "Maria",
            LastName = "Ivanova",
            Role = Role.Parent
        };

        var model = new Parent
        {
            UserId = user.Id,
            User = user
        };

        Assert.Equal(user, model.User);
        Assert.Equal("Maria", model.User.FirstName);
    }

    [Fact]
    public void Parent_CanStoreParentStudents()
    {
        var relation = new ParentStudent
        {
            ParentId = 1,
            StudentId = 1
        };

        var model = new Parent
        {
            UserId = "user-1"
        };

        model.ParentStudents.Add(relation);

        Assert.Single(model.ParentStudents);

        var stored = model.ParentStudents.First();

        Assert.Equal(1, stored.ParentId);
        Assert.Equal(1, stored.StudentId);
    }
}