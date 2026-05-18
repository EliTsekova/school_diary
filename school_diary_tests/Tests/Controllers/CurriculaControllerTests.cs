using Microsoft.AspNetCore.Mvc;
using Moq;
using school_diary.Controllers.Api;
using school_diary.Dtos;
using school_diary.Services;

namespace school_diary.school_diary_tests.Tests.Controllers;

public class CurriculaControllerTests
{
    [Fact]
    public async Task GetAll_ReturnsOk_WithCurricula()
    {
        var service = new Mock<ICurriculumService>();

        service
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<CurriculumDto>());

        var controller = new CurriculaController(service.Object);

        var result = await controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result.Result);

        var model =
            Assert.IsAssignableFrom<IReadOnlyList<CurriculumDto>>(ok.Value);

        Assert.Empty(model);

        service.Verify(
            x => x.GetAllAsync(),
            Times.Once);
    }

    [Fact]
    public async Task Get_ReturnsOk_WhenCurriculumExists()
    {
        var service = new Mock<ICurriculumService>();

        var dto = new CurriculumDto(
            1,
            "First Term",
            1,
            1,
            "8A",
            new List<CurriculumEntryDto>());

        service
            .Setup(x => x.GetAsync(1))
            .ReturnsAsync(dto);

        var controller = new CurriculaController(service.Object);

        var result = await controller.Get(1);

        var ok = Assert.IsType<OkObjectResult>(result.Result);

        var model = Assert.IsType<CurriculumDto>(ok.Value);

        Assert.Equal(1, model.Id);
        Assert.Equal("First Term", model.Term);
    }

    [Fact]
    public async Task Get_ReturnsNotFound_WhenCurriculumDoesNotExist()
    {
        var service = new Mock<ICurriculumService>();

        service
            .Setup(x => x.GetAsync(99))
            .ReturnsAsync((CurriculumDto?)null);

        var controller = new CurriculaController(service.Object);

        var result = await controller.Get(99);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Post_ReturnsCreatedAtAction()
    {
        var service = new Mock<ICurriculumService>();

        var input = new CreateCurriculumDto(
            "First Term",
            1,
            new List<CreateCurriculumEntryDto>());

        var created = new CurriculumDto(
            10,
            "First Term",
            1,
            1,
            "8A",
            new List<CurriculumEntryDto>());

        service
            .Setup(x => x.CreateAsync(input))
            .ReturnsAsync(created);

        var controller = new CurriculaController(service.Object);

        var result = await controller.Post(input);

        var createdResult =
            Assert.IsType<CreatedAtActionResult>(result.Result);

        Assert.Equal(
            nameof(CurriculaController.Get),
            createdResult.ActionName);

        Assert.Equal(
            10,
            createdResult.RouteValues!["id"]);

        var model =
            Assert.IsType<CurriculumDto>(createdResult.Value);

        Assert.Equal(10, model.Id);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent()
    {
        var service = new Mock<ICurriculumService>();

        service
            .Setup(x => x.DeleteAsync(1))
            .Returns(Task.CompletedTask);

        var controller = new CurriculaController(service.Object);

        var result = await controller.Delete(1);

        Assert.IsType<NoContentResult>(result);

        service.Verify(
            x => x.DeleteAsync(1),
            Times.Once);
    }
}