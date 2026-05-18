using Microsoft.EntityFrameworkCore;
using school_diary.Data;
using school_diary.Models;
using school_diary.Services;
using Xunit;

namespace school_diary.school_diary_tests.Tests;
public class AdminStatisticsServiceTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static AdminStatisticsService CreateService(ApplicationDbContext ctx)
    {
        return new AdminStatisticsService(ctx);
    }

    [Fact]
    public async Task GetGlobalSubjectAveragesAsync_ShouldReturnAverageGradesGroupedBySubject()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        var math = new Subject
        {
            Id = 1,
            Name = "Math"
        };

        var biology = new Subject
        {
            Id = 2,
            Name = "Biology"
        };

        ctx.Subjects.Add(math);
        ctx.Subjects.Add(biology);

        ctx.Grades.Add(new Grade
        {
            Id = 1,
            Value = 6,
            SubjectId = 1,
            Subject = math,
            StudentId = 1
        });

        ctx.Grades.Add(new Grade
        {
            Id = 2,
            Value = 4,
            SubjectId = 1,
            Subject = math,
            StudentId = 2
        });

        ctx.Grades.Add(new Grade
        {
            Id = 3,
            Value = 5,
            SubjectId = 2,
            Subject = biology,
            StudentId = 1
        });

        await ctx.SaveChangesAsync();

        var result = await service.GetGlobalSubjectAveragesAsync();

        Assert.Equal(2, result.Count);

        var mathAverage = result.First(x => x.SubjectName == "Math");
        var biologyAverage = result.First(x => x.SubjectName == "Biology");

        Assert.Equal(5, mathAverage.AverageGrade);
        Assert.Equal(5, biologyAverage.AverageGrade);
    }

    [Fact]
    public async Task GetSubjectAveragesBySchoolAsync_ShouldReturnOnlyAveragesForSelectedSchool()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        var math = new Subject
        {
            Id = 1,
            Name = "Math"
        };

        var studentFromSchoolOne = new Student
        {
            Id = 1,
            UserId = "student-1",
            SchoolId = 1
        };

        var studentFromSchoolTwo = new Student
        {
            Id = 2,
            UserId = "student-2",
            SchoolId = 2
        };

        ctx.Subjects.Add(math);
        ctx.Students.Add(studentFromSchoolOne);
        ctx.Students.Add(studentFromSchoolTwo);

        ctx.Grades.Add(new Grade
        {
            Id = 1,
            Value = 6,
            SubjectId = 1,
            Subject = math,
            StudentId = 1,
            Student = studentFromSchoolOne
        });

        ctx.Grades.Add(new Grade
        {
            Id = 2,
            Value = 4,
            SubjectId = 1,
            Subject = math,
            StudentId = 1,
            Student = studentFromSchoolOne
        });

        ctx.Grades.Add(new Grade
        {
            Id = 3,
            Value = 2,
            SubjectId = 1,
            Subject = math,
            StudentId = 2,
            Student = studentFromSchoolTwo
        });

        await ctx.SaveChangesAsync();

        var result = await service.GetSubjectAveragesBySchoolAsync(1);

        Assert.Single(result);

        var average = result.First();

        Assert.Equal("Math", average.SubjectName);
        Assert.Equal(5, average.AverageGrade);
    }

    [Fact]
    public async Task GetSubjectAveragesBySchoolAsync_ShouldReturnEmptyList_WhenSchoolHasNoGrades()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        var math = new Subject
        {
            Id = 1,
            Name = "Math"
        };

        var student = new Student
        {
            Id = 1,
            UserId = "student-1",
            SchoolId = 1
        };

        ctx.Subjects.Add(math);
        ctx.Students.Add(student);

        ctx.Grades.Add(new Grade
        {
            Id = 1,
            Value = 6,
            SubjectId = 1,
            Subject = math,
            StudentId = 1,
            Student = student
        });

        await ctx.SaveChangesAsync();

        var result = await service.GetSubjectAveragesBySchoolAsync(999);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetGlobalSubjectAveragesAsync_ShouldReturnEmptyList_WhenThereAreNoGrades()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        var result = await service.GetGlobalSubjectAveragesAsync();

        Assert.Empty(result);
    }
}