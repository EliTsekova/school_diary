using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using school_diary.Controllers.Api;
using school_diary.Dtos;
using school_diary.Services;

namespace school_diary.school_diary_tests.Tests.Controllers;

public class GradesControllerTests
{
    private static GradesController CreateController(
        IGradeService gradeService,
        string userId = "user-1",
        string role = "Admin")
    {
        var controller = new GradesController(gradeService);

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
    public async Task GetAll_ReturnsAllGrades_WhenUserIsAdmin()
    {
        var service = new Mock<IGradeService>();

        service
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<GradeDto>
            {
                new GradeDto(1, 6, 1, 1, 1, DateTime.Today)
            });

        var controller = CreateController(service.Object, role: "Admin");

        var result = await controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var grades = Assert.IsAssignableFrom<IReadOnlyList<GradeDto>>(ok.Value);

        Assert.Single(grades);

        service.Verify(x => x.GetAllAsync(), Times.Once);
        service.Verify(x => x.GetAllForTeacherAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetAll_ReturnsTeacherGrades_WhenUserIsTeacher()
    {
        var service = new Mock<IGradeService>();

        service
            .Setup(x => x.GetAllForTeacherAsync("teacher-user-1"))
            .ReturnsAsync(new List<GradeDto>
            {
                new GradeDto(1, 5, 1, 1, 1, DateTime.Today)
            });

        var controller = CreateController(
            service.Object,
            userId: "teacher-user-1",
            role: "Teacher");

        var result = await controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var grades = Assert.IsAssignableFrom<IReadOnlyList<GradeDto>>(ok.Value);

        Assert.Single(grades);

        service.Verify(x => x.GetAllForTeacherAsync("teacher-user-1"), Times.Once);
        service.Verify(x => x.GetAllAsync(), Times.Never);
    }

    [Fact]
    public async Task Get_ReturnsGrade_WhenAdminAndGradeExists()
    {
        var service = new Mock<IGradeService>();

        var dto = new GradeDto(1, 6, 1, 1, 1, DateTime.Today);

        service
            .Setup(x => x.GetAsync(1))
            .ReturnsAsync(dto);

        var controller = CreateController(service.Object, role: "Admin");

        var result = await controller.Get(1);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var model = Assert.IsType<GradeDto>(ok.Value);

        Assert.Equal(1, model.Id);
    }

    [Fact]
    public async Task Get_ReturnsNotFound_WhenGradeDoesNotExist()
    {
        var service = new Mock<IGradeService>();

        service
            .Setup(x => x.GetAsync(999))
            .ReturnsAsync((GradeDto?)null);

        var controller = CreateController(service.Object, role: "Admin");

        var result = await controller.Get(999);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Get_UsesTeacherServiceMethod_WhenUserIsTeacher()
    {
        var service = new Mock<IGradeService>();

        var dto = new GradeDto(1, 5, 1, 1, 1, DateTime.Today);

        service
            .Setup(x => x.GetForTeacherAsync(1, "teacher-user-1"))
            .ReturnsAsync(dto);

        var controller = CreateController(
            service.Object,
            userId: "teacher-user-1",
            role: "Teacher");

        var result = await controller.Get(1);

        Assert.IsType<OkObjectResult>(result.Result);

        service.Verify(
            x => x.GetForTeacherAsync(1, "teacher-user-1"),
            Times.Once);
    }

    [Fact]
    public async Task Get_UsesParentServiceMethod_WhenUserIsParent()
    {
        var service = new Mock<IGradeService>();

        var dto = new GradeDto(1, 5, 1, 1, 1, DateTime.Today);

        service
            .Setup(x => x.GetForParentAsync(1, "parent-user-1"))
            .ReturnsAsync(dto);

        var controller = CreateController(
            service.Object,
            userId: "parent-user-1",
            role: "Parent");

        var result = await controller.Get(1);

        Assert.IsType<OkObjectResult>(result.Result);

        service.Verify(
            x => x.GetForParentAsync(1, "parent-user-1"),
            Times.Once);
    }

    [Fact]
    public async Task Post_CreatesGrade()
    {
        var service = new Mock<IGradeService>();

        var input = new CreateGradeDto(1, 1, 1, 6);

        var created = new GradeDto(10, 6, 1, 1, 1, DateTime.Today);

        service
            .Setup(x => x.CreateAsync(input, "teacher-user-1"))
            .ReturnsAsync(created);

        var controller = CreateController(
            service.Object,
            userId: "teacher-user-1",
            role: "Teacher");

        var result = await controller.Post(input);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);

        Assert.Equal(nameof(GradesController.Get), createdResult.ActionName);
        Assert.Equal(10, createdResult.RouteValues!["id"]);

        var model = Assert.IsType<GradeDto>(createdResult.Value);

        Assert.Equal(10, model.Id);
    }

    [Fact]
    public async Task Put_UpdatesGrade()
    {
        var service = new Mock<IGradeService>();

        var dto = new UpdateGradeDto(5);

        service
            .Setup(x => x.UpdateAsync(1, dto, "teacher-user-1"))
            .Returns(Task.CompletedTask);

        var controller = CreateController(
            service.Object,
            userId: "teacher-user-1",
            role: "Teacher");

        var result = await controller.Put(1, dto);

        Assert.IsType<NoContentResult>(result);

        service.Verify(
            x => x.UpdateAsync(1, dto, "teacher-user-1"),
            Times.Once);
    }

    [Fact]
    public async Task Delete_UsesAdminDelete_WhenUserIsAdmin()
    {
        var service = new Mock<IGradeService>();

        service
            .Setup(x => x.DeleteAsAdminAsync(1))
            .Returns(Task.CompletedTask);

        var controller = CreateController(service.Object, role: "Admin");

        var result = await controller.Delete(1);

        Assert.IsType<NoContentResult>(result);

        service.Verify(x => x.DeleteAsAdminAsync(1), Times.Once);
        service.Verify(x => x.DeleteAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Delete_UsesTeacherDelete_WhenUserIsTeacher()
    {
        var service = new Mock<IGradeService>();

        service
            .Setup(x => x.DeleteAsync(1, "teacher-user-1"))
            .Returns(Task.CompletedTask);

        var controller = CreateController(
            service.Object,
            userId: "teacher-user-1",
            role: "Teacher");

        var result = await controller.Delete(1);

        Assert.IsType<NoContentResult>(result);

        service.Verify(x => x.DeleteAsync(1, "teacher-user-1"), Times.Once);
        service.Verify(x => x.DeleteAsAdminAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetGradesForMyChildren_ReturnsGrades()
    {
        var service = new Mock<IGradeService>();

        IReadOnlyList<GradeDto> grades = new List<GradeDto>
        {
            new GradeDto(1, 6, 1, 1, 1, DateTime.Today)
        };

        service
            .Setup(x => x.GetGradesForParentAsync("parent-user-1"))
            .ReturnsAsync(grades);

        var controller = CreateController(
            service.Object,
            userId: "parent-user-1",
            role: "Parent");

        var result = await controller.GetGradesForMyChildren();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var model = Assert.IsAssignableFrom<IReadOnlyList<GradeDto>>(ok.Value);

        Assert.Single(model);

        service.Verify(
            x => x.GetGradesForParentAsync("parent-user-1"),
            Times.Once);
    }
}