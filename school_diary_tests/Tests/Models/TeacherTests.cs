using System.ComponentModel.DataAnnotations;
using school_diary.Models;

namespace school_diary.school_diary_tests.Tests.Models;

public class TeacherTests
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
    public void Teacher_IsValid_WhenRequiredFieldsAreSet()
    {
        var model = new Teacher
        {
            UserId = "teacher-user-1",
            SchoolId = 1,
            AssignedClasses = "8A,8B"
        };

        var results = ValidateModel(model);

        Assert.Empty(results);
    }

    [Fact]
    public void Teacher_IsInvalid_WhenUserIdIsMissing()
    {
        var model = new Teacher
        {
            UserId = "",
            SchoolId = 1
        };

        var results = ValidateModel(model);

        Assert.NotEmpty(results);
    }

    [Fact]
    public void Teacher_IsInvalid_WhenAssignedClassesIsTooLong()
    {
        var model = new Teacher
        {
            UserId = "teacher-user-1",
            SchoolId = 1,
            AssignedClasses = new string('A', 201)
        };

        var results = ValidateModel(model);

        Assert.NotEmpty(results);
    }

    [Fact]
    public void Teacher_DefaultCollections_AreInitialized()
    {
        var model = new Teacher();

        Assert.NotNull(model.TeacherSubjects);
        Assert.NotNull(model.Grades);

        Assert.Empty(model.TeacherSubjects);
        Assert.Empty(model.Grades);
    }

    [Fact]
    public void Teacher_DefaultAssignedClasses_IsEmptyString()
    {
        var model = new Teacher();

        Assert.Equal("", model.AssignedClasses);
    }

    [Fact]
    public void Teacher_CanStoreUserNavigationProperty()
    {
        var user = new User
        {
            Id = "teacher-user-1",
            FirstName = "Ivan",
            LastName = "Ivanov",
            Role = Role.Teacher
        };

        var model = new Teacher
        {
            UserId = user.Id,
            SchoolId = 1,
            User = user
        };

        Assert.Equal(user, model.User);
        Assert.Equal("Ivan", model.User!.FirstName);
    }

    [Fact]
    public void Teacher_CanStoreSchoolNavigationProperty()
    {
        var school = new School
        {
            Id = 1,
            Name = "Test School",
            Address = "Sofia Center"
        };

        var model = new Teacher
        {
            UserId = "teacher-user-1",
            SchoolId = school.Id,
            School = school
        };

        Assert.Equal(school, model.School);
        Assert.Equal("Test School", model.School.Name);
    }

    [Fact]
    public void Teacher_CanStoreTeacherSubjects()
    {
        var relation = new TeacherSubject
        {
            Id = 1,
            TeacherId = 1,
            SubjectId = 1
        };

        var model = new Teacher
        {
            UserId = "teacher-user-1",
            SchoolId = 1
        };

        model.TeacherSubjects.Add(relation);

        Assert.Single(model.TeacherSubjects);

        var stored = model.TeacherSubjects.First();

        Assert.Equal(1, stored.TeacherId);
        Assert.Equal(1, stored.SubjectId);
    }

    [Fact]
    public void Teacher_CanStoreGrades()
    {
        var grade = new Grade
        {
            Id = 1,
            Value = 6,
            StudentId = 1,
            SubjectId = 1,
            TeacherId = 1,
            CreatedOn = DateTime.UtcNow
        };

        var model = new Teacher
        {
            UserId = "teacher-user-1",
            SchoolId = 1
        };

        model.Grades.Add(grade);

        Assert.Single(model.Grades);
        Assert.Equal(6, model.Grades.First().Value);
    }
}
