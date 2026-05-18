namespace school_diary.school_diary_tests.Tests.Controllers;

using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;
using school_diary.Controllers;
using school_diary.Data;
using school_diary.Dtos;
using school_diary.Models;
using school_diary.Services;


public class TeacherUIControllerTests
{
    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("TeacherUiTests_" + Guid.NewGuid())
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

    private static TeacherUIController CreateController(
        ApplicationDbContext db,
        UserManager<User>? userManager = null,
        ITeacherService? teacherService = null,
        IGradeService? gradeService = null,
        IAbsenceService? absenceService = null,
        ISubjectService? subjectService = null,
        string userId = "teacher-user-1")
    {
        var controller = new TeacherUIController(
            userManager ?? CreateUserManagerMock().Object,
            teacherService ?? Mock.Of<ITeacherService>(),
            gradeService ?? Mock.Of<IGradeService>(),
            absenceService ?? Mock.Of<IAbsenceService>(),
            subjectService ?? Mock.Of<ISubjectService>(),
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
    public async Task Index_ReturnsNotFound_WhenTeacherDoesNotExist()
    {
        var db = CreateDb();

        var userManager = CreateUserManagerMock();

        userManager
            .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
            .Returns("teacher-user-1");

        var teacherService = new Mock<ITeacherService>();

        teacherService
            .Setup(x => x.GetByUserIdAsync("teacher-user-1"))
            .ReturnsAsync((TeacherDto?)null);

        var controller = CreateController(
            db,
            userManager: userManager.Object,
            teacherService: teacherService.Object);

        var result = await controller.Index();

        var notFound = Assert.IsType<NotFoundObjectResult>(result);

        Assert.Equal("Teacher not found.", notFound.Value);
    }

    [Fact]
    public async Task AddGrade_ReturnsRedirectAndError_WhenModelStateIsInvalid()
    {
        var db = CreateDb();

        var controller = CreateController(db);

        controller.ModelState.AddModelError("Value", "Invalid");

        var dto = new CreateGradeDto(
            1,
            1,
            1,
            6);

        var result = await controller.AddGrade(dto);

        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal(nameof(TeacherUIController.Index), redirect.ActionName);
        Assert.Equal("Invalid grade data.", controller.TempData["Error"]);
    }

    [Fact]
    public async Task EditGrade_ReturnsRedirectAndMessage_WhenValueIsInvalid()
    {
        var db = CreateDb();

        var controller = CreateController(db);

        var result = await controller.EditGrade(1, 7);

        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal(nameof(TeacherUIController.Index), redirect.ActionName);
        Assert.Equal("The grade must be between 2 and 6.", controller.TempData["Message"]);
    }

    [Fact]
    public async Task AddAbsence_ReturnsRedirectAndError_WhenModelStateIsInvalid()
    {
        var db = CreateDb();

        var controller = CreateController(db);

        controller.ModelState.AddModelError("StudentId", "Invalid");

        var dto = new CreateAbsenceDto(
            1,
            1,
            DateTime.Today,
            false);

        var result = await controller.AddAbsence(dto);

        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal(nameof(TeacherUIController.Index), redirect.ActionName);
        Assert.Equal("Invalid absence data.", controller.TempData["Error"]);
    }

    [Fact]
    public async Task ChangePassword_ReturnsError_WhenPasswordIsEmpty()
    {
        var db = CreateDb();

        var controller = CreateController(db);

        var result = await controller.ChangePassword("", "");

        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal(nameof(TeacherUIController.Index), redirect.ActionName);
        Assert.Equal("Enter a new password.", controller.TempData["Error"]);
    }

    [Fact]
    public async Task ChangePassword_ReturnsError_WhenPasswordsDoNotMatch()
    {
        var db = CreateDb();

        var controller = CreateController(db);

        var result = await controller.ChangePassword("Password123!", "Different123!");

        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal(nameof(TeacherUIController.Index), redirect.ActionName);
        Assert.Equal("The passwords do not match.", controller.TempData["Error"]);
    }

    [Fact]
    public async Task EditProfile_ReturnsError_WhenInputIsInvalid()
    {
        var db = CreateDb();

        var controller = CreateController(db);

        var input = new TeacherUIController.EditProfileInputModel
        {
            FullName = "",
            Email = ""
        };

        var result = await controller.EditProfile(input);

        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal(nameof(TeacherUIController.Index), redirect.ActionName);
        Assert.Equal("Name and email are required.", controller.TempData["Error"]);
    }
}