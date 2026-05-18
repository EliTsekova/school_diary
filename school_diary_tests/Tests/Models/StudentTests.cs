using System.ComponentModel.DataAnnotations;
using school_diary.Models;

namespace school_diary.school_diary_tests.Tests.Models;

public class StudentTests
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
    public void Student_IsValid_WhenUserIdIsSet()
    {
        var model = new Student
        {
            UserId = "student-user-1"
        };

        var results = ValidateModel(model);

        Assert.Empty(results);
    }

    [Fact]
    public void Student_IsInvalid_WhenUserIdIsMissing()
    {
        var model = new Student
        {
            UserId = ""
        };

        var results = ValidateModel(model);

        Assert.NotEmpty(results);
    }

    [Fact]
    public void Student_DefaultCollections_AreInitialized()
    {
        var model = new Student();

        Assert.NotNull(model.ParentStudents);
        Assert.NotNull(model.Grades);
        Assert.NotNull(model.Absences);

        Assert.Empty(model.ParentStudents);
        Assert.Empty(model.Grades);
        Assert.Empty(model.Absences);
    }

    [Fact]
    public void Student_SchoolId_And_ClassId_CanBeNull()
    {
        var model = new Student
        {
            UserId = "student-user-1",
            SchoolId = null,
            ClassId = null
        };

        var results = ValidateModel(model);

        Assert.Empty(results);
    }

    [Fact]
    public void Student_CanStoreUserNavigationProperty()
    {
        var user = new User
        {
            Id = "student-user-1",
            FirstName = "Ivan",
            LastName = "Ivanov",
            Role = Role.Student
        };

        var model = new Student
        {
            UserId = user.Id,
            User = user
        };

        Assert.Equal(user, model.User);
        Assert.Equal("Ivan", model.User.FirstName);
    }

    [Fact]
    public void Student_CanStoreSchoolNavigationProperty()
    {
        var school = new School
        {
            Id = 1,
            Name = "Test School",
            Address = "Sofia Center"
        };

        var model = new Student
        {
            UserId = "student-user-1",
            SchoolId = school.Id,
            School = school
        };

        Assert.Equal(school, model.School);
        Assert.Equal("Test School", model.School!.Name);
    }

    [Fact]
    public void Student_CanStoreClassNavigationProperty()
    {
        var classEntity = new Class
        {
            Id = 1,
            SchoolId = 1,
            Name = "8A"
        };

        var model = new Student
        {
            UserId = "student-user-1",
            ClassId = classEntity.Id,
            Class = classEntity
        };

        Assert.Equal(classEntity, model.Class);
        Assert.Equal("8A", model.Class!.Name);
    }

    [Fact]
    public void Student_CanStoreGrades()
    {
        var grade = new Grade
        {
            Id = 1,
            Value = 6,
            StudentId = 1,
            SubjectId = 1,
            CreatedOn = DateTime.UtcNow
        };

        var model = new Student
        {
            UserId = "student-user-1"
        };

        model.Grades.Add(grade);

        Assert.Single(model.Grades);
        Assert.Equal(6, model.Grades.First().Value);
    }

    [Fact]
    public void Student_CanStoreAbsences()
    {
        var absence = new Absence
        {
            Id = 1,
            StudentId = 1,
            TeacherId = 1,
            SubjectId = 1,
            Date = DateTime.Today,
            IsExcused = false
        };

        var model = new Student
        {
            UserId = "student-user-1"
        };

        model.Absences.Add(absence);

        Assert.Single(model.Absences);
        Assert.False(model.Absences.First().IsExcused);
    }

    [Fact]
    public void Student_CanStoreParentStudents()
    {
        var relation = new ParentStudent
        {
            Id = 1,
            ParentId = 1,
            StudentId = 1
        };

        var model = new Student
        {
            UserId = "student-user-1"
        };

        model.ParentStudents.Add(relation);

        Assert.Single(model.ParentStudents);
        Assert.Equal(1, model.ParentStudents.First().ParentId);
    }
}