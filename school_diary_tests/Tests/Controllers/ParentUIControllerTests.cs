namespace school_diary.school_diary_tests.Tests.Controllers;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using school_diary.Controllers.Api;
using school_diary.Data;
using school_diary.Models;


public class ParentUIControllerTests
{
    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("ParentUiTests_" + Guid.NewGuid())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static Mock<UserManager<User>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<User>>();

        return new Mock<UserManager<User>>(
            store.Object,
            null!, null!, null!, null!, null!, null!, null!, null!);
    }

    private static ParentUIController CreateController(
        ApplicationDbContext db,
        UserManager<User> userManager,
        string userId = "parent-user-1")
    {
        var controller = new ParentUIController(userManager, db);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }, "TestAuth"));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = user
            }
        };

        return controller;
    }

    [Fact]
    public async Task Index_ReturnsNotFound_WhenParentDoesNotExist()
    {
        var db = CreateDb();

        var userManager = CreateUserManagerMock();

        userManager
            .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
            .Returns("missing-parent-user");

        var controller = CreateController(
            db,
            userManager.Object,
            "missing-parent-user");

        var result = await controller.Index();

        var notFound = Assert.IsType<NotFoundObjectResult>(result);

        Assert.Equal("Parent not found.", notFound.Value);
    }

    [Fact]
    public async Task Index_ReturnsView_WithParentChildrenGradesAndAbsences()
    {
        var db = CreateDb();

        var parentUser = new User
        {
            Id = "parent-user-1",
            FirstName = "Maria",
            LastName = "Ivanova",
            Email = "parent@test.com",
            UserName = "parent@test.com",
            Role = Role.Parent
        };

        var studentUser = new User
        {
            Id = "student-user-1",
            FirstName = "Ivan",
            LastName = "Ivanov",
            Email = "student@test.com",
            UserName = "student@test.com",
            Role = Role.Student
        };

        var school = new School
        {
            Id = 1,
            Name = "Test School",
            Address = "Sofia"
        };

        var classEntity = new Class
        {
            Id = 1,
            Name = "8A",
            SchoolId = school.Id,
            School = school
        };

        var subject = new Subject
        {
            Id = 1,
            Name = "Math"
        };

        var parent = new Parent
        {
            Id = 1,
            UserId = parentUser.Id,
            User = parentUser
        };

        var student = new Student
        {
            Id = 1,
            UserId = studentUser.Id,
            User = studentUser,
            SchoolId = school.Id,
            School = school,
            ClassId = classEntity.Id,
            Class = classEntity
        };

        var parentStudent = new ParentStudent
        {
            ParentId = parent.Id,
            Parent = parent,
            StudentId = student.Id,
            Student = student
        };

        db.Users.AddRange(parentUser, studentUser);
        db.Schools.Add(school);
        db.Classes.Add(classEntity);
        db.Subjects.Add(subject);
        db.Parents.Add(parent);
        db.Students.Add(student);
        db.ParentStudents.Add(parentStudent);

        db.Grades.AddRange(
            new Grade
            {
                Id = 1,
                StudentId = student.Id,
                SubjectId = subject.Id,
                Value = 5,
                CreatedOn = DateTime.UtcNow.AddDays(-1)
            },
            new Grade
            {
                Id = 2,
                StudentId = student.Id,
                SubjectId = subject.Id,
                Value = 6,
                CreatedOn = DateTime.UtcNow
            });

        db.Absences.AddRange(
            new Absence
            {
                Id = 1,
                StudentId = student.Id,
                SubjectId = subject.Id,
                TeacherId = 1,
                Date = DateTime.Today.AddDays(-1),
                IsExcused = false
            },
            new Absence
            {
                Id = 2,
                StudentId = student.Id,
                SubjectId = subject.Id,
                TeacherId = 1,
                Date = DateTime.Today,
                IsExcused = true
            });

        await db.SaveChangesAsync();

        var userManager = CreateUserManagerMock();

        userManager
            .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
            .Returns(parentUser.Id);

        var controller = CreateController(
            db,
            userManager.Object,
            parentUser.Id);

        var result = await controller.Index();

        Assert.IsType<ViewResult>(result);

        Assert.NotNull(controller.ViewBag.Parent);
        Assert.NotNull(controller.ViewBag.Children);
        Assert.NotNull(controller.ViewBag.Grades);
        Assert.NotNull(controller.ViewBag.Absences);

        var children = Assert.IsAssignableFrom<List<Student>>(
            (object)controller.ViewBag.Children);

        var grades = Assert.IsAssignableFrom<List<Grade>>(
            (object)controller.ViewBag.Grades);

        var absences = Assert.IsAssignableFrom<List<Absence>>(
            (object)controller.ViewBag.Absences);

        Assert.Single(children);

        Assert.Equal(2, grades.Count);
        Assert.Equal(6, grades[0].Value);

        Assert.Equal(2, absences.Count);
        Assert.True(absences[0].IsExcused);
    }
}