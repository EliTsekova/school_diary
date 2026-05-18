namespace school_diary.school_diary_tests.Tests.Controllers;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using school_diary.Controllers;
using school_diary.Models;


public class HomeControllerTests
{
    private static HomeController CreateController(bool isAuthenticated)
    {
        var logger = new Mock<ILogger<HomeController>>();

        var controller = new HomeController(logger.Object);

        var identity = isAuthenticated
            ? new ClaimsIdentity(
                new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, "user-1")
                },
                "TestAuth")
            : new ClaimsIdentity();

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity),
                TraceIdentifier = "trace-test-id"
            }
        };

        return controller;
    }

    [Fact]
    public void Index_RedirectsToRoleRedirect_WhenUserIsAuthenticated()
    {
        var controller = CreateController(true);

        var result = controller.Index();

        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("RoleRedirect", redirect.ControllerName);
    }

    [Fact]
    public void Index_RedirectsToLogin_WhenUserIsNotAuthenticated()
    {
        var controller = CreateController(false);

        var result = controller.Index();

        var redirect = Assert.IsType<RedirectResult>(result);

        Assert.Equal("/Identity/Account/Login", redirect.Url);
    }

    [Fact]
    public void Privacy_ReturnsView()
    {
        var controller = CreateController(false);

        var result = controller.Privacy();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void Error_ReturnsView_WithErrorViewModel()
    {
        var controller = CreateController(false);

        var result = controller.Error();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ErrorViewModel>(viewResult.Model);

        Assert.Equal("trace-test-id", model.RequestId);
    }
}