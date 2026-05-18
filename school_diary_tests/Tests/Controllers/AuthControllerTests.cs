using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using school_diary.Controllers.Api;
using school_diary.Dtos;
using school_diary.Models;
using IdentitySignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace school_diary.school_diary_tests.Tests.Controllers;

public class AuthControllerTests
{
    private static Mock<UserManager<User>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<User>>();

        return new Mock<UserManager<User>>(
            store.Object,
            null!, null!, null!, null!, null!, null!, null!, null!);
    }

    private static Mock<SignInManager<User>> CreateSignInManagerMock(
        UserManager<User> userManager)
    {
        var contextAccessor =
            new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();

        var claimsFactory =
            new Mock<IUserClaimsPrincipalFactory<User>>();

        return new Mock<SignInManager<User>>(
            userManager,
            contextAccessor.Object,
            claimsFactory.Object,
            null!,
            null!,
            null!,
            null!);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenUserNotFound()
    {
        var userManager = CreateUserManagerMock();

        var signInManager =
            CreateSignInManagerMock(userManager.Object);

        userManager
            .Setup(x => x.FindByEmailAsync("missing@test.com"))
            .ReturnsAsync((User?)null);

        var controller = new AuthController(
            signInManager.Object,
            userManager.Object);

        var dto = new LoginDto(
            "missing@test.com",
            "Password123!");

        var result = await controller.Login(dto);

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenPasswordIsInvalid()
    {
        var user = new User
        {
            Id = "user-1",
            Email = "teacher@test.com",
            UserName = "teacher@test.com",
            FirstName = "Ivan",
            LastName = "Ivanov",
            Role = Role.Teacher
        };

        var userManager = CreateUserManagerMock();

        var signInManager =
            CreateSignInManagerMock(userManager.Object);

        userManager
            .Setup(x => x.FindByEmailAsync(user.Email))
            .ReturnsAsync(user);

        signInManager
            .Setup(x => x.PasswordSignInAsync(
                user,
                "WrongPassword",
                false,
                false))
            .ReturnsAsync(IdentitySignInResult.Failed);

        var controller = new AuthController(
            signInManager.Object,
            userManager.Object);

        var dto = new LoginDto(
            user.Email!,
            "WrongPassword");

        var result = await controller.Login(dto);

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Login_ReturnsOk_WhenPasswordIsValidAndIdentityRoleExists()
    {
        var user = new User
        {
            Id = "user-1",
            Email = "admin@test.com",
            UserName = "admin@test.com",
            FirstName = "Admin",
            LastName = "User",
            Role = Role.Teacher
        };

        var userManager = CreateUserManagerMock();

        var signInManager =
            CreateSignInManagerMock(userManager.Object);

        userManager
            .Setup(x => x.FindByEmailAsync(user.Email))
            .ReturnsAsync(user);

        signInManager
            .Setup(x => x.PasswordSignInAsync(
                user,
                "Password123!",
                false,
                false))
            .ReturnsAsync(IdentitySignInResult.Success);

        userManager
            .Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string>
            {
                "Admin"
            });

        var controller = new AuthController(
            signInManager.Object,
            userManager.Object);

        var dto = new LoginDto(
            user.Email!,
            "Password123!");

        var result = await controller.Login(dto);

        var ok = Assert.IsType<OkObjectResult>(result);

        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task Login_ReturnsOk_WithUserRole_WhenIdentityRoleIsMissing()
    {
        var user = new User
        {
            Id = "user-1",
            Email = "teacher@test.com",
            UserName = "teacher@test.com",
            FirstName = "Ivan",
            LastName = "Ivanov",
            Role = Role.Teacher
        };

        var userManager = CreateUserManagerMock();

        var signInManager =
            CreateSignInManagerMock(userManager.Object);

        userManager
            .Setup(x => x.FindByEmailAsync(user.Email))
            .ReturnsAsync(user);

        signInManager
            .Setup(x => x.PasswordSignInAsync(
                user,
                "Password123!",
                false,
                false))
            .ReturnsAsync(IdentitySignInResult.Success);

        userManager
            .Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string>());

        var controller = new AuthController(
            signInManager.Object,
            userManager.Object);

        var dto = new LoginDto(
            user.Email!,
            "Password123!");

        var result = await controller.Login(dto);

        var ok = Assert.IsType<OkObjectResult>(result);

        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task Logout_SignsOutAndReturnsOk()
    {
        var userManager = CreateUserManagerMock();

        var signInManager =
            CreateSignInManagerMock(userManager.Object);

        signInManager
            .Setup(x => x.SignOutAsync())
            .Returns(Task.CompletedTask);

        var controller = new AuthController(
            signInManager.Object,
            userManager.Object);

        var result = await controller.Logout();

        Assert.IsType<OkObjectResult>(result);

        signInManager.Verify(
            x => x.SignOutAsync(),
            Times.Once);
    }
}