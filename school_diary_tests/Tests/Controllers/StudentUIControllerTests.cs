using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;
using school_diary.Controllers;
using school_diary.Data;
using school_diary.Models;

namespace school_diary.school_diary_tests.Tests.Controllers;

public class StudentUIControllerTests
{
    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("StudentUiTests_" + Guid.NewGuid())
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

    private static StudentUIController CreateController(
        ApplicationDbContext db,
        UserManager<User> userManager,
        string userId = "student-user-1")
    {
        var controller = new StudentUIController(userManager, db);

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }, "TestAuth"));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            }
        };

        controller.TempData = new TempDataDictionary(
            controller.HttpContext,
            Mock.Of<ITempDataProvider>());

        return controller;
    }

    [Fact]
    public async Task Index_ReturnsNotFound_WhenStudentDoesNotExist()
    {
        var db = CreateDb();

        var userManager = CreateUserManagerMock();

        userManager
            .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
            .Returns("missing-user-id");

        var controller = CreateController(
            db,
            userManager.Object,
            "missing-user-id");

        var result = await controller.Index();

        var notFound = Assert.IsType<NotFoundObjectResult>(result);

        Assert.Equal("Student not found.", notFound.Value);
    }

    [Fact]
    public async Task Index_ReturnsView_WithStudentData()
    {
        var db = CreateDb();

        var user = new User
        {
            Id = "student-user-1",
            FirstName = "Ivan",
            LastName = "Ivanov",
            Email = "ivan@test.com",
            UserName = "ivan@test.com",
            Role = Role.Student
        };

        var school = new School
        {
            Id = 100,
            Name = "Test School",
            Address = "Sofia"
        };

        var classEntity = new Class
        {
            Id = 200,
            Name = "8A",
            SchoolId = school.Id,
            School = school
        };

        var subject = new Subject
        {
            Id = 300,
            Name = "Math"
        };

        var student = new Student
        {
            Id = 400,
            UserId = user.Id,
            User = user,
            SchoolId = school.Id,
            School = school,
            ClassId = classEntity.Id,
            Class = classEntity
        };

        var teacherUser = new User
        {
            Id = "teacher-user-1",
            FirstName = "Petar",
            LastName = "Petrov",
            Email = "teacher@test.com",
            UserName = "teacher@test.com",
            Role = Role.Teacher
        };

        var teacher = new Teacher
        {
            Id = 1,
            UserId = teacherUser.Id,
            User = teacherUser,
            SchoolId = school.Id,
            School = school
        };

        db.Users.AddRange(user, teacherUser);
        db.Schools.Add(school);
        db.Classes.Add(classEntity);
        db.Subjects.Add(subject);
        db.Students.Add(student);
        db.Teachers.Add(teacher);

        db.Grades.AddRange(
            new Grade
            {
                Id = 1,
                StudentId = student.Id,
                SubjectId = subject.Id,
                Value = 5,
                CreatedOn = DateTime.UtcNow.AddDays(-2)
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
                TeacherId = teacher.Id,
                Date = DateTime.Today.AddDays(-1),
                IsExcused = false
            },
            new Absence
            {
                Id = 2,
                StudentId = student.Id,
                SubjectId = subject.Id,
                TeacherId = teacher.Id,
                Date = DateTime.Today,
                IsExcused = true
            });

        var curriculum = new Curriculum
        {
            Id = 10,
            ClassId = classEntity.Id,
            Term = "First Term",
            Entries = new List<CurriculumEntry>
            {
                new CurriculumEntry
                {
                    Id = 1,
                    SubjectId = subject.Id,
                    Subject = subject,
                    TeacherId = teacher.Id,
                    Teacher = teacher
                }
            }
        };

        db.Curricula.Add(curriculum);

        await db.SaveChangesAsync();

        var userManager = CreateUserManagerMock();

        userManager
            .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
            .Returns(user.Id);

        var controller = CreateController(
            db,
            userManager.Object,
            user.Id);

        var result = await controller.Index();

        Assert.IsType<ViewResult>(result);

        Assert.NotNull(controller.ViewBag.Student);
        Assert.NotNull(controller.ViewBag.Grades);
        Assert.NotNull(controller.ViewBag.Absences);
        Assert.NotNull(controller.ViewBag.Curriculum);

        var grades = Assert.IsAssignableFrom<List<Grade>>(
            controller.ViewBag.Grades);

        var absences = Assert.IsAssignableFrom<List<Absence>>(
            controller.ViewBag.Absences);

        Assert.Equal(2, grades.Count);
        Assert.Equal(6, grades[0].Value);

        Assert.Equal(2, absences.Count);
        Assert.True(absences[0].IsExcused);
    }

    [Fact]
    public async Task UpdateProfile_ReturnsNotFound_WhenUserDoesNotExist()
    {
        var db = CreateDb();

        var userManager = CreateUserManagerMock();

        userManager
            .Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((User?)null);

        var controller = CreateController(
            db,
            userManager.Object);

        var result = await controller.UpdateProfile(
            "new@test.com",
            "Password123!");

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task UpdateProfile_UpdatesEmail_WhenEmailIsProvided()
    {
        var db = CreateDb();

        var user = new User
        {
            Id = "student-user-1",
            FirstName = "Ivan",
            LastName = "Ivanov",
            Email = "old@test.com",
            UserName = "old@test.com",
            Role = Role.Student
        };

        var userManager = CreateUserManagerMock();

        userManager
            .Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        userManager
            .Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        var controller = CreateController(
            db,
            userManager.Object,
            user.Id);

        var result = await controller.UpdateProfile(
            "new@test.com",
            null);

        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal(
            nameof(StudentUIController.Index),
            redirect.ActionName);

        Assert.Equal("new@test.com", user.Email);
        Assert.Equal("new@test.com", user.UserName);

        Assert.Equal(
            "Profile updated successfully.",
            controller.TempData["Success"]);
    }

    [Fact]
    public async Task UpdateProfile_SetsError_WhenEmailUpdateFails()
    {
        var db = CreateDb();

        var user = new User
        {
            Id = "student-user-1",
            Email = "old@test.com",
            UserName = "old@test.com",
            Role = Role.Student
        };

        var userManager = CreateUserManagerMock();

        userManager
            .Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        userManager
            .Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(
                IdentityResult.Failed(
                    new IdentityError
                    {
                        Description = "Email error"
                    }));

        var controller = CreateController(
            db,
            userManager.Object,
            user.Id);

        var result = await controller.UpdateProfile(
            "bad-email",
            null);

        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal(
            nameof(StudentUIController.Index),
            redirect.ActionName);

        Assert.Equal(
            "Email error",
            controller.TempData["Error"]);
    }

    [Fact]
    public async Task UpdateProfile_UpdatesPassword_WhenPasswordIsProvided()
    {
        var db = CreateDb();

        var user = new User
        {
            Id = "student-user-1",
            Email = "test@test.com",
            UserName = "test@test.com",
            Role = Role.Student
        };

        var userManager = CreateUserManagerMock();

        userManager
            .Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        userManager
            .Setup(x => x.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync("token");

        userManager
            .Setup(x => x.ResetPasswordAsync(
                user,
                "token",
                "NewPass123!"))
            .ReturnsAsync(IdentityResult.Success);

        var controller = CreateController(
            db,
            userManager.Object,
            user.Id);

        var result = await controller.UpdateProfile(
            "",
            "NewPass123!");

        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal(
            nameof(StudentUIController.Index),
            redirect.ActionName);

        Assert.Equal(
            "Profile updated successfully.",
            controller.TempData["Success"]);
    }

    [Fact]
    public async Task UpdateProfile_SetsError_WhenPasswordUpdateFails()
    {
        var db = CreateDb();

        var user = new User
        {
            Id = "student-user-1",
            Email = "test@test.com",
            UserName = "test@test.com",
            Role = Role.Student
        };

        var userManager = CreateUserManagerMock();

        userManager
            .Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        userManager
            .Setup(x => x.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync("token");

        userManager
            .Setup(x => x.ResetPasswordAsync(
                user,
                "token",
                "weak"))
            .ReturnsAsync(
                IdentityResult.Failed(
                    new IdentityError
                    {
                        Description = "Password error"
                    }));

        var controller = CreateController(
            db,
            userManager.Object,
            user.Id);

        var result = await controller.UpdateProfile(
            "",
            "weak");

        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal(
            nameof(StudentUIController.Index),
            redirect.ActionName);

        Assert.Equal(
            "Password error",
            controller.TempData["Error"]);
    }
}