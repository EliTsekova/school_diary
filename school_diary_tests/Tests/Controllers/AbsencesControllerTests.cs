using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using school_diary.Controllers.Api;
using school_diary.Dtos;
using school_diary.Services;

namespace school_diary.school_diary_tests.Tests.Controllers;

public class AbsencesControllerTests
{
    private static AbsencesController CreateController(
        IAbsenceService service,
        string userId = "teacher-user-1",
        string role = "Teacher")
    {
        var controller = new AbsencesController(service);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Role, role)
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
    public async Task Get_ReturnsOk_WhenAbsenceExists()
    {
        var service = new Mock<IAbsenceService>();

        var dto = new AbsenceDto(
            1,
            1,
            1,
            "teacher-user-1",
            DateTime.Today,
            false,
            null);

        service
            .Setup(x => x.GetForTeacherAsync(1, "teacher-user-1"))
            .ReturnsAsync(dto);

        var controller = CreateController(service.Object);

        var result = await controller.Get(1);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var model = Assert.IsType<AbsenceDto>(ok.Value);

        Assert.Equal(1, model.Id);
    }

    [Fact]
    public async Task Get_ReturnsNotFound_WhenAbsenceDoesNotExist()
    {
        var service = new Mock<IAbsenceService>();

        service
            .Setup(x => x.GetForTeacherAsync(99, "teacher-user-1"))
            .ReturnsAsync((AbsenceDto?)null);

        var controller = CreateController(service.Object);

        var result = await controller.Get(99);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WithAbsences()
    {
        var service = new Mock<IAbsenceService>();

        IReadOnlyList<AbsenceDto> absences = new List<AbsenceDto>
        {
            new AbsenceDto(
                1,
                1,
                1,
                "teacher-user-1",
                DateTime.Today,
                false,
                null)
        };

        service
            .Setup(x => x.GetAllForTeacherAsync("teacher-user-1"))
            .ReturnsAsync(absences);

        var controller = CreateController(service.Object);

        var result = await controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var model = Assert.IsAssignableFrom<IReadOnlyList<AbsenceDto>>(ok.Value);

        Assert.Single(model);
    }

    [Fact]
    public async Task Post_ReturnsCreatedAtAction()
    {
        var service = new Mock<IAbsenceService>();

        var input = new CreateAbsenceDto(
            1,
            1,
            DateTime.Today,
            false);

        var created = new AbsenceDto(
            10,
            1,
            1,
            "teacher-user-1",
            DateTime.Today,
            false,
            null);

        service
            .Setup(x => x.CreateAsync(input, "teacher-user-1"))
            .ReturnsAsync(created);

        var controller = CreateController(service.Object);

        var result = await controller.Post(input);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);

        Assert.Equal(nameof(AbsencesController.Get), createdResult.ActionName);
        Assert.Equal(10, createdResult.RouteValues!["id"]);

        var model = Assert.IsType<AbsenceDto>(createdResult.Value);

        Assert.Equal(10, model.Id);
    }

    [Fact]
    public async Task Put_ReturnsNoContent()
    {
        var service = new Mock<IAbsenceService>();

        var dto = new UpdateAbsenceDto(
            1,
            DateTime.Today,
            true);

        var updated = new AbsenceDto(
            1,
            1,
            1,
            "teacher-user-1",
            DateTime.Today,
            true,
            DateTime.Today);

        service
            .Setup(x => x.UpdateAsync(1, dto, "teacher-user-1"))
            .ReturnsAsync(updated);

        var controller = CreateController(service.Object);

        var result = await controller.Put(1, dto);

        Assert.IsType<NoContentResult>(result);

        service.Verify(
            x => x.UpdateAsync(1, dto, "teacher-user-1"),
            Times.Once);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent()
    {
        var service = new Mock<IAbsenceService>();

        service
            .Setup(x => x.DeleteAsync(1, "teacher-user-1"))
            .Returns(Task.CompletedTask);

        var controller = CreateController(service.Object);

        var result = await controller.Delete(1);

        Assert.IsType<NoContentResult>(result);

        service.Verify(
            x => x.DeleteAsync(1, "teacher-user-1"),
            Times.Once);
    }

    [Fact]
    public async Task GetAbsencesForMyChildren_ReturnsOk()
    {
        var service = new Mock<IAbsenceService>();

        IReadOnlyList<AbsenceDto> absences = new List<AbsenceDto>
        {
            new AbsenceDto(
                1,
                1,
                1,
                "teacher-user-1",
                DateTime.Today,
                true,
                DateTime.Today)
        };

        service
            .Setup(x => x.GetAbsencesForParentAsync("parent-user-1"))
            .ReturnsAsync(absences);

        var controller = CreateController(
            service.Object,
            userId: "parent-user-1",
            role: "Parent");

        var result = await controller.GetAbsencesForMyChildren();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var model = Assert.IsAssignableFrom<IReadOnlyList<AbsenceDto>>(ok.Value);

        Assert.Single(model);
    }
}