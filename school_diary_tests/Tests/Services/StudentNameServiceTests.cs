namespace school_diary.school_diary_tests.Tests;

using Microsoft.EntityFrameworkCore;
using school_diary.Data;
using school_diary.Models;
using school_diary.Services;
using Xunit;


public class StudentNameServiceTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static StudentNameService CreateService(ApplicationDbContext ctx)
    {
        return new StudentNameService(ctx);
    }

    [Fact]
    public async Task GetStudentNamesByParentIdAsync_ShouldReturnStudentFullNames()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        var user1 = new User
        {
            Id = "student-user-1",
            FirstName = "Ivan",
            LastName = "Petrov",
            Email = "ivan@test.com",
            UserName = "ivan@test.com",
            Role = Role.Student
        };

        var user2 = new User
        {
            Id = "student-user-2",
            FirstName = "Maria",
            LastName = "Ivanova",
            Email = "maria@test.com",
            UserName = "maria@test.com",
            Role = Role.Student
        };

        var student1 = new Student
        {
            Id = 1,
            UserId = "student-user-1",
            User = user1,
            SchoolId = 1
        };

        var student2 = new Student
        {
            Id = 2,
            UserId = "student-user-2",
            User = user2,
            SchoolId = 1
        };

        ctx.Users.Add(user1);
        ctx.Users.Add(user2);

        ctx.Students.Add(student1);
        ctx.Students.Add(student2);

        ctx.ParentStudents.Add(new ParentStudent
        {
            ParentId = 1,
            StudentId = 1,
            Student = student1
        });

        ctx.ParentStudents.Add(new ParentStudent
        {
            ParentId = 1,
            StudentId = 2,
            Student = student2
        });

        await ctx.SaveChangesAsync();

        var result = await service.GetStudentNamesByParentIdAsync(1);

        Assert.Equal(2, result.Count);
        Assert.Contains("Ivan Petrov", result);
        Assert.Contains("Maria Ivanova", result);
    }

    [Fact]
    public async Task GetStudentNamesByParentIdAsync_ShouldReturnEmptyList_WhenParentHasNoStudents()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        var result = await service.GetStudentNamesByParentIdAsync(999);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetStudentNamesByParentIdAsync_ShouldReturnOnlyStudentsForSpecifiedParent()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        var user1 = new User
        {
            Id = "student-user-1",
            FirstName = "Ivan",
            LastName = "Petrov",
            Email = "ivan@test.com",
            UserName = "ivan@test.com",
            Role = Role.Student
        };

        var user2 = new User
        {
            Id = "student-user-2",
            FirstName = "Maria",
            LastName = "Ivanova",
            Email = "maria@test.com",
            UserName = "maria@test.com",
            Role = Role.Student
        };

        var student1 = new Student
        {
            Id = 1,
            UserId = "student-user-1",
            User = user1,
            SchoolId = 1
        };

        var student2 = new Student
        {
            Id = 2,
            UserId = "student-user-2",
            User = user2,
            SchoolId = 1
        };

        ctx.Users.Add(user1);
        ctx.Users.Add(user2);

        ctx.Students.Add(student1);
        ctx.Students.Add(student2);

        ctx.ParentStudents.Add(new ParentStudent
        {
            ParentId = 1,
            StudentId = 1,
            Student = student1
        });

        ctx.ParentStudents.Add(new ParentStudent
        {
            ParentId = 2,
            StudentId = 2,
            Student = student2
        });

        await ctx.SaveChangesAsync();

        var result = await service.GetStudentNamesByParentIdAsync(1);

        Assert.Single(result);
        Assert.Equal("Ivan Petrov", result.First());
    }
}