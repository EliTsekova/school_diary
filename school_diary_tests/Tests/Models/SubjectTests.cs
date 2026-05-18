using System.ComponentModel.DataAnnotations;
using school_diary.Models;

namespace school_diary.school_diary_tests.Tests.Models;

public class SubjectTests
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
    public void Subject_IsValid_WhenNameIsCorrect()
    {
        var model = new Subject
        {
            Name = "Mathematics"
        };

        var results = ValidateModel(model);

        Assert.Empty(results);
    }

    [Fact]
    public void Subject_IsInvalid_WhenNameIsMissing()
    {
        var model = new Subject
        {
            Name = ""
        };

        var results = ValidateModel(model);

        Assert.NotEmpty(results);
    }

    [Fact]
    public void Subject_IsInvalid_WhenNameIsTooShort()
    {
        var model = new Subject
        {
            Name = "A"
        };

        var results = ValidateModel(model);

        Assert.NotEmpty(results);
    }

    [Fact]
    public void Subject_IsInvalid_WhenNameIsTooLong()
    {
        var model = new Subject
        {
            Name = new string('A', 101)
        };

        var results = ValidateModel(model);

        Assert.NotEmpty(results);
    }

    [Fact]
    public void Subject_DefaultValues_AreCorrect()
    {
        var model = new Subject();

        Assert.Equal(0, model.Id);
        Assert.Null(model.Name);
        Assert.Null(model.TeacherSubjects);
    }

    [Fact]
    public void Subject_CanStoreTeacherSubjects()
    {
        var relation = new TeacherSubject
        {
            Id = 1,
            TeacherId = 1,
            SubjectId = 1
        };

        var model = new Subject
        {
            Name = "Math",
            TeacherSubjects = new List<TeacherSubject>()
        };

        model.TeacherSubjects.Add(relation);

        Assert.Single(model.TeacherSubjects);

        var stored = model.TeacherSubjects.First();

        Assert.Equal(1, stored.TeacherId);
        Assert.Equal(1, stored.SubjectId);
    }
}