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


public class ParentServiceTests
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
            cfg.CreateMap<Parent, ParentDto>()
                .ConstructUsing(p => new ParentDto(
                    p.Id,
                    p.User.FirstName + " " + p.User.LastName,
                    p.User.Email ?? ""
                ));

            cfg.CreateMap<Student, StudentDto>();
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

    private static ParentService CreateService(
        ApplicationDbContext ctx,
        Mock<UserManager<User>> userManagerMock)
    {
        return new ParentService(ctx, CreateMapper(), userManagerMock.Object);
    }

    private static User CreateUser(string id, string email, Role role = Role.Parent)
    {
        return new User
        {
            Id = id,
            UserName = email,
            Email = email,
            FirstName = "Ivan",
            LastName = "Petrov",
            Role = role,
            EmailConfirmed = true
        };
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllParents()
    {
        var ctx = CreateContext();
        var userManager = CreateUserManagerMock();
        var service = CreateService(ctx, userManager);

        var user1 = CreateUser("user-1", "parent1@test.com");
        var user2 = CreateUser("user-2", "parent2@test.com");

        ctx.Users.Add(user1);
        ctx.Users.Add(user2);

        ctx.Parents.Add(new Parent
        {
            Id = 1,
            UserId = "user-1",
            User = user1
        });

        ctx.Parents.Add(new Parent
        {
            Id = 2,
            UserId = "user-2",
            User = user2
        });

        await ctx.SaveChangesAsync();

        var result = await service.GetAllAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnParent_WhenParentExists()
    {
        var ctx = CreateContext();
        var userManager = CreateUserManagerMock();
        var service = CreateService(ctx, userManager);

        var user = CreateUser("user-1", "parent@test.com");

        ctx.Users.Add(user);

        ctx.Parents.Add(new Parent
        {
            Id = 1,
            UserId = "user-1",
            User = user
        });

        await ctx.SaveChangesAsync();

        var result = await service.GetAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result!.Id);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnNull_WhenParentDoesNotExist()
    {
        var ctx = CreateContext();
        var userManager = CreateUserManagerMock();
        var service = CreateService(ctx, userManager);

        var result = await service.GetAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task AddChildAsync_ShouldAddStudentToParent_WhenDataIsValid()
    {
        var ctx = CreateContext();
        var userManager = CreateUserManagerMock();
        var service = CreateService(ctx, userManager);

        ctx.Parents.Add(new Parent
        {
            Id = 1,
            UserId = "parent-user-id"
        });

        ctx.Students.Add(new Student
        {
            Id = 1,
            UserId = "student-user-id",
            SchoolId = 1
        });

        await ctx.SaveChangesAsync();

        await service.AddChildAsync(1, 1);

        Assert.Single(ctx.ParentStudents);
        Assert.Equal(1, ctx.ParentStudents.First().ParentId);
        Assert.Equal(1, ctx.ParentStudents.First().StudentId);
    }

    [Fact]
    public async Task AddChildAsync_ShouldThrowKeyNotFoundException_WhenParentDoesNotExist()
    {
        var ctx = CreateContext();
        var userManager = CreateUserManagerMock();
        var service = CreateService(ctx, userManager);

        ctx.Students.Add(new Student
        {
            Id = 1,
            UserId = "student-user-id",
            SchoolId = 1
        });

        await ctx.SaveChangesAsync();

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.AddChildAsync(999, 1));

        Assert.Equal("Parent not found", exception.Message);
    }

    [Fact]
    public async Task AddChildAsync_ShouldThrowKeyNotFoundException_WhenStudentDoesNotExist()
    {
        var ctx = CreateContext();
        var userManager = CreateUserManagerMock();
        var service = CreateService(ctx, userManager);

        ctx.Parents.Add(new Parent
        {
            Id = 1,
            UserId = "parent-user-id"
        });

        await ctx.SaveChangesAsync();

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.AddChildAsync(1, 999));

        Assert.Equal("Student not found", exception.Message);
    }

    [Fact]
    public async Task AddChildAsync_ShouldNotDuplicateExistingParentStudentLink()
    {
        var ctx = CreateContext();
        var userManager = CreateUserManagerMock();
        var service = CreateService(ctx, userManager);

        ctx.Parents.Add(new Parent
        {
            Id = 1,
            UserId = "parent-user-id"
        });

        ctx.Students.Add(new Student
        {
            Id = 1,
            UserId = "student-user-id",
            SchoolId = 1
        });

        ctx.ParentStudents.Add(new ParentStudent
        {
            ParentId = 1,
            StudentId = 1
        });

        await ctx.SaveChangesAsync();

        await service.AddChildAsync(1, 1);

        Assert.Single(ctx.ParentStudents);
    }

    [Fact]
    public async Task AddChildAsync_ShouldThrowInvalidOperationException_WhenStudentAlreadyHasTwoParents()
    {
        var ctx = CreateContext();
        var userManager = CreateUserManagerMock();
        var service = CreateService(ctx, userManager);

        ctx.Parents.Add(new Parent { Id = 1, UserId = "parent-1" });
        ctx.Parents.Add(new Parent { Id = 2, UserId = "parent-2" });
        ctx.Parents.Add(new Parent { Id = 3, UserId = "parent-3" });

        ctx.Students.Add(new Student
        {
            Id = 1,
            UserId = "student-user-id",
            SchoolId = 1
        });

        ctx.ParentStudents.Add(new ParentStudent { ParentId = 1, StudentId = 1 });
        ctx.ParentStudents.Add(new ParentStudent { ParentId = 2, StudentId = 1 });

        await ctx.SaveChangesAsync();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.AddChildAsync(3, 1));

        Assert.Equal("This student already has two parents.", exception.Message);
    }

    [Fact]
    public async Task RemoveChildAsync_ShouldRemoveExistingParentStudentLink()
    {
        var ctx = CreateContext();
        var userManager = CreateUserManagerMock();
        var service = CreateService(ctx, userManager);

        ctx.ParentStudents.Add(new ParentStudent
        {
            ParentId = 1,
            StudentId = 1
        });

        await ctx.SaveChangesAsync();

        await service.RemoveChildAsync(1, 1);

        Assert.Empty(ctx.ParentStudents);
    }

    [Fact]
    public async Task RemoveChildAsync_ShouldDoNothing_WhenLinkDoesNotExist()
    {
        var ctx = CreateContext();
        var userManager = CreateUserManagerMock();
        var service = CreateService(ctx, userManager);

        await service.RemoveChildAsync(1, 1);

        Assert.Empty(ctx.ParentStudents);
    }

    [Fact]
    public async Task AssignStudentsAsync_ShouldReplaceParentStudents()
    {
        var ctx = CreateContext();
        var userManager = CreateUserManagerMock();
        var service = CreateService(ctx, userManager);

        ctx.Parents.Add(new Parent
        {
            Id = 1,
            UserId = "parent-user-id"
        });

        ctx.Students.Add(new Student
        {
            Id = 1,
            UserId = "student-1",
            SchoolId = 1
        });

        ctx.Students.Add(new Student
        {
            Id = 2,
            UserId = "student-2",
            SchoolId = 1
        });

        ctx.ParentStudents.Add(new ParentStudent
        {
            ParentId = 1,
            StudentId = 1
        });

        await ctx.SaveChangesAsync();

        await service.AssignStudentsAsync(1, new List<int> { 2 });

        Assert.Single(ctx.ParentStudents);
        Assert.Equal(2, ctx.ParentStudents.First().StudentId);
    }

    [Fact]
    public async Task AssignStudentsAsync_ShouldThrowKeyNotFoundException_WhenParentDoesNotExist()
    {
        var ctx = CreateContext();
        var userManager = CreateUserManagerMock();
        var service = CreateService(ctx, userManager);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.AssignStudentsAsync(999, new List<int> { 1 }));

        Assert.Equal("Parent not found", exception.Message);
    }

    [Fact]
    public async Task AssignStudentsAsync_ShouldThrowKeyNotFoundException_WhenSomeStudentsDoNotExist()
    {
        var ctx = CreateContext();
        var userManager = CreateUserManagerMock();
        var service = CreateService(ctx, userManager);

        ctx.Parents.Add(new Parent
        {
            Id = 1,
            UserId = "parent-user-id"
        });

        ctx.Students.Add(new Student
        {
            Id = 1,
            UserId = "student-1",
            SchoolId = 1
        });

        await ctx.SaveChangesAsync();

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.AssignStudentsAsync(1, new List<int> { 1, 999 }));

        Assert.Equal("One or more students not found", exception.Message);
    }

    [Fact]
    public async Task GetStudentIdsForParentAsync_ShouldReturnStudentIds()
    {
        var ctx = CreateContext();
        var userManager = CreateUserManagerMock();
        var service = CreateService(ctx, userManager);

        ctx.ParentStudents.Add(new ParentStudent
        {
            ParentId = 1,
            StudentId = 10
        });

        ctx.ParentStudents.Add(new ParentStudent
        {
            ParentId = 1,
            StudentId = 20
        });

        await ctx.SaveChangesAsync();

        var result = await service.GetStudentIdsForParentAsync(1);

        Assert.Equal(2, result.Count);
        Assert.Contains(10, result);
        Assert.Contains(20, result);
    }

    [Fact]
    public async Task GetStudentNamesForParentAsync_ShouldReturnStudentFullNames()
    {
        var ctx = CreateContext();
        var userManager = CreateUserManagerMock();
        var service = CreateService(ctx, userManager);

        var studentUser = new User
        {
            Id = "student-user-id",
            FirstName = "Maria",
            LastName = "Ivanova",
            Email = "student@test.com",
            UserName = "student@test.com",
            Role = Role.Student
        };

        var student = new Student
        {
            Id = 1,
            UserId = "student-user-id",
            User = studentUser,
            SchoolId = 1
        };

        ctx.Users.Add(studentUser);
        ctx.Students.Add(student);

        ctx.ParentStudents.Add(new ParentStudent
        {
            ParentId = 1,
            StudentId = 1,
            Student = student
        });

        await ctx.SaveChangesAsync();

        var result = await service.GetStudentNamesForParentAsync(1);

        Assert.Single(result);
        Assert.Equal("Maria Ivanova", result.First());
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteParentAndUser()
    {
        var ctx = CreateContext();
        var userManager = CreateUserManagerMock();
        var service = CreateService(ctx, userManager);

        var user = CreateUser("parent-user-id", "parent@test.com");

        ctx.Users.Add(user);

        ctx.Parents.Add(new Parent
        {
            Id = 1,
            UserId = "parent-user-id",
            User = user,
            ParentStudents = new List<ParentStudent>
            {
                new ParentStudent
                {
                    ParentId = 1,
                    StudentId = 1
                }
            }
        });

        userManager
            .Setup(x => x.DeleteAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);

        await ctx.SaveChangesAsync();

        await service.DeleteAsync(1);

        Assert.Empty(ctx.Parents);
        Assert.Empty(ctx.ParentStudents);

        userManager.Verify(x => x.DeleteAsync(It.Is<User>(u => u.Id == "parent-user-id")), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowKeyNotFoundException_WhenParentDoesNotExist()
    {
        var ctx = CreateContext();
        var userManager = CreateUserManagerMock();
        var service = CreateService(ctx, userManager);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.DeleteAsync(999));

        Assert.Equal("Parent not found", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateParentUser()
    {
        var ctx = CreateContext();
        var userManager = CreateUserManagerMock();
        var service = CreateService(ctx, userManager);

        var user = CreateUser("parent-user-id", "old@test.com");

        ctx.Users.Add(user);

        ctx.Parents.Add(new Parent
        {
            Id = 1,
            UserId = "parent-user-id",
            User = user
        });

        userManager
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);

        await ctx.SaveChangesAsync();

        var dto = new UpdateParentDto(
            "NewFirst",
            "NewLast",
            "new@test.com"
        );

        await service.UpdateAsync(1, dto);

        var parent = await ctx.Parents
            .Include(p => p.User)
            .FirstAsync(p => p.Id == 1);

        Assert.Equal("NewFirst", parent.User.FirstName);
        Assert.Equal("NewLast", parent.User.LastName);
        Assert.Equal("new@test.com", parent.User.Email);
        Assert.Equal("new@test.com", parent.User.UserName);
        Assert.Equal(Role.Parent, parent.User.Role);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowKeyNotFoundException_WhenParentDoesNotExist()
    {
        var ctx = CreateContext();
        var userManager = CreateUserManagerMock();
        var service = CreateService(ctx, userManager);

        var dto = new UpdateParentDto(
            "First",
            "Last",
            "test@test.com"
        );

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.UpdateAsync(999, dto));

        Assert.Equal("Parent not found", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateParentAndAssignStudents_WhenDataIsValid()
    {
        var ctx = CreateContext();
        var userManager = CreateUserManagerMock();
        var service = CreateService(ctx, userManager);

        ctx.Students.Add(new Student
        {
            Id = 1,
            UserId = "student-user-id",
            SchoolId = 1
        });

        userManager
            .Setup(x => x.FindByEmailAsync("parent@test.com"))
            .ReturnsAsync((User?)null);

        userManager
            .Setup(x => x.CreateAsync(It.IsAny<User>(), "Password123!"))
            .Callback<User, string>((user, password) =>
            {
                user.Id = "new-parent-user-id";
            })
            .ReturnsAsync(IdentityResult.Success);

        userManager
            .Setup(x => x.AddToRoleAsync(It.IsAny<User>(), "Parent"))
            .ReturnsAsync(IdentityResult.Success);

        await ctx.SaveChangesAsync();

        var dto = new CreateParentDto(
            "Ivan",
            "Petrov",
            "parent@test.com",
            "Password123!",
            new List<int> { 1 }
        );

        var result = await service.CreateAsync(dto);

        Assert.NotNull(result);
        Assert.Equal("Ivan Petrov", result.FullName);
        Assert.Equal("parent@test.com", result.Email);
        Assert.Single(ctx.Parents);
        Assert.Single(ctx.ParentStudents);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowInvalidOperationException_WhenEmailAlreadyExists()
    {
        var ctx = CreateContext();
        var userManager = CreateUserManagerMock();
        var service = CreateService(ctx, userManager);

        userManager
            .Setup(x => x.FindByEmailAsync("parent@test.com"))
            .ReturnsAsync(CreateUser("existing-user-id", "parent@test.com"));

        var dto = new CreateParentDto(
            "Ivan",
            "Petrov",
            "parent@test.com",
            "Password123!",
            new List<int>()
        );

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateAsync(dto));

        Assert.Equal("A user with this email already exists.", exception.Message);
    }
}