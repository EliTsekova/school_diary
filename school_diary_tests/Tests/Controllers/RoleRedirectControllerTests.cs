namespace school_diary.school_diary_tests.Tests.Controllers;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using school_diary.Controllers;

public class RoleRedirectControllerTests
{
    private static RoleRedirectController CreateController(string role)
    {
        var controller = new RoleRedirectController();

        var user = new ClaimsPrincipal(
            new ClaimsIdentity(
                new[]
                {
                    new Claim(ClaimTypes.Role, role)
                },
                "TestAuth"));

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
    public void Index_RedirectsToAdminUi_WhenUserIsAdmin()
    {
        var controller = CreateController("Admin");

        var result = controller.Index();

        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("AdminUi", redirect.ControllerName);
    }

    [Fact]
    public void Index_RedirectsToDirectorUi_WhenUserIsDirector()
    {
        var controller = CreateController("Director");

        var result = controller.Index();

        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("DirectorUI", redirect.ControllerName);
    }

    [Fact]
    public void Index_RedirectsToTeacherUi_WhenUserIsTeacher()
    {
        var controller = CreateController("Teacher");

        var result = controller.Index();

        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("TeacherUI", redirect.ControllerName);
    }

    [Fact]
    public void Index_RedirectsToParentUi_WhenUserIsParent()
    {
        var controller = CreateController("Parent");

        var result = controller.Index();

        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("ParentUI", redirect.ControllerName);
    }

    [Fact]
    public void Index_RedirectsToStudentUi_WhenUserIsStudent()
    {
        var controller = CreateController("Student");

        var result = controller.Index();

        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("StudentUI", redirect.ControllerName);
    }

    [Fact]
    public void Index_RedirectsToHome_WhenUserHasNoKnownRole()
    {
        var controller = CreateController("Unknown");

        var result = controller.Index();

        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Home", redirect.ControllerName);
    }
}