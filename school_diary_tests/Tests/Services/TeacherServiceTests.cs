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


public class TeacherServiceTests
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
            cfg.CreateMap<Teacher, TeacherDto>()
                .ConstructUsing(t => new TeacherDto(
                    t.Id,
                    "Teacher " + t.Id,
                    t.UserId,
                    t.SchoolId,
                    t.TeacherSubjects.Select(ts => ts.SubjectId).ToList(),
                    new List<int>()
                ));

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

    private static TeacherService CreateService(
        ApplicationDbContext ctx,
        Mock<UserManager<User>> userManager)
    {
        return new TeacherService(ctx, CreateMapper(), userManager.Object);
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
    public async Task CreateAsync_ShouldCreateTeacher_WhenDataIsValid()
    {
        var ctx = CreateContext();
        var userManager = CreateUserManagerMock();
        var service = CreateService(ctx, userManager);

        ctx.Schools.Add(CreateSchool(1));
        ctx.Subjects.Add(new Subject { Id = 1, Name = "Math" });
        ctx.Classes.Add(new Class { Id = 1, Name = "5A", SchoolId = 1 });

        userManager
            .Setup(x => x.FindByEmailAsync("teacher@test.com"))
            .ReturnsAsync((User?)null);

        userManager
            .Setup(x => x.CreateAsync(It.IsAny<User>(), "Password123!"))
            .Callback<User, string>((user, password) =>
            {
                user.Id = "teacher-user-id";
            })
            .ReturnsAsync(IdentityResult.Success);

        userManager
            .Setup(x => x.IsInRoleAsync(It.IsAny<User>(), "Teacher"))
            .ReturnsAsync(false);

        userManager
            .Setup(x => x.AddToRoleAsync(It.IsAny<User>(), "Teacher"))
            .ReturnsAsync(IdentityResult.Success);

        await ctx.SaveChangesAsync();

        var dto = new CreateTeacherDto(
            "Ivan",
            "Petrov",
            "teacher@test.com",
            "Password123!",
            1,
            new List<int> { 1 },
            new List<int> { 1 }
        );

        var result = await service.CreateAsync(dto);

        Assert.NotNull(result);
        Assert.Single(ctx.Teachers);
        Assert.Single(ctx.TeacherSubjects);

        var teacher = await ctx.Teachers
            .Include(t => t.TeacherSubjects)
            .FirstAsync();

        Assert.Equal("teacher-user-id", teacher.UserId);
        Assert.Equal(1, teacher.SchoolId);
        Assert.Equal("1", teacher.AssignedClasses);
        Assert.Equal(1, teacher.TeacherSubjects.First().SubjectId);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowArgumentNullException_WhenDtoIsNull()
    {
        var ctx = CreateContext();
        var userManager = CreateUserManagerMock();
        var service = CreateService(ctx, userManager);

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => service.CreateAsync(null!));
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowKeyNotFoundException_WhenSchoolDoesNotExist()
    {
        var ctx = CreateContext();
        var userManager = CreateUserManagerMock();
        var service = CreateService(ctx, userManager);

        var dto = new CreateTeacherDto(
            "Ivan",
            "Petrov",
            "teacher@test.com",
            "Password123!",
            999,
            new List<int>(),
            new List<int>()
        );

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.CreateAsync(dto));

        Assert.Equal("School not found.", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowKeyNotFoundException_WhenSubjectDoesNotExist()
    {
        var ctx = CreateContext();
        var userManager = CreateUserManagerMock();
        var service = CreateService(ctx, userManager);

        ctx.Schools.Add(CreateSchool(1));
        await ctx.SaveChangesAsync();

        var dto = new CreateTeacherDto(
            "Ivan",
            "Petrov",
            "teacher@test.com",
            "Password123!",
            1,
            new List<int> { 999 },
            new List<int>()
        );

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.CreateAsync(dto));

        Assert.Equal("One or more subjects not found.", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowKeyNotFoundException_WhenClassDoesNotExist()
    {
        var ctx = CreateContext();
        var userManager = CreateUserManagerMock();
        var service = CreateService(ctx, userManager);

        ctx.Schools.Add(CreateSchool(1));
        await ctx.SaveChangesAsync();

        var dto = new CreateTeacherDto(
            "Ivan",
            "Petrov",
            "teacher@test.com",
            "Password123!",
            1,
            new List<int>(),
            new List<int> { 999 }
        );

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.CreateAsync(dto));

        Assert.Equal("One or more classes not found.", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowInvalidOperationException_WhenUserIsAlreadyTeacher()
    {
        var ctx = CreateContext();
        var userManager = CreateUserManagerMock();
        var service = CreateService(ctx, userManager);

        ctx.Schools.Add(CreateSchool(1));

        ctx.Teachers.Add(new Teacher
        {
            Id = 1,
            UserId = "existing-user-id",
            SchoolId = 1
        });

        var existingUser = new User
        {
            Id = "existing-user-id",
            FirstName = "Ivan",
            LastName = "Petrov",
            Email = "teacher@test.com",
            UserName = "teacher@test.com",
            Role = Role.Teacher
        };

        userManager
            .Setup(x => x.FindByEmailAsync("teacher@test.com"))
            .ReturnsAsync(existingUser);

        await ctx.SaveChangesAsync();

        var dto = new CreateTeacherDto(
            "Ivan",
            "Petrov",
            "teacher@test.com",
            "Password123!",
            1,
            new List<int>(),
            new List<int>()
        );

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateAsync(dto));

        Assert.Equal("User is already a teacher.", exception.Message);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllTeachers()
    {
        var ctx = CreateContext();
        var userManager = CreateUserManagerMock();
        var service = CreateService(ctx, userManager);

        ctx.Teachers.Add(new Teacher
        {
            Id = 1,
            UserId = "teacher-1",
            SchoolId = 1
        });

        ctx.Teachers.Add(new Teacher
        {
            Id = 2,
            UserId = "teacher-2",
            SchoolId = 1
        });

        await ctx.SaveChangesAsync();

        var result = await service.GetAllAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnTeacher_WhenTeacherExists()
    {
        var ctx = CreateContext();
        var userManager = CreateUserManagerMock();
        var service = CreateService(ctx, userManager);

        ctx.Teachers.Add(new Teacher
        {
            Id = 1,
            UserId = "teacher-user-id",
            SchoolId = 1
        });

        await ctx.SaveChangesAsync();

        var result = await service.GetAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result!.Id);
        Assert.Equal(1, result.SchoolId);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnNull_WhenTeacherDoesNotExist()
    {
        var ctx = CreateContext();
        var userManager = CreateUserManagerMock();
        var service = CreateService(ctx, userManager);

        var result = await service.GetAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnTeacher_WhenUserIdExists()
    {
        var ctx = CreateContext();
        var userManager = CreateUserManagerMock();
        var service = CreateService(ctx, userManager);

        ctx.Teachers.Add(new Teacher
        {
            Id = 1,
            UserId = "teacher-user-id",
            SchoolId = 1
        });

        await ctx.SaveChangesAsync();

        var result = await service.GetByUserIdAsync("teacher-user-id");

        Assert.NotNull(result);
        Assert.Equal(1, result!.Id);
    }

    [Fact]
    public async Task GetSubjectIdsForTeacherAsync_ShouldReturnTeacherSubjectIds()
    {
        var ctx = CreateContext();
        var userManager = CreateUserManagerMock();
        var service = CreateService(ctx, userManager);

        ctx.TeacherSubjects.Add(new TeacherSubject
        {
            TeacherId = 1,
            SubjectId = 10
        });

        ctx.TeacherSubjects.Add(new TeacherSubject
        {
            TeacherId = 1,
            SubjectId = 20
        });

        ctx.TeacherSubjects.Add(new TeacherSubject
        {
            TeacherId = 2,
            SubjectId = 30
        });

        await ctx.SaveChangesAsync();

        var result = await service.GetSubjectIdsForTeacherAsync(1);

        Assert.Equal(2, result.Count);
        Assert.Contains(10, result);
        Assert.Contains(20, result);
        Assert.DoesNotContain(30, result);
    }

    [Fact]
    public async Task GetMyStudentsAsync_ShouldReturnStudentsFromAssignedClasses()
    {
        var ctx = CreateContext();
        var userManager = CreateUserManagerMock();
        var service = CreateService(ctx, userManager);

        var classA = new Class
        {
            Id = 1,
            Name = "5A",
            SchoolId = 1
        };

        var classB = new Class
        {
            Id = 2,
            Name = "6B",
            SchoolId = 1
        };

        var user1 = new User
        {
            Id = "student-user-1",
            FirstName = "Ivan",
            LastName = "Petrov",
            Email = "student1@test.com",
            UserName = "student1@test.com",
            Role = Role.Student
        };

        var user2 = new User
        {
            Id = "student-user-2",
            FirstName = "Maria",
            LastName = "Ivanova",
            Email = "student2@test.com",
            UserName = "student2@test.com",
            Role = Role.Student
        };

        ctx.Classes.Add(classA);
        ctx.Classes.Add(classB);

        ctx.Users.Add(user1);
        ctx.Users.Add(user2);

        ctx.Teachers.Add(new Teacher
        {
            Id = 1,
            UserId = "teacher-user-id",
            SchoolId = 1,
            AssignedClasses = "1"
        });

        ctx.Students.Add(new Student
        {
            Id = 1,
            UserId = "student-user-1",
            User = user1,
            SchoolId = 1,
            ClassId = 1,
            Class = classA
        });

        ctx.Students.Add(new Student
        {
            Id = 2,
            UserId = "student-user-2",
            User = user2,
            SchoolId = 1,
            ClassId = 2,
            Class = classB
        });

        await ctx.SaveChangesAsync();

        var result = await service.GetMyStudentsAsync(1);

        Assert.Single(result);
        Assert.Equal(1, result.First().Id);
    }

    [Fact]
    public async Task GetMyStudentsAsync_ShouldReturnEmptyList_WhenTeacherDoesNotExist()
    {
        var ctx = CreateContext();
        var userManager = CreateUserManagerMock();
        var service = CreateService(ctx, userManager);

        var result = await service.GetMyStudentsAsync(999);

        Assert.Empty(result);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateTeacher()
    {
        var ctx = CreateContext();
        var userManager = CreateUserManagerMock();
        var service = CreateService(ctx, userManager);

        ctx.Schools.Add(CreateSchool(2));
        ctx.Subjects.Add(new Subject { Id = 2, Name = "History" });
        ctx.Classes.Add(new Class { Id = 2, Name = "6B", SchoolId = 2 });

        ctx.Teachers.Add(new Teacher
        {
            Id = 1,
            UserId = "teacher-user-id",
            SchoolId = 1,
            AssignedClasses = "1",
            TeacherSubjects = new List<TeacherSubject>
            {
                new TeacherSubject
                {
                    TeacherId = 1,
                    SubjectId = 1
                }
            }
        });

        var user = new User
        {
            Id = "teacher-user-id",
            FirstName = "Old",
            LastName = "Teacher",
            Email = "old@test.com",
            UserName = "old@test.com",
            Role = Role.Teacher
        };

        userManager
            .Setup(x => x.FindByIdAsync("teacher-user-id"))
            .ReturnsAsync(user);

        userManager
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);

        await ctx.SaveChangesAsync();

        var dto = new UpdateTeacherDto(
            "New",
            "Teacher",
            "new@test.com",
            2,
            new List<int> { 2 },
            new List<int> { 2 },
            null
        );

        var result = await service.UpdateAsync(1, dto);

        var teacher = await ctx.Teachers
            .Include(t => t.TeacherSubjects)
            .FirstAsync(t => t.Id == 1);

        Assert.NotNull(result);
        Assert.Equal(2, teacher.SchoolId);
        Assert.Equal("2", teacher.AssignedClasses);
        Assert.Single(teacher.TeacherSubjects);
        Assert.Equal(2, teacher.TeacherSubjects.First().SubjectId);

        Assert.Equal("New", user.FirstName);
        Assert.Equal("Teacher", user.LastName);
        Assert.Equal("new@test.com", user.Email);
    }

    [Fact]
    public async Task UpdateAsync_ShouldResetPassword_WhenNewPasswordIsProvided()
    {
        var ctx = CreateContext();
        var userManager = CreateUserManagerMock();
        var service = CreateService(ctx, userManager);

        ctx.Teachers.Add(new Teacher
        {
            Id = 1,
            UserId = "teacher-user-id",
            SchoolId = 1
        });

        var user = new User
        {
            Id = "teacher-user-id",
            FirstName = "Old",
            LastName = "Teacher",
            Email = "old@test.com",
            UserName = "old@test.com",
            Role = Role.Teacher
        };

        userManager
            .Setup(x => x.FindByIdAsync("teacher-user-id"))
            .ReturnsAsync(user);

        userManager
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);

        userManager
            .Setup(x => x.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync("reset-token");

        userManager
            .Setup(x => x.ResetPasswordAsync(user, "reset-token", "NewPassword123!"))
            .ReturnsAsync(IdentityResult.Success);

        await ctx.SaveChangesAsync();

        var dto = new UpdateTeacherDto(
            "New",
            "Teacher",
            "new@test.com",
            1,
            new List<int>(),
            new List<int>(),
            "NewPassword123!"
        );

        await service.UpdateAsync(1, dto);

        userManager.Verify(
            x => x.ResetPasswordAsync(user, "reset-token", "NewPassword123!"),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowKeyNotFoundException_WhenTeacherDoesNotExist()
    {
        var ctx = CreateContext();
        var userManager = CreateUserManagerMock();
        var service = CreateService(ctx, userManager);

        var dto = new UpdateTeacherDto(
            "New",
            "Teacher",
            "new@test.com",
            1,
            new List<int>(),
            new List<int>(),
            null
        );

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.UpdateAsync(999, dto));

        Assert.Equal("Teacher not found.", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowKeyNotFoundException_WhenTeacherUserDoesNotExist()
    {
        var ctx = CreateContext();
        var userManager = CreateUserManagerMock();
        var service = CreateService(ctx, userManager);

        ctx.Teachers.Add(new Teacher
        {
            Id = 1,
            UserId = "missing-user-id",
            SchoolId = 1
        });

        userManager
            .Setup(x => x.FindByIdAsync("missing-user-id"))
            .ReturnsAsync((User?)null);

        await ctx.SaveChangesAsync();

        var dto = new UpdateTeacherDto(
            "New",
            "Teacher",
            "new@test.com",
            1,
            new List<int>(),
            new List<int>(),
            null
        );

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.UpdateAsync(1, dto));

        Assert.Equal("Teacher user not found.", exception.Message);
    }

    [Fact]
    public async Task AddSubjectsAsync_ShouldReplaceTeacherSubjects()
    {
        var ctx = CreateContext();
        var userManager = CreateUserManagerMock();
        var service = CreateService(ctx, userManager);

        ctx.Teachers.Add(new Teacher
        {
            Id = 1,
            UserId = "teacher-user-id",
            SchoolId = 1,
            TeacherSubjects = new List<TeacherSubject>
            {
                new TeacherSubject
                {
                    TeacherId = 1,
                    SubjectId = 1
                }
            }
        });

        await ctx.SaveChangesAsync();

        var result = await service.AddSubjectsAsync(1, new List<int> { 2, 3, 3 });

        var teacher = await ctx.Teachers
            .Include(t => t.TeacherSubjects)
            .FirstAsync(t => t.Id == 1);

        Assert.NotNull(result);
        Assert.Equal(2, teacher.TeacherSubjects.Count);
        Assert.Contains(teacher.TeacherSubjects, ts => ts.SubjectId == 2);
        Assert.Contains(teacher.TeacherSubjects, ts => ts.SubjectId == 3);
        Assert.DoesNotContain(teacher.TeacherSubjects, ts => ts.SubjectId == 1);
    }

    [Fact]
    public async Task AddSubjectsAsync_ShouldThrowKeyNotFoundException_WhenTeacherDoesNotExist()
    {
        var ctx = CreateContext();
        var userManager = CreateUserManagerMock();
        var service = CreateService(ctx, userManager);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.AddSubjectsAsync(999, new List<int> { 1 }));

        Assert.Equal("Teacher not found.", exception.Message);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteTeacherAndDetachGrades()
    {
        var ctx = CreateContext();
        var userManager = CreateUserManagerMock();
        var service = CreateService(ctx, userManager);

        ctx.Teachers.Add(new Teacher
        {
            Id = 1,
            UserId = "teacher-user-id",
            SchoolId = 1,
            TeacherSubjects = new List<TeacherSubject>
            {
                new TeacherSubject
                {
                    TeacherId = 1,
                    SubjectId = 1
                }
            }
        });

        ctx.Grades.Add(new Grade
        {
            Id = 1,
            Value = 6,
            StudentId = 1,
            SubjectId = 1,
            TeacherId = 1
        });

        ctx.CurriculumEntries.Add(new CurriculumEntry
        {
            Id = 1,
            TeacherId = 1,
            SubjectId = 1,
            DayOfWeek = "Monday",
            Period = 1
        });

        var user = new User
        {
            Id = "teacher-user-id",
            Email = "teacher@test.com",
            UserName = "teacher@test.com",
            FirstName = "Ivan",
            LastName = "Petrov",
            Role = Role.Teacher
        };

        userManager
            .Setup(x => x.FindByIdAsync("teacher-user-id"))
            .ReturnsAsync(user);

        userManager
            .Setup(x => x.DeleteAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        await ctx.SaveChangesAsync();

        await service.DeleteAsync(1);

        Assert.Empty(ctx.Teachers);
        Assert.Empty(ctx.TeacherSubjects);
        Assert.Empty(ctx.CurriculumEntries);

        var grade = await ctx.Grades.FirstAsync();
        Assert.Null(grade.TeacherId);

        userManager.Verify(x => x.DeleteAsync(user), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowKeyNotFoundException_WhenTeacherDoesNotExist()
    {
        var ctx = CreateContext();
        var userManager = CreateUserManagerMock();
        var service = CreateService(ctx, userManager);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.DeleteAsync(999));

        Assert.Equal("Teacher not found.", exception.Message);
    }
}