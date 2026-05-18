namespace school_diary.school_diary_tests.Tests;

using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using school_diary.Data;
using school_diary.Dtos;
using school_diary.Models;
using school_diary.Services;
using Xunit;


public class StudentServiceTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static IMapper CreateMapper()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Student, StudentDto>()
                .ConstructUsing(s => new StudentDto(
                    s.Id,
                    s.User.FirstName + " " + s.User.LastName,
                    s.User.Email ?? "",
                    s.Class != null ? s.Class.Name : "Unassigned",
                    s.SchoolId
                ));
        });

        return config.CreateMapper();
    }

    private static Mock<UserManager<User>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<User>>();

        return new Mock<UserManager<User>>(
            store.Object,
            Mock.Of<IOptions<IdentityOptions>>(),
            Mock.Of<IPasswordHasher<User>>(),
            Array.Empty<IUserValidator<User>>(),
            Array.Empty<IPasswordValidator<User>>(),
            Mock.Of<ILookupNormalizer>(),
            new IdentityErrorDescriber(),
            Mock.Of<IServiceProvider>(),
            Mock.Of<ILogger<UserManager<User>>>()
        );
    }

    private static Mock<RoleManager<IdentityRole>> CreateRoleManagerMock()
    {
        var store = new Mock<IRoleStore<IdentityRole>>();

        return new Mock<RoleManager<IdentityRole>>(
            store.Object,
            Array.Empty<IRoleValidator<IdentityRole>>(),
            Mock.Of<ILookupNormalizer>(),
            new IdentityErrorDescriber(),
            Mock.Of<ILogger<RoleManager<IdentityRole>>>()
        );
    }

    private static StudentService CreateService(
        ApplicationDbContext ctx,
        Mock<UserManager<User>> userManager,
        Mock<RoleManager<IdentityRole>> roleManager)
    {
        return new StudentService(
            ctx,
            CreateMapper(),
            userManager.Object,
            roleManager.Object);
    }

    private static School CreateSchool(int id)
    {
        return new School
        {
            Id = id,
            Name = "School " + id,
            Address = "Address " + id
        };
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateStudent_WhenDataIsValidAndClassDoesNotExist()
    {
        var ctx = CreateContext();
        var userManager = CreateUserManagerMock();
        var roleManager = CreateRoleManagerMock();
        var service = CreateService(ctx, userManager, roleManager);

        ctx.Schools.Add(CreateSchool(1));

        userManager
            .Setup(x => x.CreateAsync(It.IsAny<User>(), "Password123!"))
            .Callback<User, string>((user, password) =>
            {
                user.Id = "student-user-id";
            })
            .ReturnsAsync(IdentityResult.Success);

        roleManager
            .Setup(x => x.RoleExistsAsync("Student"))
            .ReturnsAsync(false);

        roleManager
            .Setup(x => x.CreateAsync(It.IsAny<IdentityRole>()))
            .ReturnsAsync(IdentityResult.Success);

        userManager
            .Setup(x => x.AddToRoleAsync(It.IsAny<User>(), "Student"))
            .ReturnsAsync(IdentityResult.Success);

        await ctx.SaveChangesAsync();

        var dto = new CreateStudentDto
        {
            FirstName = "Ivan",
            LastName = "Petrov",
            Email = "student@test.com",
            Password = "Password123!",
            SchoolId = 1,
            ClassName = "5A"
        };

        var result = await service.CreateAsync(dto);

        Assert.NotNull(result);
        Assert.Equal("Ivan Petrov", result.FullName);
        Assert.Equal("student@test.com", result.Email);
        Assert.Equal(1, result.SchoolId);
        Assert.Equal("5A", result.ClassName);
        Assert.Equal("Password123!", result.InitialPassword);

        Assert.Single(ctx.Students);
        Assert.Single(ctx.Classes);
    }

    [Fact]
    public async Task CreateAsync_ShouldUseExistingClass_WhenClassAlreadyExists()
    {
        var ctx = CreateContext();
        var userManager = CreateUserManagerMock();
        var roleManager = CreateRoleManagerMock();
        var service = CreateService(ctx, userManager, roleManager);

        ctx.Schools.Add(CreateSchool(1));

        ctx.Classes.Add(new Class
        {
            Id = 1,
            Name = "5A",
            SchoolId = 1
        });

        userManager
            .Setup(x => x.CreateAsync(It.IsAny<User>(), "Password123!"))
            .Callback<User, string>((user, password) =>
            {
                user.Id = "student-user-id";
            })
            .ReturnsAsync(IdentityResult.Success);

        roleManager
            .Setup(x => x.RoleExistsAsync("Student"))
            .ReturnsAsync(true);

        userManager
            .Setup(x => x.AddToRoleAsync(It.IsAny<User>(), "Student"))
            .ReturnsAsync(IdentityResult.Success);

        await ctx.SaveChangesAsync();

        var dto = new CreateStudentDto
        {
            FirstName = "Ivan",
            LastName = "Petrov",
            Email = "student@test.com",
            Password = "Password123!",
            SchoolId = 1,
            ClassName = "5A"
        };

        var result = await service.CreateAsync(dto);

        Assert.NotNull(result);
        Assert.Equal("5A", result.ClassName);
        Assert.Single(ctx.Classes);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllStudents()
    {
        var ctx = CreateContext();
        var userManager = CreateUserManagerMock();
        var roleManager = CreateRoleManagerMock();
        var service = CreateService(ctx, userManager, roleManager);

        var user = new User
        {
            Id = "student-user-id",
            FirstName = "Ivan",
            LastName = "Petrov",
            Email = "ivan@test.com",
            UserName = "ivan@test.com",
            Role = Role.Student
        };

        ctx.Users.Add(user);

        ctx.Students.Add(new Student
        {
            Id = 1,
            UserId = "student-user-id",
            User = user,
            SchoolId = 1
        });

        await ctx.SaveChangesAsync();

        var result = await service.GetAllAsync();

        Assert.Single(result);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnStudent_WhenStudentExists()
    {
        var ctx = CreateContext();
        var userManager = CreateUserManagerMock();
        var roleManager = CreateRoleManagerMock();
        var service = CreateService(ctx, userManager, roleManager);

        var user = new User
        {
            Id = "student-user-id",
            FirstName = "Ivan",
            LastName = "Petrov",
            Email = "ivan@test.com",
            UserName = "ivan@test.com",
            Role = Role.Student
        };

        ctx.Users.Add(user);

        ctx.Students.Add(new Student
        {
            Id = 1,
            UserId = "student-user-id",
            User = user,
            SchoolId = 1
        });

        await ctx.SaveChangesAsync();

        var result = await service.GetAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result!.Id);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnNull_WhenStudentDoesNotExist()
    {
        var ctx = CreateContext();
        var userManager = CreateUserManagerMock();
        var roleManager = CreateRoleManagerMock();
        var service = CreateService(ctx, userManager, roleManager);

        var result = await service.GetAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnStudent_WhenUserExists()
    {
        var ctx = CreateContext();
        var userManager = CreateUserManagerMock();
        var roleManager = CreateRoleManagerMock();
        var service = CreateService(ctx, userManager, roleManager);

        var user = new User
        {
            Id = "student-user-id",
            FirstName = "Ivan",
            LastName = "Petrov",
            Email = "ivan@test.com",
            UserName = "ivan@test.com",
            Role = Role.Student
        };

        ctx.Users.Add(user);

        ctx.Students.Add(new Student
        {
            Id = 1,
            UserId = "student-user-id",
            User = user,
            SchoolId = 1
        });

        await ctx.SaveChangesAsync();

        var result = await service.GetByUserIdAsync("student-user-id");

        Assert.NotNull(result);
        Assert.Equal(1, result!.Id);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateStudent()
    {
        var ctx = CreateContext();
        var userManager = CreateUserManagerMock();
        var roleManager = CreateRoleManagerMock();
        var service = CreateService(ctx, userManager, roleManager);

        ctx.Schools.Add(CreateSchool(2));

        var user = new User
        {
            Id = "student-user-id",
            FirstName = "Old",
            LastName = "Name",
            Email = "old@test.com",
            UserName = "old@test.com",
            Role = Role.Student
        };

        ctx.Users.Add(user);

        ctx.Students.Add(new Student
        {
            Id = 1,
            UserId = "student-user-id",
            User = user,
            SchoolId = 1
        });

        await ctx.SaveChangesAsync();

        var dto = new UpdateStudentDto
        {
            Id = 1,
            FirstName = "New",
            LastName = "Student",
            Email = "new@test.com",
            SchoolId = 2,
            ClassName = "6B"
        };

        await service.UpdateAsync(1, dto);

        var student = await ctx.Students
            .Include(s => s.User)
            .Include(s => s.Class)
            .FirstAsync(s => s.Id == 1);

        Assert.Equal(2, student.SchoolId);
        Assert.Equal("6B", student.Class!.Name);
        Assert.Equal("New", student.User.FirstName);
        Assert.Equal("Student", student.User.LastName);
        Assert.Equal("new@test.com", student.User.Email);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteStudent_WhenStudentExists()
    {
        var ctx = CreateContext();
        var userManager = CreateUserManagerMock();
        var roleManager = CreateRoleManagerMock();
        var service = CreateService(ctx, userManager, roleManager);

        ctx.Students.Add(new Student
        {
            Id = 1,
            UserId = "student-user-id",
            SchoolId = 1
        });

        await ctx.SaveChangesAsync();

        await service.DeleteAsync(1);

        Assert.Empty(ctx.Students);
    }

    [Fact]
    public async Task CreateRecordAsync_ShouldCreateStudent()
    {
        var ctx = CreateContext();
        var userManager = CreateUserManagerMock();
        var roleManager = CreateRoleManagerMock();
        var service = CreateService(ctx, userManager, roleManager);

        ctx.Schools.Add(CreateSchool(1));

        userManager
            .Setup(x => x.CreateAsync(It.IsAny<User>(), "Password123!"))
            .Callback<User, string>((user, password) =>
            {
                user.Id = "student-user-id";
            })
            .ReturnsAsync(IdentityResult.Success);

        roleManager
            .Setup(x => x.RoleExistsAsync("Student"))
            .ReturnsAsync(true);

        userManager
            .Setup(x => x.AddToRoleAsync(It.IsAny<User>(), "Student"))
            .ReturnsAsync(IdentityResult.Success);

        await ctx.SaveChangesAsync();

        var dto = new CreateStudentDto
        {
            FirstName = "Ivan",
            LastName = "Petrov",
            Email = "student@test.com",
            Password = "Password123!",
            SchoolId = 1,
            ClassName = "5A"
        };

        await service.CreateRecordAsync(dto);

        Assert.Single(ctx.Students);
    }
}