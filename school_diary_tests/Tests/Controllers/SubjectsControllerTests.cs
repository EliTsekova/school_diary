namespace school_diary.school_diary_tests.Tests.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using school_diary.Controllers;
using school_diary.Dtos;
using school_diary.Services;


public class SubjectsControllerTests
{
    [Fact]
    public async Task ListSubject_ReturnsView_WithSubjects()
    {
        var subjectService = new Mock<ISubjectService>();

        var subjects = new List<SubjectDto>
        {
            new SubjectDto(1, "Math"),
            new SubjectDto(2, "History")
        };

        subjectService
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(subjects);

        var controller = new SubjectsController(subjectService.Object);

        var result = await controller.ListSubject();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<List<SubjectDto>>(viewResult.Model);

        Assert.Equal(2, model.Count);
        Assert.Equal("Math", model[0].Name);

        subjectService.Verify(
            x => x.GetAllAsync(),
            Times.Once);
    }

    [Fact]
    public void AddSubject_Get_ReturnsView()
    {
        var subjectService = new Mock<ISubjectService>();
        var controller = new SubjectsController(subjectService.Object);

        var result = controller.AddSubject();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task AddSubject_Post_ReturnsView_WhenModelStateIsInvalid()
    {
        var subjectService = new Mock<ISubjectService>();
        var controller = new SubjectsController(subjectService.Object);

        controller.ModelState.AddModelError("Name", "Required");

        var dto = new SubjectDto(1, "");

        var result = await controller.AddSubject(dto);

        var viewResult = Assert.IsType<ViewResult>(result);

        Assert.Equal(dto, viewResult.Model);

        subjectService.Verify(
            x => x.CreateSubjectAsync(It.IsAny<SubjectDto>()),
            Times.Never);
    }

    [Fact]
    public async Task AddSubject_Post_RedirectsToListSubject_WhenValid()
    {
        var subjectService = new Mock<ISubjectService>();

        subjectService
            .Setup(x => x.CreateSubjectAsync(It.IsAny<SubjectDto>()))
            .ReturnsAsync(new SubjectDto(1, "Biology"));

        var controller = new SubjectsController(subjectService.Object);

        var dto = new SubjectDto(1, "Biology");

        var result = await controller.AddSubject(dto);

        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal(nameof(SubjectsController.ListSubject), redirect.ActionName);

        subjectService.Verify(
            x => x.CreateSubjectAsync(It.Is<SubjectDto>(
                s => s.Id == 1 && s.Name == "Biology")),
            Times.Once);
    }
}