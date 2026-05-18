namespace school_diary.school_diary_tests.Tests.Models;
using System.ComponentModel.DataAnnotations;
using school_diary.Models;


public class CurriculumModelTests
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
    public void Curriculum_IsValid_WhenRequiredFieldsAreSet()
    {
        var model = new Curriculum
        {
            Term = "First Term",
            ClassId = 1
        };

        var results = ValidateModel(model);

        Assert.Empty(results);
    }

    [Fact]
    public void Curriculum_IsInvalid_WhenTermIsMissing()
    {
        var model = new Curriculum
        {
            Term = "",
            ClassId = 1
        };

        var results = ValidateModel(model);

        Assert.NotEmpty(results);
    }

    [Fact]
    public void Curriculum_IsInvalid_WhenTermIsTooShort()
    {
        var model = new Curriculum
        {
            Term = "A",
            ClassId = 1
        };

        var results = ValidateModel(model);

        Assert.NotEmpty(results);
    }

    [Fact]
    public void Curriculum_IsInvalid_WhenTermIsTooLong()
    {
        var model = new Curriculum
        {
            Term = new string('A', 101),
            ClassId = 1
        };

        var results = ValidateModel(model);

        Assert.NotEmpty(results);
    }

    [Fact]
    public void Curriculum_DefaultEntriesCollection_IsInitialized()
    {
        var model = new Curriculum();

        Assert.NotNull(model.Entries);
        Assert.Empty(model.Entries);
    }

    [Fact]
    public void Curriculum_CanStoreEntries()
    {
        var entry = new CurriculumEntry
        {
            Id = 1
        };

        var model = new Curriculum
        {
            Term = "First Term",
            ClassId = 1
        };

        model.Entries.Add(entry);

        Assert.Single(model.Entries);
        Assert.Equal(1, model.Entries.First().Id);
    }

    [Fact]
    public void Curriculum_CanStoreClassNavigationProperty()
    {
        var classEntity = new Class
        {
            Id = 1,
            SchoolId = 1,
            Name = "8A"
        };

        var model = new Curriculum
        {
            Term = "First Term",
            ClassId = classEntity.Id,
            Class = classEntity
        };

        Assert.Equal(classEntity, model.Class);
        Assert.Equal("8A", model.Class.Name);
    }
}