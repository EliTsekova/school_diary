namespace school_diary.school_diary_tests.Tests.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using school_diary.Controllers;
using school_diary.Dtos;
using school_diary.Services;


public class SchoolsControllerTests
{
    [Fact]
    public async Task ListSchool_ReturnsView_WithSchools()
    {
        var schoolService = new Mock<ISchoolService>();

        var schools = new List<SchoolDto>
        {
            new SchoolDto(1, "Test School", "Sofia"),
            new SchoolDto(2, "Second School", "Plovdiv")
        };

        schoolService
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(schools);

        var controller = new SchoolsController(schoolService.Object);

        var result = await controller.ListSchool();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<List<SchoolDto>>(viewResult.Model);

        Assert.Equal(2, model.Count);
        Assert.Equal("Test School", model[0].Name);

        schoolService.Verify(
            x => x.GetAllAsync(),
            Times.Once);
    }

    [Fact]
    public void AddSchool_Get_ReturnsView()
    {
        var schoolService = new Mock<ISchoolService>();

        var controller = new SchoolsController(schoolService.Object);

        var result = controller.AddSchool();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task AddSchool_Post_ReturnsView_WhenModelStateIsInvalid()
    {
        var schoolService = new Mock<ISchoolService>();

        var controller = new SchoolsController(schoolService.Object);

        controller.ModelState.AddModelError("Name", "Required");

        var dto = new CreateSchoolDto
        {
            Name = "",
            Address = "Sofia"
        };

        var result = await controller.AddSchool(dto);

        var viewResult = Assert.IsType<ViewResult>(result);

        Assert.Equal(dto, viewResult.Model);

        schoolService.Verify(
            x => x.CreateAsync(It.IsAny<CreateSchoolDto>()),
            Times.Never);
    }

    [Fact]
    public async Task AddSchool_Post_RedirectsToListSchool_WhenValid()
    {
        var schoolService = new Mock<ISchoolService>();

        schoolService
            .Setup(x => x.CreateAsync(It.IsAny<CreateSchoolDto>()))
            .ReturnsAsync(new SchoolDto(1, "Test School", "Sofia"));

        var controller = new SchoolsController(schoolService.Object);

        var dto = new CreateSchoolDto
        {
            Name = "Test School",
            Address = "Sofia"
        };

        var result = await controller.AddSchool(dto);

        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal(nameof(SchoolsController.ListSchool), redirect.ActionName);

        schoolService.Verify(
            x => x.CreateAsync(It.Is<CreateSchoolDto>(
                s => s.Name == "Test School" &&
                     s.Address == "Sofia")),
            Times.Once);
    }

    [Fact]
    public async Task EditSchool_Get_ReturnsNotFound_WhenSchoolMissing()
    {
        var schoolService = new Mock<ISchoolService>();

        schoolService
            .Setup(x => x.GetAsync(999))
            .ReturnsAsync((SchoolDto?)null);

        var controller = new SchoolsController(schoolService.Object);

        var result = await controller.EditSchool(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task EditSchool_Get_ReturnsView_WhenSchoolExists()
    {
        var schoolService = new Mock<ISchoolService>();

        schoolService
            .Setup(x => x.GetAsync(1))
            .ReturnsAsync(new SchoolDto(
                1,
                "Test School",
                "Sofia"));

        var controller = new SchoolsController(schoolService.Object);

        var result = await controller.EditSchool(1);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<UpdateSchoolDto>(viewResult.Model);

        Assert.Equal("Test School", model.Name);
        Assert.Equal("Sofia", model.Address);
        Assert.Equal(1, controller.ViewBag.Id);
    }

    [Fact]
    public async Task EditSchool_Post_ReturnsView_WhenModelStateIsInvalid()
    {
        var schoolService = new Mock<ISchoolService>();

        var controller = new SchoolsController(schoolService.Object);

        controller.ModelState.AddModelError("Name", "Required");

        var dto = new UpdateSchoolDto
        {
            Name = "",
            Address = "Sofia"
        };

        var result = await controller.EditSchool(1, dto);

        var viewResult = Assert.IsType<ViewResult>(result);

        Assert.Equal(dto, viewResult.Model);
        Assert.Equal(1, controller.ViewBag.Id);

        schoolService.Verify(
            x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<UpdateSchoolDto>()),
            Times.Never);
    }

    [Fact]
    public async Task EditSchool_Post_RedirectsToListSchool_WhenValid()
    {
        var schoolService = new Mock<ISchoolService>();

        schoolService
            .Setup(x => x.GetAsync(999))
            .Returns(Task.FromResult<SchoolDto?>(null));

        var controller = new SchoolsController(schoolService.Object);

        var dto = new UpdateSchoolDto
        {
            Name = "Updated School",
            Address = "Updated Address"
        };

        var result = await controller.EditSchool(1, dto);

        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal(nameof(SchoolsController.ListSchool), redirect.ActionName);

        schoolService.Verify(
            x => x.UpdateAsync(1, It.Is<UpdateSchoolDto>(
                s => s.Name == "Updated School" &&
                     s.Address == "Updated Address")),
            Times.Once);
    }

    [Fact]
    public async Task DeleteSchool_RedirectsToListSchool()
    {
        var schoolService = new Mock<ISchoolService>();

        schoolService
            .Setup(x => x.DeleteAsync(1))
            .Returns(Task.CompletedTask);

        var controller = new SchoolsController(schoolService.Object);

        var result = await controller.DeleteSchool(1);

        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal(nameof(SchoolsController.ListSchool), redirect.ActionName);

        schoolService.Verify(
            x => x.DeleteAsync(1),
            Times.Once);
    }
}