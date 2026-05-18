using Microsoft.AspNetCore.Mvc;
using Moq;
using school_diary.Controllers.Api;
using school_diary.Dtos;
using school_diary.Services;

namespace school_diary.school_diary_tests.Tests.Controllers;

public class AdminControllerTests
{
    private static AdminController CreateController(
        IAdminStatisticsService? stats = null,
        ITeacherService? teachers = null,
        IStudentService? students = null,
        IParentService? parents = null,
        IDirectorService? directors = null,
        ISchoolService? schools = null,
        ISubjectService? subjects = null)
    {
        return new AdminController(
            stats ?? Mock.Of<IAdminStatisticsService>(),
            teachers ?? Mock.Of<ITeacherService>(),
            students ?? Mock.Of<IStudentService>(),
            parents ?? Mock.Of<IParentService>(),
            directors ?? Mock.Of<IDirectorService>(),
            schools ?? Mock.Of<ISchoolService>(),
            subjects ?? Mock.Of<ISubjectService>());
    }

    [Fact]
    public async Task GetGlobalSubjectAverages_ReturnsOk()
    {
        var stats = new Mock<IAdminStatisticsService>();

        stats
            .Setup(x => x.GetGlobalSubjectAveragesAsync())
            .ReturnsAsync(new List<SubjectAverageDto>());

        var controller = CreateController(stats: stats.Object);

        var result = await controller.GetGlobalSubjectAverages();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var model = Assert.IsAssignableFrom<IReadOnlyList<SubjectAverageDto>>(ok.Value);

        Assert.Empty(model);
    }

    [Fact]
    public async Task GetBySchool_ReturnsOk()
    {
        var stats = new Mock<IAdminStatisticsService>();

        stats
            .Setup(x => x.GetSubjectAveragesBySchoolAsync(1))
            .ReturnsAsync(new List<SubjectAverageDto>());

        var controller = CreateController(stats: stats.Object);

        var result = await controller.GetBySchool(1);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var model = Assert.IsAssignableFrom<IReadOnlyList<SubjectAverageDto>>(ok.Value);

        Assert.Empty(model);
    }

    [Fact]
    public async Task CreateTeacher_ReturnsCreatedAtAction()
    {
        var teachers = new Mock<ITeacherService>();

        var input = new CreateTeacherDto(
            "Ivan",
            "Ivanov",
            "teacher@test.com",
            "Password123!",
            1,
            new List<int> { 1 },
            new List<int> { 1 });

        var created = new TeacherDto(
            1,
            "Ivan Ivanov",
            "teacher@test.com",
            1,
            new List<int> { 1 },
            new List<int> { 1 });

        teachers
            .Setup(x => x.CreateAsync(input))
            .ReturnsAsync(created);

        var controller = CreateController(teachers: teachers.Object);

        var result = await controller.CreateTeacher(input);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);

        Assert.Equal(nameof(AdminController.CreateTeacher), createdResult.ActionName);

        var model = Assert.IsType<TeacherDto>(createdResult.Value);

        Assert.Equal(1, model.Id);
        Assert.Equal("Ivan Ivanov", model.FullName);
    }

    [Fact]
    public async Task CreateStudent_ReturnsCreatedAtAction()
    {
        var students = new Mock<IStudentService>();

        var input = new CreateStudentDto
        {
            FirstName = "Anna",
            LastName = "Petrova",
            Email = "student@test.com",
            Password = "Password123!",
            SchoolId = 1,
            ClassName = "8A"
        };

        var created = new CreatedStudentDto(
            1,
            "Anna Petrova",
            "student@test.com",
            1,
            "8A",
            "Password123!");

        students
            .Setup(x => x.CreateAsync(input))
            .ReturnsAsync(created);

        var controller = CreateController(students: students.Object);

        var result = await controller.CreateStudent(input);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);

        Assert.Equal(nameof(AdminController.CreateStudent), createdResult.ActionName);

        var model = Assert.IsType<CreatedStudentDto>(createdResult.Value);

        Assert.Equal(1, model.Id);
        Assert.Equal("Anna Petrova", model.FullName);
    }

    [Fact]
    public async Task CreateParent_ReturnsCreatedAtAction()
    {
        var parents = new Mock<IParentService>();

        var input = new CreateParentDto(
            "Maria",
            "Ivanova",
            "parent@test.com",
            "Password123!",
            new List<int> { 1 });

        var created = new ParentDto(
            1,
            "Maria Ivanova",
            "parent@test.com");

        parents
            .Setup(x => x.CreateAsync(input))
            .ReturnsAsync(created);

        var controller = CreateController(parents: parents.Object);

        var result = await controller.CreateParent(input);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);

        Assert.Equal(nameof(AdminController.CreateParent), createdResult.ActionName);

        var model = Assert.IsType<ParentDto>(createdResult.Value);

        Assert.Equal(1, model.Id);
        Assert.Equal("Maria Ivanova", model.FullName);
    }

    [Fact]
    public async Task CreateDirector_ReturnsCreatedAtAction()
    {
        var directors = new Mock<IDirectorService>();

        var input = new CreateDirectorDto
        {
            UserId = "director-user-1",
            SchoolId = 1
        };

        var created = new DirectorDto(
            1,
            "director-user-1",
            1);

        directors
            .Setup(x => x.CreateAsync(input))
            .ReturnsAsync(created);

        var controller = CreateController(directors: directors.Object);

        var result = await controller.CreateDirector(input);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);

        Assert.Equal(nameof(AdminController.CreateDirector), createdResult.ActionName);

        var model = Assert.IsType<DirectorDto>(createdResult.Value);

        Assert.Equal(1, model.Id);
        Assert.Equal("director-user-1", model.UserId);
    }

    [Fact]
    public async Task CreateSchool_ReturnsCreatedAtAction()
    {
        var schools = new Mock<ISchoolService>();

        var input = new CreateSchoolDto
        {
            Name = "Test School",
            Address = "Sofia"
        };

        var created = new SchoolDto(
            1,
            "Test School",
            "Sofia");

        schools
            .Setup(x => x.CreateAsync(input))
            .ReturnsAsync(created);

        var controller = CreateController(schools: schools.Object);

        var result = await controller.CreateSchool(input);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);

        Assert.Equal(nameof(AdminController.CreateSchool), createdResult.ActionName);

        var model = Assert.IsType<SchoolDto>(createdResult.Value);

        Assert.Equal(1, model.Id);
        Assert.Equal("Test School", model.Name);
    }

    [Fact]
    public async Task CreateSubject_ReturnsCreatedAtAction()
    {
        var subjects = new Mock<ISubjectService>();

        var input = new SubjectDto(
            1,
            "Math");

        var created = new SubjectDto(
            1,
            "Math");

        subjects
            .Setup(x => x.CreateSubjectAsync(input))
            .ReturnsAsync(created);

        var controller = CreateController(subjects: subjects.Object);

        var result = await controller.CreateSubject(input);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);

        Assert.Equal(nameof(AdminController.CreateSubject), createdResult.ActionName);

        var model = Assert.IsType<SubjectDto>(createdResult.Value);

        Assert.Equal(1, model.Id);
        Assert.Equal("Math", model.Name);
    }
}