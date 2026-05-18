using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;
using school_diary.Controllers.Api;
using school_diary.Data;
using school_diary.Dtos;
using school_diary.Models;
using school_diary.Services;

namespace school_diary.school_diary_tests.Tests.Controllers;

public class DirectorUIControllerTests
{
    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("DirectorUiTests_" + Guid.NewGuid())
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

    private static DirectorUIController CreateController(
        ApplicationDbContext db,
        UserManager<User>? userManager = null,
        IDirectorService? directorService = null,
        IDirectorStatisticsService? statisticsService = null,
        ISchoolService? schoolService = null,
        string userId = "director-user-1")
    {
        var controller = new DirectorUIController(
            userManager ?? CreateUserManagerMock().Object,
            directorService ?? Mock.Of<IDirectorService>(),
            statisticsService ?? Mock.Of<IDirectorStatisticsService>(),
            schoolService ?? Mock.Of<ISchoolService>(),
            db);

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

        controller.TempData = new TempDataDictionary(
            controller.HttpContext,
            Mock.Of<ITempDataProvider>());

        return controller;
    }

    [Fact]
    public async Task Index_ReturnsView_WithSchoolDataAndStatistics()
    {
        var db = CreateDb();

        var directorUser = new User
        {
            Id = "director-user-1",
            FirstName = "Ivan",
            LastName = "Director",
            Email = "director@test.com",
            UserName = "director@test.com",
            Role = Role.Director
        };

        var studentUser = new User
        {
            Id = "student-user-1",
            FirstName = "Anna",
            LastName = "Student",
            Email = "student@test.com",
            UserName = "student@test.com",
            Role = Role.Student
        };

        var teacherUser = new User
        {
            Id = "teacher-user-1",
            FirstName = "Petar",
            LastName = "Teacher",
            Email = "teacher@test.com",
            UserName = "teacher@test.com",
            Role = Role.Teacher
        };

        var parentUser = new User
        {
            Id = "parent-user-1",
            FirstName = "Maria",
            LastName = "Parent",
            Email = "parent@test.com",
            UserName = "parent@test.com",
            Role = Role.Parent
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

        var director = new Director
        {
            Id = 1,
            UserId = directorUser.Id,
            User = directorUser,
            SchoolId = school.Id,
            School = school
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

        var teacher = new Teacher
        {
            Id = 1,
            UserId = teacherUser.Id,
            User = teacherUser,
            SchoolId = school.Id,
            School = school
        };

        var parent = new Parent
        {
            Id = 1,
            UserId = parentUser.Id,
            User = parentUser
        };

        var parentStudent = new ParentStudent
        {
            ParentId = parent.Id,
            Parent = parent,
            StudentId = student.Id,
            Student = student
        };

        db.Users.AddRange(directorUser, studentUser, teacherUser, parentUser);
        db.Schools.Add(school);
        db.Classes.Add(classEntity);
        db.Directors.Add(director);
        db.Students.Add(student);
        db.Teachers.Add(teacher);
        db.Parents.Add(parent);
        db.ParentStudents.Add(parentStudent);

        await db.SaveChangesAsync();

        var userManager = CreateUserManagerMock();

        userManager
            .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
            .Returns(directorUser.Id);

        var directorService = new Mock<IDirectorService>();

        directorService
            .Setup(x => x.GetSchoolIdByUserId(directorUser.Id))
            .ReturnsAsync(school.Id);

        var schoolService = new Mock<ISchoolService>();

        schoolService
            .Setup(x => x.GetAsync(school.Id))
            .ReturnsAsync(new SchoolDto(
                school.Id,
                school.Name,
                school.Address));

        var statisticsService = new Mock<IDirectorStatisticsService>();

        statisticsService
            .Setup(x => x.GetAverageGradesPerSubjectAsync(school.Id))
            .ReturnsAsync(new List<SubjectAverageDto>());

        statisticsService
            .Setup(x => x.GetAverageGradesPerTeacherAsync(school.Id))
            .ReturnsAsync(new List<TeacherAverageDto>());

        statisticsService
            .Setup(x => x.GetAverageGradesPerClassAsync(school.Id))
            .ReturnsAsync(new List<ClassAverageDto>());

        statisticsService
            .Setup(x => x.GetAbsencesByClassAsync(school.Id))
            .ReturnsAsync(new List<ClassAbsenceDto>());

        var controller = CreateController(
            db,
            userManager: userManager.Object,
            directorService: directorService.Object,
            statisticsService: statisticsService.Object,
            schoolService: schoolService.Object,
            userId: directorUser.Id);

        var result = await controller.Index();

        Assert.IsType<ViewResult>(result);

        Assert.NotNull(controller.ViewBag.Director);
        Assert.NotNull(controller.ViewBag.School);
        Assert.NotNull(controller.ViewBag.Students);
        Assert.NotNull(controller.ViewBag.Teachers);
        Assert.NotNull(controller.ViewBag.Parents);
        Assert.NotNull(controller.ViewBag.SubjectAverages);
        Assert.NotNull(controller.ViewBag.TeacherAverages);
        Assert.NotNull(controller.ViewBag.ClassAverages);
        Assert.NotNull(controller.ViewBag.ClassAbsences);
    }

    [Fact]
    public async Task EditProfile_ReturnsError_WhenEmailIsEmpty()
    {
        var db = CreateDb();

        var controller = CreateController(db);

        var result = await controller.EditProfile("");

        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal(nameof(DirectorUIController.Index), redirect.ActionName);
        Assert.Equal("Email is required.", controller.TempData["Error"]);
    }

    [Fact]
    public async Task EditProfile_ReturnsNotFound_WhenUserDoesNotExist()
    {
        var db = CreateDb();

        var userManager = CreateUserManagerMock();

        userManager
            .Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((User?)null);

        var controller = CreateController(
            db,
            userManager: userManager.Object);

        var result = await controller.EditProfile("director@test.com");

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task EditProfile_UpdatesEmail_WhenValid()
    {
        var db = CreateDb();

        var user = new User
        {
            Id = "director-user-1",
            FirstName = "Ivan",
            LastName = "Director",
            Email = "old@test.com",
            UserName = "old@test.com",
            Role = Role.Director
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
            userManager: userManager.Object,
            userId: user.Id);

        var result = await controller.EditProfile("new@test.com");

        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal(nameof(DirectorUIController.Index), redirect.ActionName);
        Assert.Equal("new@test.com", user.Email);
        Assert.Equal("new@test.com", user.UserName);
        Assert.Equal("Profile updated successfully.", controller.TempData["Success"]);
    }

    [Fact]
    public async Task EditProfile_SetsError_WhenUpdateFails()
    {
        var db = CreateDb();

        var user = new User
        {
            Id = "director-user-1",
            Email = "old@test.com",
            UserName = "old@test.com",
            Role = Role.Director
        };

        var userManager = CreateUserManagerMock();

        userManager
            .Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        userManager
            .Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Failed(
                new IdentityError
                {
                    Description = "Email update failed"
                }));

        var controller = CreateController(
            db,
            userManager: userManager.Object,
            userId: user.Id);

        var result = await controller.EditProfile("new@test.com");

        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal(nameof(DirectorUIController.Index), redirect.ActionName);
        Assert.Equal("Email update failed", controller.TempData["Error"]);
    }

    [Fact]
    public async Task ChangePassword_ReturnsError_WhenPasswordIsEmpty()
    {
        var db = CreateDb();

        var controller = CreateController(db);

        var result = await controller.ChangePassword("", "");

        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal(nameof(DirectorUIController.Index), redirect.ActionName);
        Assert.Equal("Enter a new password.", controller.TempData["Error"]);
    }

    [Fact]
    public async Task ChangePassword_ReturnsError_WhenPasswordsDoNotMatch()
    {
        var db = CreateDb();

        var controller = CreateController(db);

        var result = await controller.ChangePassword("Password123!", "Different123!");

        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal(nameof(DirectorUIController.Index), redirect.ActionName);
        Assert.Equal("The passwords do not match.", controller.TempData["Error"]);
    }

    [Fact]
    public async Task ChangePassword_ReturnsNotFound_WhenUserDoesNotExist()
    {
        var db = CreateDb();

        var userManager = CreateUserManagerMock();

        userManager
            .Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((User?)null);

        var controller = CreateController(
            db,
            userManager: userManager.Object);

        var result = await controller.ChangePassword("Password123!", "Password123!");

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task ChangePassword_ChangesPassword_WhenValid()
    {
        var db = CreateDb();

        var user = new User
        {
            Id = "director-user-1",
            Email = "director@test.com",
            UserName = "director@test.com",
            Role = Role.Director
        };

        var userManager = CreateUserManagerMock();

        userManager
            .Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        userManager
            .Setup(x => x.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync("token");

        userManager
            .Setup(x => x.ResetPasswordAsync(user, "token", "Password123!"))
            .ReturnsAsync(IdentityResult.Success);

        var controller = CreateController(
            db,
            userManager: userManager.Object,
            userId: user.Id);

        var result = await controller.ChangePassword("Password123!", "Password123!");

        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal(nameof(DirectorUIController.Index), redirect.ActionName);
        Assert.Equal("Password changed successfully.", controller.TempData["Success"]);
    }

    [Fact]
    public async Task ChangePassword_SetsError_WhenPasswordChangeFails()
    {
        var db = CreateDb();

        var user = new User
        {
            Id = "director-user-1",
            Email = "director@test.com",
            UserName = "director@test.com",
            Role = Role.Director
        };

        var userManager = CreateUserManagerMock();

        userManager
            .Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        userManager
            .Setup(x => x.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync("token");

        userManager
            .Setup(x => x.ResetPasswordAsync(user, "token", "bad"))
            .ReturnsAsync(IdentityResult.Failed(
                new IdentityError
                {
                    Description = "Password error"
                }));

        var controller = CreateController(
            db,
            userManager: userManager.Object,
            userId: user.Id);

        var result = await controller.ChangePassword("bad", "bad");

        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal(nameof(DirectorUIController.Index), redirect.ActionName);
        Assert.Equal("Password error", controller.TempData["Error"]);
    }
}