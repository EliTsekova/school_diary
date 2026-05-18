namespace school_diary.school_diary_tests.Tests.Controllers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using school_diary.Controllers;
using school_diary.Data;
using school_diary.Models;
using school_diary.Services;
using Microsoft.AspNetCore.Mvc;
using school_diary.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using school_diary.ViewModels;
using school_diary.Views.AdminUi;

public class AdminUiControllerTests
{
    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("AdminUiTests_" + Guid.NewGuid())
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

    private static Mock<SignInManager<User>> CreateSignInManagerMock(
        UserManager<User> userManager)
    {
        var contextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        var claimsFactory = new Mock<IUserClaimsPrincipalFactory<User>>();

        return new Mock<SignInManager<User>>(
            userManager,
            contextAccessor.Object,
            claimsFactory.Object,
            null!, null!, null!, null!);
    }

    private static AdminUiController CreateController(
        ApplicationDbContext db,
        ISchoolService? schools = null,
        IAdminStatisticsService? stats = null,
        SignInManager<User>? signInManager = null,
        IStudentService? students = null)
    {
        var parents = new Mock<IParentService>();
        var directors = new Mock<IDirectorService>();
        var subjects = new Mock<ISubjectService>();
        var teachers = new Mock<ITeacherService>();
        var curricula = new Mock<ICurriculumService>();

        var userManager = CreateUserManagerMock();
        var defaultSignInManager = CreateSignInManagerMock(userManager.Object);

        var controller = new AdminUiController(
            parents.Object,
            directors.Object,
            schools ?? Mock.Of<ISchoolService>(),
            subjects.Object,
            teachers.Object,
            curricula.Object,
            students ?? Mock.Of<IStudentService>(),
            stats ?? Mock.Of<IAdminStatisticsService>(),
            db,
            userManager.Object,
            signInManager ?? defaultSignInManager.Object);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        controller.TempData = new TempDataDictionary(
            controller.HttpContext,
            Mock.Of<ITempDataProvider>());

        return controller;
    }
    
    private static AdminUiController CreateControllerWithParents(
        ApplicationDbContext db,
        IParentService parents)
    {
        var directors = new Mock<IDirectorService>();
        var subjects = new Mock<ISubjectService>();
        var teachers = new Mock<ITeacherService>();
        var curricula = new Mock<ICurriculumService>();
        var students = new Mock<IStudentService>();

        var userManager = CreateUserManagerMock();
        var signInManager = CreateSignInManagerMock(userManager.Object);

        var controller = new AdminUiController(
            parents,
            directors.Object,
            Mock.Of<ISchoolService>(),
            subjects.Object,
            teachers.Object,
            curricula.Object,
            students.Object,
            Mock.Of<IAdminStatisticsService>(),
            db,
            userManager.Object,
            signInManager.Object);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        controller.TempData = new TempDataDictionary(
            controller.HttpContext,
            Mock.Of<ITempDataProvider>());

        return controller;
    }
    private static AdminUiController CreateControllerWithCurricula(
        ApplicationDbContext db,
        ICurriculumService curricula)
    {
        var parents = new Mock<IParentService>();
        var directors = new Mock<IDirectorService>();
        var subjects = new Mock<ISubjectService>();
        var teachers = new Mock<ITeacherService>();
        var students = new Mock<IStudentService>();

        var userManager = CreateUserManagerMock();
        var signInManager = CreateSignInManagerMock(userManager.Object);

        var controller = new AdminUiController(
            parents.Object,
            directors.Object,
            Mock.Of<ISchoolService>(),
            subjects.Object,
            teachers.Object,
            curricula,
            students.Object,
            Mock.Of<IAdminStatisticsService>(),
            db,
            userManager.Object,
            signInManager.Object);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        controller.TempData = new TempDataDictionary(
            controller.HttpContext,
            Mock.Of<ITempDataProvider>());

        return controller;
    }
    
    [Fact]
    public async Task Index_ReturnsView_WithCounts()
    {
        var db = CreateDb();

        db.Schools.Add(new School
        {
            Id = 1,
            Name = "Test School",
            Address = "Sofia"
        });

        db.Classes.Add(new Class
        {
            Id = 1,
            Name = "8A",
            SchoolId = 1
        });

        db.Users.AddRange(
            new User
            {
                Id = "student-user",
                FirstName = "Ivan",
                LastName = "Ivanov",
                Email = "student@test.com",
                UserName = "student@test.com",
                Role = Role.Student
            },
            new User
            {
                Id = "teacher-user",
                FirstName = "Petar",
                LastName = "Petrov",
                Email = "teacher@test.com",
                UserName = "teacher@test.com",
                Role = Role.Teacher
            },
            new User
            {
                Id = "parent-user",
                FirstName = "Maria",
                LastName = "Petrova",
                Email = "parent@test.com",
                UserName = "parent@test.com",
                Role = Role.Parent
            });

        db.Students.Add(new Student
        {
            Id = 1,
            UserId = "student-user",
            SchoolId = 1,
            ClassId = 1
        });

        db.Teachers.Add(new Teacher
        {
            Id = 1,
            UserId = "teacher-user",
            SchoolId = 1
        });

        db.Parents.Add(new Parent
        {
            Id = 1,
            UserId = "parent-user"
        });

        await db.SaveChangesAsync();

        var controller = CreateController(db);

        var result = await controller.Index();

        Assert.IsType<Microsoft.AspNetCore.Mvc.ViewResult>(result);

        Assert.Equal(1, controller.ViewBag.StudentsCount);
        Assert.Equal(1, controller.ViewBag.TeachersCount);
        Assert.Equal(1, controller.ViewBag.ParentsCount);
        Assert.Equal(1, controller.ViewBag.SchoolsCount);
    }
    
    [Fact]
public void AddSchool_Get_ReturnsView()
{
    var db = CreateDb();
    var controller = CreateController(db);

    var result = controller.AddSchool();

    Assert.IsType<ViewResult>(result);
}

[Fact]
public async Task AddSchool_Post_ReturnsView_WhenModelStateIsInvalid()
{
    var db = CreateDb();
    var controller = CreateController(db);

    controller.ModelState.AddModelError("Name", "Required");

    var input = new CreateSchoolDto
    {
        Name = "",
        Address = "Sofia"
    };

    var result = await controller.AddSchool(input);

    var viewResult = Assert.IsType<ViewResult>(result);
    Assert.Equal(input, viewResult.Model);
}

private static AdminUiController CreateControllerWithDirectors(
    ApplicationDbContext db,
    IDirectorService directors)
{
    var parents = new Mock<IParentService>();
    var subjects = new Mock<ISubjectService>();
    var teachers = new Mock<ITeacherService>();
    var curricula = new Mock<ICurriculumService>();
    var students = new Mock<IStudentService>();

    var userManager = CreateUserManagerMock();
    var signInManager = CreateSignInManagerMock(userManager.Object);

    var controller = new AdminUiController(
        parents.Object,
        directors,
        Mock.Of<ISchoolService>(),
        subjects.Object,
        teachers.Object,
        curricula.Object,
        students.Object,
        Mock.Of<IAdminStatisticsService>(),
        db,
        userManager.Object,
        signInManager.Object);

    controller.ControllerContext = new ControllerContext
    {
        HttpContext = new DefaultHttpContext()
    };

    controller.TempData = new TempDataDictionary(
        controller.HttpContext,
        Mock.Of<ITempDataProvider>());

    return controller;
}

[Fact]
public async Task AddSchool_Post_ReturnsView_WhenSchoolAlreadyExists()
{
    var db = CreateDb();

    db.Schools.Add(new School
    {
        Id = 1,
        Name = "Test School",
        Address = "Sofia"
    });

    await db.SaveChangesAsync();

    var controller = CreateController(db);

    var input = new CreateSchoolDto
    {
        Name = "Test School",
        Address = "Sofia"
    };

    var result = await controller.AddSchool(input);

    var viewResult = Assert.IsType<ViewResult>(result);

    Assert.Equal(input, viewResult.Model);
    Assert.False(controller.ModelState.IsValid);
}

[Fact]
public async Task AddSchool_Post_RedirectsToSchools_WhenValid()
{
    var db = CreateDb();

    var schools = new Mock<ISchoolService>();

    schools
        .Setup(x => x.CreateAsync(It.IsAny<CreateSchoolDto>()))
        .ReturnsAsync(new SchoolDto(1, "New School", "Sofia"));

    var controller = CreateController(db, schools: schools.Object);

    var input = new CreateSchoolDto
    {
        Name = "New School",
        Address = "Sofia"
    };

    var result = await controller.AddSchool(input);

    var redirect = Assert.IsType<RedirectToActionResult>(result);

    Assert.Equal(nameof(AdminUiController.Schools), redirect.ActionName);

    schools.Verify(
        x => x.CreateAsync(It.Is<CreateSchoolDto>(
            dto => dto.Name == "New School" &&
                   dto.Address == "Sofia")),
        Times.Once);
}

[Fact]
public async Task EditSchool_Get_ReturnsNotFound_WhenSchoolDoesNotExist()
{
    var db = CreateDb();

    var schools = new Mock<ISchoolService>();

    schools
        .Setup(x => x.GetAsync(99))
        .ReturnsAsync((SchoolDto?)null);

    var controller = CreateController(db, schools: schools.Object);

    var result = await controller.EditSchool(99);

    Assert.IsType<NotFoundResult>(result);
}

[Fact]
public async Task EditSchool_Get_ReturnsView_WhenSchoolExists()
{
    var db = CreateDb();

    var schools = new Mock<ISchoolService>();

    schools
        .Setup(x => x.GetAsync(1))
        .ReturnsAsync(new SchoolDto(
            1,
            "Test School",
            "Sofia"
        ));
    
    var controller = CreateController(db, schools: schools.Object);

    var result = await controller.EditSchool(1);

    var viewResult = Assert.IsType<ViewResult>(result);
    var model = Assert.IsType<UpdateSchoolDto>(viewResult.Model);

    Assert.Equal("Schools/Edit", viewResult.ViewName);
    Assert.Equal("Test School", model.Name);
    Assert.Equal("Sofia", model.Address);
    Assert.Equal(1, controller.ViewBag.Id);
}

[Fact]
public async Task EditSchool_Post_ReturnsView_WhenModelStateIsInvalid()
{
    var db = CreateDb();
    var controller = CreateController(db);

    controller.ModelState.AddModelError("Name", "Required");

    var input = new UpdateSchoolDto
    {
        Name = "",
        Address = "Sofia"
    };

    var result = await controller.EditSchool(1, input);

    var viewResult = Assert.IsType<ViewResult>(result);

    Assert.Equal("Schools/Edit", viewResult.ViewName);
    Assert.Equal(input, viewResult.Model);
    Assert.Equal(1, controller.ViewBag.Id);
}

[Fact]
public async Task EditSchool_Post_ReturnsView_WhenDuplicateSchoolExists()
{
    var db = CreateDb();

    db.Schools.AddRange(
        new School
        {
            Id = 1,
            Name = "Old School",
            Address = "Old Address"
        },
        new School
        {
            Id = 2,
            Name = "Duplicate School",
            Address = "Sofia"
        });

    await db.SaveChangesAsync();

    var controller = CreateController(db);

    var input = new UpdateSchoolDto
    {
        Name = "Duplicate School",
        Address = "Sofia"
    };

    var result = await controller.EditSchool(1, input);

    var viewResult = Assert.IsType<ViewResult>(result);

    Assert.Equal("Schools/Edit", viewResult.ViewName);
    Assert.Equal(input, viewResult.Model);
    Assert.False(controller.ModelState.IsValid);
    Assert.Equal(1, controller.ViewBag.Id);
}

[Fact]
public async Task EditSchool_Post_RedirectsToSchools_WhenValid()
{
    var db = CreateDb();

    db.Schools.Add(new School
    {
        Id = 1,
        Name = "Old School",
        Address = "Old Address"
    });

    await db.SaveChangesAsync();

    var schools = new Mock<ISchoolService>();

    schools
        .Setup(x => x.UpdateAsync(1, It.IsAny<UpdateSchoolDto>()))
        .Returns(Task.CompletedTask);

    var controller = CreateController(db, schools: schools.Object);

    var input = new UpdateSchoolDto
    {
        Name = "Updated School",
        Address = "Updated Address"
    };

    var result = await controller.EditSchool(1, input);

    var redirect = Assert.IsType<RedirectToActionResult>(result);

    Assert.Equal(nameof(AdminUiController.Schools), redirect.ActionName);

    schools.Verify(
        x => x.UpdateAsync(1, It.Is<UpdateSchoolDto>(
            dto => dto.Name == "Updated School" &&
                   dto.Address == "Updated Address")),
        Times.Once);
}
[Fact]
public void AddSubject_Get_ReturnsView()
{
    var db = CreateDb();
    var controller = CreateController(db);

    var result = controller.AddSubject();

    Assert.IsType<ViewResult>(result);
}

[Fact]
public async Task AddSubject_Post_ReturnsView_WhenNameIsEmpty()
{
    var db = CreateDb();
    var controller = CreateController(db);

    var result = await controller.AddSubject("");

    Assert.IsType<ViewResult>(result);
    Assert.False(controller.ModelState.IsValid);
}

[Fact]
public async Task AddSubject_Post_RedirectsToSubjects_WhenNameIsValid()
{
    var db = CreateDb();
    var controller = CreateController(db);

    var result = await controller.AddSubject(" Math ");

    var redirect = Assert.IsType<RedirectToActionResult>(result);

    Assert.Equal(nameof(AdminUiController.Subjects), redirect.ActionName);
    Assert.Single(db.Subjects);
    Assert.Equal("Math", db.Subjects.First().Name);
}

[Fact]
public async Task EditSubject_Get_ReturnsNotFound_WhenSubjectDoesNotExist()
{
    var db = CreateDb();
    var controller = CreateController(db);

    var result = await controller.EditSubject(99);

    Assert.IsType<NotFoundResult>(result);
}

[Fact]
public async Task EditSubject_Get_ReturnsView_WhenSubjectExists()
{
    var db = CreateDb();

    db.Subjects.Add(new Subject
    {
        Id = 1,
        Name = "Math"
    });

    await db.SaveChangesAsync();

    var controller = CreateController(db);

    var result = await controller.EditSubject(1);

    var viewResult = Assert.IsType<ViewResult>(result);
    var model = Assert.IsType<SubjectDto>(viewResult.Model);

    Assert.Equal("Subjects/Edit", viewResult.ViewName);
    Assert.Equal(1, model.Id);
    Assert.Equal("Math", model.Name);
}

[Fact]
public async Task EditSubject_Post_RedirectsToEditSubject_WhenNameIsEmpty()
{
    var db = CreateDb();
    var controller = CreateController(db);

    var result = await controller.EditSubject(1, "");

    var redirect = Assert.IsType<RedirectToActionResult>(result);

    Assert.Equal(nameof(AdminUiController.EditSubject), redirect.ActionName);
    Assert.Equal(1, redirect.RouteValues!["id"]);
    Assert.False(controller.ModelState.IsValid);
}

[Fact]
public async Task EditSubject_Post_ReturnsNotFound_WhenSubjectDoesNotExist()
{
    var db = CreateDb();
    var controller = CreateController(db);

    var result = await controller.EditSubject(99, "Physics");

    Assert.IsType<NotFoundResult>(result);
}

[Fact]
public async Task EditSubject_Post_RedirectsToSubjects_WhenValid()
{
    var db = CreateDb();

    db.Subjects.Add(new Subject
    {
        Id = 1,
        Name = "Math"
    });

    await db.SaveChangesAsync();

    var controller = CreateController(db);

    var result = await controller.EditSubject(1, "Physics");

    var redirect = Assert.IsType<RedirectToActionResult>(result);

    Assert.Equal(nameof(AdminUiController.Subjects), redirect.ActionName);
    Assert.Equal("Physics", db.Subjects.First().Name);
}

[Fact]
public async Task DeleteSubject_ReturnsNotFound_WhenSubjectDoesNotExist()
{
    var db = CreateDb();
    var controller = CreateController(db);

    var result = await controller.DeleteSubject(99);

    Assert.IsType<NotFoundResult>(result);
}

[Fact]
public async Task DeleteSubject_RedirectsToSubjects_WhenSubjectIsUsed()
{
    var db = CreateDb();

    db.Subjects.Add(new Subject
    {
        Id = 1,
        Name = "Math"
    });

    db.TeacherSubjects.Add(new TeacherSubject
    {
        Id = 1,
        SubjectId = 1,
        TeacherId = 1
    });

    await db.SaveChangesAsync();

    var controller = CreateController(db);

    var result = await controller.DeleteSubject(1);

    var redirect = Assert.IsType<RedirectToActionResult>(result);

    Assert.Equal(nameof(AdminUiController.Subjects), redirect.ActionName);
    Assert.Equal("Cannot delete subject because it is already used in teachers, curricula, grades or absences.", controller.TempData["Error"]);
    Assert.Single(db.Subjects);
}

[Fact]
public async Task DeleteSubject_RedirectsToSubjects_WhenSubjectIsDeleted()
{
    var db = CreateDb();

    db.Subjects.Add(new Subject
    {
        Id = 1,
        Name = "Math"
    });

    await db.SaveChangesAsync();

    var controller = CreateController(db);

    var result = await controller.DeleteSubject(1);

    var redirect = Assert.IsType<RedirectToActionResult>(result);

    Assert.Equal(nameof(AdminUiController.Subjects), redirect.ActionName);
    Assert.Empty(db.Subjects);
    Assert.Equal("Subject deleted successfully.", controller.TempData["Success"]);
}
[Fact]
public async Task Statistics_ReturnsView_WithGlobalStats_WhenSchoolIdIsNull()
{
    var db = CreateDb();

    var schools = new Mock<ISchoolService>();
    var stats = new Mock<IAdminStatisticsService>();

    schools
        .Setup(x => x.GetAllAsync())
        .ReturnsAsync(new List<SchoolDto>
        {
            new SchoolDto(1, "Test School", "Sofia")
        });

    stats
        .Setup(x => x.GetGlobalSubjectAveragesAsync())
        .ReturnsAsync(new List<SubjectAverageDto>
        {
            new SubjectAverageDto("Math", 5.50)
        });

    var controller = CreateController(
        db,
        schools: schools.Object,
        stats: stats.Object);

    var result = await controller.Statistics(null);

    Assert.IsType<ViewResult>(result);

    var subjectAverages = Assert.IsType<List<SubjectAverageDto>>(
        (object)controller.ViewBag.SubjectAverages);

    Assert.Single(subjectAverages);
    Assert.Equal("Math", subjectAverages[0].SubjectName);

    stats.Verify(
        x => x.GetGlobalSubjectAveragesAsync(),
        Times.Once);

    stats.Verify(
        x => x.GetSubjectAveragesBySchoolAsync(It.IsAny<int>()),
        Times.Never);
}
[Fact]
public async Task DeleteSchool_RedirectsToSchools_WhenDeleteSucceeds()
{
    var db = CreateDb();

    var schools = new Mock<ISchoolService>();

    schools
        .Setup(x => x.DeleteAsync(1))
        .Returns(Task.CompletedTask);

    var controller = CreateController(db, schools: schools.Object);

    var result = await controller.DeleteSchool(1);

    var redirect = Assert.IsType<RedirectToActionResult>(result);

    Assert.Equal(nameof(AdminUiController.Schools), redirect.ActionName);

    schools.Verify(
        x => x.DeleteAsync(1),
        Times.Once);
}

[Fact]
public async Task DeleteSchool_SetsErrorAndRedirects_WhenServiceThrows()
{
    var db = CreateDb();

    var schools = new Mock<ISchoolService>();

    schools
        .Setup(x => x.DeleteAsync(1))
        .ThrowsAsync(new InvalidOperationException("Cannot delete school."));

    var controller = CreateController(db, schools: schools.Object);

    var result = await controller.DeleteSchool(1);

    var redirect = Assert.IsType<RedirectToActionResult>(result);

    Assert.Equal(nameof(AdminUiController.Schools), redirect.ActionName);
    Assert.Equal("Cannot delete school.", controller.TempData["Error"]);
}
[Fact]
public async Task Statistics_ReturnsView_WithSchoolStats_WhenSchoolIdIsProvided()
{
    var db = CreateDb();

    var schools = new Mock<ISchoolService>();
    var stats = new Mock<IAdminStatisticsService>();

    schools
        .Setup(x => x.GetAllAsync())
        .ReturnsAsync(new List<SchoolDto>
        {
            new SchoolDto(1, "Test School", "Sofia")
        });

    stats
        .Setup(x => x.GetSubjectAveragesBySchoolAsync(1))
        .ReturnsAsync(new List<SubjectAverageDto>
        {
            new SubjectAverageDto("Biology", 5.80)
        });

    var controller = CreateController(
        db,
        schools: schools.Object,
        stats: stats.Object);

    var result = await controller.Statistics(1);

    Assert.IsType<ViewResult>(result);

    var subjectAverages = Assert.IsType<List<SubjectAverageDto>>(
        (object)controller.ViewBag.SubjectAverages);

    Assert.Single(subjectAverages);
    Assert.Equal("Biology", subjectAverages[0].SubjectName);

    stats.Verify(
        x => x.GetSubjectAveragesBySchoolAsync(1),
        Times.Once);

    stats.Verify(
        x => x.GetGlobalSubjectAveragesAsync(),
        Times.Never);
}
[Fact]
public async Task Logout_SignsOutAndRedirectsToLogin()
{
    var db = CreateDb();

    var userManager = CreateUserManagerMock();
    var signInManager = CreateSignInManagerMock(userManager.Object);

    signInManager
        .Setup(x => x.SignOutAsync())
        .Returns(Task.CompletedTask);

    var controller = CreateController(
        db,
        signInManager: signInManager.Object);

    var result = await controller.Logout();

    var redirect = Assert.IsType<RedirectResult>(result);

    Assert.Equal("/Identity/Account/Login", redirect.Url);

    signInManager.Verify(
        x => x.SignOutAsync(),
        Times.Once);
}
[Fact]
public async Task Students_ReturnsView_WithStudentsModel()
{
    var db = CreateDb();

    var students = new Mock<IStudentService>();

    var studentList = new List<StudentDto>
    {
        new StudentDto(
            1,
            "Ivan Ivanov",
            "ivan@test.com",
            "Test School",
            1)
    };

    students
        .Setup(x => x.GetAllAsync())
        .ReturnsAsync(studentList);

    var controller = CreateController(
        db,
        students: students.Object);

    var result = await controller.Students();

    var viewResult = Assert.IsType<ViewResult>(result);

    Assert.Equal("Students", viewResult.ViewName);

    var model = Assert.IsType<List<StudentDto>>(viewResult.Model);

    Assert.Single(model);
    Assert.Equal("Ivan Ivanov", model[0].FullName);

    students.Verify(
        x => x.GetAllAsync(),
        Times.Once);
}

[Fact]
public async Task Schools_ReturnsView_WithSchoolsModel()
{
    var db = CreateDb();

    var schools = new Mock<ISchoolService>();

    var schoolList = new List<SchoolDto>
    {
        new SchoolDto(1, "Test School", "Sofia")
    };

    schools
        .Setup(x => x.GetAllAsync())
        .ReturnsAsync(schoolList);

    var controller = CreateController(
        db,
        schools: schools.Object);

    var result = await controller.Schools();

    var viewResult = Assert.IsType<ViewResult>(result);

    Assert.Equal("Schools/Index", viewResult.ViewName);

    var model = Assert.IsType<List<SchoolDto>>(viewResult.Model);

    Assert.Single(model);
    Assert.Equal("Test School", model[0].Name);

    schools.Verify(
        x => x.GetAllAsync(),
        Times.Once);
}
[Fact]
public async Task EditSchool_Get_ReturnsNotFound_WhenSchoolMissing()
{
    var db = CreateDb();

    var schools = new Mock<ISchoolService>();

    schools
        .Setup(x => x.GetAsync(999))
        .ReturnsAsync((SchoolDto?)null);

    var controller = CreateController(
        db,
        schools: schools.Object);

    var result = await controller.EditSchool(999);

    Assert.IsType<NotFoundResult>(result);
}

[Fact]
public async Task DeleteSchool_Redirects_WhenDeleteSucceeds()
{
    var db = CreateDb();

    var schools = new Mock<ISchoolService>();

    schools
        .Setup(x => x.DeleteAsync(1))
        .Returns(Task.CompletedTask);

    var controller = CreateController(
        db,
        schools: schools.Object);

    var result = await controller.DeleteSchool(1);

    var redirect = Assert.IsType<RedirectToActionResult>(result);

    Assert.Equal(nameof(AdminUiController.Schools), redirect.ActionName);

    schools.Verify(
        x => x.DeleteAsync(1),
        Times.Once);
}

[Fact]
public async Task DeleteSchool_SetsError_WhenDeleteFails()
{
    var db = CreateDb();

    var schools = new Mock<ISchoolService>();

    schools
        .Setup(x => x.DeleteAsync(1))
        .ThrowsAsync(new InvalidOperationException("Delete failed"));

    var controller = CreateController(
        db,
        schools: schools.Object);

    var result = await controller.DeleteSchool(1);

    var redirect = Assert.IsType<RedirectToActionResult>(result);

    Assert.Equal(nameof(AdminUiController.Schools), redirect.ActionName);

    Assert.Equal(
        "Delete failed",
        controller.TempData["Error"]);
}
[Fact]
public async Task AddStudent_Get_ReturnsView_WithDropdownLists()
{
    var db = CreateDb();

    db.Schools.Add(new School
    {
        Id = 1,
        Name = "Test School",
        Address = "Sofia"
    });

    db.Classes.Add(new Class
    {
        Id = 1,
        Name = "8A",
        SchoolId = 1
    });

    await db.SaveChangesAsync();

    var controller = CreateController(db);

    var result = await controller.AddStudent();

    var viewResult = Assert.IsType<ViewResult>(result);
    var model = Assert.IsType<AddStudentViewModel>(viewResult.Model);

    Assert.Equal("~/Views/Student/AddStudent.cshtml", viewResult.ViewName);
    Assert.Single(model.Schools);
    Assert.Single(model.Classes);
}

[Fact]
public async Task AddStudent_Post_ReturnsView_WhenModelStateIsInvalid()
{
    var db = CreateDb();

    db.Schools.Add(new School
    {
        Id = 1,
        Name = "Test School",
        Address = "Sofia"
    });

    db.Classes.Add(new Class
    {
        Id = 1,
        Name = "8A",
        SchoolId = 1
    });

    await db.SaveChangesAsync();

    var controller = CreateController(db);

    controller.ModelState.AddModelError("Email", "Required");

    var model = new AddStudentViewModel
    {
        FirstName = "",
        LastName = "Ivanov",
        Email = "",
        Password = "Password123!",
        SelectedSchoolId = 1,
        SelectedClassName = "8A"
    };

    var result = await controller.AddStudent(model);

    var viewResult = Assert.IsType<ViewResult>(result);

    Assert.Equal("~/Views/Student/AddStudent.cshtml", viewResult.ViewName);
    Assert.Equal(model, viewResult.Model);
    Assert.Single(model.Schools);
    Assert.Single(model.Classes);
}

[Fact]
public async Task EditStudent_Get_ReturnsNotFound_WhenStudentDoesNotExist()
{
    var db = CreateDb();
    var controller = CreateController(db);

    var result = await controller.EditStudent(999);

    Assert.IsType<NotFoundResult>(result);
}

[Fact]
public async Task DeleteStudent_ReturnsNotFound_WhenStudentDoesNotExist()
{
    var db = CreateDb();
    var controller = CreateController(db);

    var result = await controller.DeleteStudent(999);

    Assert.IsType<NotFoundResult>(result);
}
[Fact]
public async Task AddParent_Get_ReturnsView()
{
    var db = CreateDb();

    var controller = CreateController(db);

    var result = await controller.AddParent();

    var viewResult = Assert.IsType<ViewResult>(result);

    Assert.Equal(
        "~/Views/AdminUi/AddParent.cshtml",
        viewResult.ViewName);

    Assert.NotNull(controller.ViewBag.Students);
}

[Fact]
public async Task AddParent_Post_ReturnsView_WhenModelStateIsInvalid()
{
    var db = CreateDb();

    var controller = CreateController(db);

    controller.ModelState.AddModelError("Email", "Required");

    var input = new AddParentModel.ParentInputModel
    {
        FirstName = "Maria",
        LastName = "Ivanova",
        Email = "",
        Password = "Password123!"
    };

    var result = await controller.AddParent(input);

    var viewResult = Assert.IsType<ViewResult>(result);

    Assert.Equal(
        "~/Views/AdminUi/AddParent.cshtml",
        viewResult.ViewName);

    Assert.Equal(input, viewResult.Model);
}

[Fact]
public async Task EditParent_Get_ReturnsNotFound_WhenParentDoesNotExist()
{
    var db = CreateDb();

    var parents = new Mock<IParentService>();

    parents
        .Setup(x => x.GetAsync(999))
        .ReturnsAsync((ParentDto?)null);

    var controller = CreateControllerWithParents(
        db,
        parents.Object);

    var result = await controller.EditParent(999);

    Assert.IsType<NotFoundResult>(result);
}

[Fact]
public async Task DeleteParent_RedirectsToParents()
{
    var db = CreateDb();

    var parents = new Mock<IParentService>();

    parents
        .Setup(x => x.DeleteAsync(1))
        .Returns(Task.CompletedTask);

    var controller = CreateControllerWithParents(
        db,
        parents.Object);

    var result = await controller.DeleteParent(1);

    var redirect = Assert.IsType<RedirectToActionResult>(result);

    Assert.Equal(nameof(AdminUiController.Parents), redirect.ActionName);

    parents.Verify(
        x => x.DeleteAsync(1),
        Times.Once);
}
[Fact]
public async Task Directors_ReturnsView_WithModel()
{
    var db = CreateDb();

    var directors = new Mock<IDirectorService>();

    directors
        .Setup(x => x.GetAllAsync())
        .ReturnsAsync(new List<DirectorDto>());

    var controller = CreateControllerWithDirectors(
        db,
        directors.Object);

    var result = await controller.Directors();

    var viewResult = Assert.IsType<ViewResult>(result);

    Assert.Equal("Directors/Index", viewResult.ViewName);

    directors.Verify(
        x => x.GetAllAsync(),
        Times.Once);
}

[Fact]
public async Task EditDirector_Get_ReturnsNotFound_WhenDirectorMissing()
{
    var db = CreateDb();

    var controller = CreateController(db);

    var result = await controller.EditDirector(999);

    Assert.IsType<NotFoundResult>(result);
}

[Fact]
public async Task DeleteDirector_RedirectsToDirectors()
{
    var db = CreateDb();

    var directors = new Mock<IDirectorService>();

    directors
        .Setup(x => x.DeleteAsync(1))
        .Returns(Task.CompletedTask);

    var controller = CreateControllerWithDirectors(
        db,
        directors.Object);

    var result = await controller.DeleteDirector(1);

    var redirect = Assert.IsType<RedirectToActionResult>(result);

    Assert.Equal(nameof(AdminUiController.Directors), redirect.ActionName);

    directors.Verify(
        x => x.DeleteAsync(1),
        Times.Once);
}
[Fact]
public async Task Curricula_ReturnsView_WithModel()
{
    var db = CreateDb();

    var curricula = new Mock<ICurriculumService>();

    curricula
        .Setup(x => x.GetAllAsync())
        .ReturnsAsync(new List<CurriculumDto>());

    var controller = CreateControllerWithCurricula(
        db,
        curricula.Object);

    var result = await controller.Curricula();

    var viewResult = Assert.IsType<ViewResult>(result);

    Assert.Equal("Curricula", viewResult.ViewName);

    curricula.Verify(
        x => x.GetAllAsync(),
        Times.Once);
}

[Fact]
public async Task EditCurriculum_Get_ReturnsNotFound_WhenCurriculumMissing()
{
    var db = CreateDb();

    var controller = CreateController(db);

    var result = await controller.EditCurriculum(999);

    Assert.IsType<NotFoundResult>(result);
}

[Fact]
public async Task DeleteCurriculum_RedirectsToCurricula()
{
    var db = CreateDb();

    var curricula = new Mock<ICurriculumService>();

    curricula
        .Setup(x => x.DeleteAsync(1))
        .Returns(Task.CompletedTask);

    var controller = CreateControllerWithCurricula(
        db,
        curricula.Object);

    var result = await controller.DeleteCurriculum(1);

    var redirect = Assert.IsType<RedirectToActionResult>(result);

    Assert.Equal(nameof(AdminUiController.Curricula), redirect.ActionName);

    curricula.Verify(
        x => x.DeleteAsync(1),
        Times.Once);
}
}