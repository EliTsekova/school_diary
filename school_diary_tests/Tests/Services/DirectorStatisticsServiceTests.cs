namespace school_diary.school_diary_tests.Tests;
using Microsoft.EntityFrameworkCore;
using school_diary.Data;
using school_diary.Models;
using school_diary.Services;
using Xunit;


public class DirectorStatisticsServiceTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static DirectorStatisticsService CreateService(ApplicationDbContext ctx)
    {
        return new DirectorStatisticsService(ctx);
    }

    [Fact]
    public async Task GetAverageGradesPerSubjectAsync_ShouldReturnAveragesOnlyForSelectedSchool()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        var math = new Subject
        {
            Id = 1,
            Name = "Math"
        };

        var history = new Subject
        {
            Id = 2,
            Name = "History"
        };

        var student1 = new Student
        {
            Id = 1,
            UserId = "student-1",
            SchoolId = 1
        };

        var student2 = new Student
        {
            Id = 2,
            UserId = "student-2",
            SchoolId = 2
        };

        ctx.Subjects.Add(math);
        ctx.Subjects.Add(history);

        ctx.Students.Add(student1);
        ctx.Students.Add(student2);

        ctx.Grades.Add(new Grade
        {
            Id = 1,
            Value = 6,
            SubjectId = 1,
            Subject = math,
            StudentId = 1,
            Student = student1
        });

        ctx.Grades.Add(new Grade
        {
            Id = 2,
            Value = 4,
            SubjectId = 1,
            Subject = math,
            StudentId = 1,
            Student = student1
        });

        ctx.Grades.Add(new Grade
        {
            Id = 3,
            Value = 2,
            SubjectId = 2,
            Subject = history,
            StudentId = 2,
            Student = student2
        });

        await ctx.SaveChangesAsync();

        var result = await service.GetAverageGradesPerSubjectAsync(1);

        Assert.Single(result);

        var average = result.First();

        Assert.Equal("Math", average.SubjectName);
        Assert.Equal(5, average.AverageGrade);
    }

    [Fact]
    public async Task GetAverageGradesPerTeacherAsync_ShouldReturnTeacherAveragesOnlyForSelectedSchool()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        var user = new User
        {
            Id = "teacher-user-1",
            FirstName = "Ivan",
            LastName = "Petrov",
            Email = "teacher@test.com",
            UserName = "teacher@test.com",
            Role = Role.Teacher
        };

        var teacher = new Teacher
        {
            Id = 1,
            UserId = "teacher-user-1",
            User = user,
            SchoolId = 1
        };

        var student1 = new Student
        {
            Id = 1,
            UserId = "student-1",
            SchoolId = 1
        };

        var student2 = new Student
        {
            Id = 2,
            UserId = "student-2",
            SchoolId = 2
        };

        var subject = new Subject
        {
            Id = 1,
            Name = "Math"
        };

        ctx.Users.Add(user);
        ctx.Teachers.Add(teacher);

        ctx.Students.Add(student1);
        ctx.Students.Add(student2);

        ctx.Subjects.Add(subject);

        ctx.Grades.Add(new Grade
        {
            Id = 1,
            Value = 6,
            StudentId = 1,
            Student = student1,
            SubjectId = 1,
            Subject = subject,
            TeacherId = 1,
            Teacher = teacher
        });

        ctx.Grades.Add(new Grade
        {
            Id = 2,
            Value = 4,
            StudentId = 1,
            Student = student1,
            SubjectId = 1,
            Subject = subject,
            TeacherId = 1,
            Teacher = teacher
        });

        ctx.Grades.Add(new Grade
        {
            Id = 3,
            Value = 2,
            StudentId = 2,
            Student = student2,
            SubjectId = 1,
            Subject = subject,
            TeacherId = 1,
            Teacher = teacher
        });

        await ctx.SaveChangesAsync();

        var result = await service.GetAverageGradesPerTeacherAsync(1);

        Assert.Single(result);

        var average = result.First();

        Assert.Equal("teacher-user-1", average.TeacherId);
        Assert.Equal("Ivan Petrov", average.TeacherName);
        Assert.Equal(5, average.AverageGrade);
    }

    [Fact]
    public async Task GetAbsencesByClassAsync_ShouldReturnAbsenceCountPerClass()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        var classA = new Class
        {
            Id = 1,
            Name = "5A",
            SchoolId = 1
        };

        var student1 = new Student
        {
            Id = 1,
            UserId = "student-1",
            SchoolId = 1,
            ClassId = 1,
            Class = classA
        };

        var student2 = new Student
        {
            Id = 2,
            UserId = "student-2",
            SchoolId = 2
        };

        ctx.Classes.Add(classA);

        ctx.Students.Add(student1);
        ctx.Students.Add(student2);

        ctx.Absences.Add(new Absence
        {
            Id = 1,
            StudentId = 1,
            Student = student1,
            SubjectId = 1,
            TeacherId = 1
        });

        ctx.Absences.Add(new Absence
        {
            Id = 2,
            StudentId = 1,
            Student = student1,
            SubjectId = 1,
            TeacherId = 1
        });

        ctx.Absences.Add(new Absence
        {
            Id = 3,
            StudentId = 2,
            Student = student2,
            SubjectId = 1,
            TeacherId = 1
        });

        await ctx.SaveChangesAsync();

        var result = await service.GetAbsencesByClassAsync(1);

        Assert.Single(result);

        var classAbsence = result.First();

        Assert.Equal("5A", classAbsence.ClassName);
        Assert.Equal(2, classAbsence.AbsenceCount);
    }

    [Fact]
    public async Task GetAverageGradesPerClassAsync_ShouldReturnAverageGradePerClass()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        var classA = new Class
        {
            Id = 1,
            Name = "5A",
            SchoolId = 1
        };

        var student1 = new Student
        {
            Id = 1,
            UserId = "student-1",
            SchoolId = 1,
            ClassId = 1,
            Class = classA
        };

        var student2 = new Student
        {
            Id = 2,
            UserId = "student-2",
            SchoolId = 2
        };

        var subject = new Subject
        {
            Id = 1,
            Name = "Math"
        };

        ctx.Classes.Add(classA);

        ctx.Students.Add(student1);
        ctx.Students.Add(student2);

        ctx.Subjects.Add(subject);

        ctx.Grades.Add(new Grade
        {
            Id = 1,
            Value = 6,
            StudentId = 1,
            Student = student1,
            SubjectId = 1,
            Subject = subject
        });

        ctx.Grades.Add(new Grade
        {
            Id = 2,
            Value = 4,
            StudentId = 1,
            Student = student1,
            SubjectId = 1,
            Subject = subject
        });

        ctx.Grades.Add(new Grade
        {
            Id = 3,
            Value = 2,
            StudentId = 2,
            Student = student2,
            SubjectId = 1,
            Subject = subject
        });

        await ctx.SaveChangesAsync();

        var result = await service.GetAverageGradesPerClassAsync(1);

        Assert.Single(result);

        var classAverage = result.First();

        Assert.Equal("5A", classAverage.ClassName);
        Assert.Equal(5, classAverage.AverageGrade);
    }

    [Fact]
    public async Task StatisticsMethods_ShouldReturnEmptyLists_WhenSchoolHasNoData()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        Assert.Empty(await service.GetAverageGradesPerSubjectAsync(999));
        Assert.Empty(await service.GetAverageGradesPerTeacherAsync(999));
        Assert.Empty(await service.GetAbsencesByClassAsync(999));
        Assert.Empty(await service.GetAverageGradesPerClassAsync(999));
    }
}