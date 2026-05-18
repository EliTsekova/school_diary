namespace school_diary.school_diary_tests.Tests;
using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using school_diary.Data;
using school_diary.Dtos;
using school_diary.Models;
using school_diary.Services;
using Xunit;


public class CurriculumServiceTests
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
            cfg.CreateMap<Curriculum, CurriculumDto>()
                .ConstructUsing(c => new CurriculumDto(
                    c.Id,
                    c.Term,
                    c.Class.SchoolId,
                    c.ClassId,
                    c.Class.Name,
                    c.Entries.Select(e => new CurriculumEntryDto(
                        e.Id,
                        e.SubjectId,
                        e.Subject.Name,
                        e.TeacherId,
                        "Teacher " + e.TeacherId,
                        e.DayOfWeek,
                        e.Period
                    )).ToList()
                ));

            cfg.CreateMap<CurriculumEntry, CurriculumEntryDto>()
                .ConstructUsing(e => new CurriculumEntryDto(
                    e.Id,
                    e.SubjectId,
                    e.Subject.Name,
                    e.TeacherId,
                    "Teacher " + e.TeacherId,
                    e.DayOfWeek,
                    e.Period
                ));
        });

        return config.CreateMapper();
    }

    private static CurriculumService CreateService(ApplicationDbContext ctx)
    {
        return new CurriculumService(ctx, CreateMapper());
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateCurriculum_WhenDataIsValid()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        ctx.Classes.Add(new Class
        {
            Id = 100,
            Name = "5A",
            SchoolId = 1
        });

        ctx.Subjects.Add(new Subject
        {
            Id = 100,
            Name = "Math"
        });

        ctx.Teachers.Add(new Teacher
        {
            Id = 100,
            UserId = "teacher-user-id",
            SchoolId = 1
        });

        ctx.TeacherSubjects.Add(new TeacherSubject
        {
            Id = 100,
            TeacherId = 100,
            SubjectId = 100
        });

        await ctx.SaveChangesAsync();

        var dto = new CreateCurriculumDto(
            "First Term",
            100,
            new List<CreateCurriculumEntryDto>
            {
                new CreateCurriculumEntryDto(
                    100,
                    100,
                    "Monday",
                    1
                )
            }
        );

        var result = await service.CreateAsync(dto);

        Assert.NotNull(result);
        Assert.Equal("First Term", result.Term);
        Assert.Equal(100, result.ClassId);
        Assert.Single(result.Entries);
        Assert.Equal(1, ctx.Curricula.Count());
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowValidationException_WhenEntriesAreEmpty()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        var dto = new CreateCurriculumDto(
            "First Term",
            100,
            new List<CreateCurriculumEntryDto>()
        );

        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => service.CreateAsync(dto));

        Assert.Equal("Curriculum entries are required.", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowKeyNotFoundException_WhenClassDoesNotExist()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        var dto = new CreateCurriculumDto(
            "First Term",
            999,
            new List<CreateCurriculumEntryDto>
            {
                new CreateCurriculumEntryDto(
                    100,
                    100,
                    "Monday",
                    1
                )
            }
        );

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.CreateAsync(dto));

        Assert.Equal("Class not found", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowValidationException_WhenThereAreDuplicateDayAndPeriodEntries()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        ctx.Classes.Add(new Class
        {
            Id = 100,
            Name = "5A",
            SchoolId = 1
        });

        await ctx.SaveChangesAsync();

        var dto = new CreateCurriculumDto(
            "First Term",
            100,
            new List<CreateCurriculumEntryDto>
            {
                new CreateCurriculumEntryDto(
                    100,
                    100,
                    "Monday",
                    1
                ),
                new CreateCurriculumEntryDto(
                    101,
                    101,
                    "Monday",
                    1
                )
            }
        );

        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => service.CreateAsync(dto));

        Assert.Equal("Only one subject is allowed for the same day and period.", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowValidationException_WhenSubjectDoesNotExist()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        ctx.Classes.Add(new Class
        {
            Id = 100,
            Name = "5A",
            SchoolId = 1
        });

        await ctx.SaveChangesAsync();

        var dto = new CreateCurriculumDto(
            "First Term",
            100,
            new List<CreateCurriculumEntryDto>
            {
                new CreateCurriculumEntryDto(
                    999,
                    100,
                    "Monday",
                    1
                )
            }
        );

        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => service.CreateAsync(dto));

        Assert.Equal("One or more subjects do not exist.", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowValidationException_WhenTeacherDoesNotExist()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        ctx.Classes.Add(new Class
        {
            Id = 100,
            Name = "5A",
            SchoolId = 1
        });

        ctx.Subjects.Add(new Subject
        {
            Id = 100,
            Name = "Math"
        });

        await ctx.SaveChangesAsync();

        var dto = new CreateCurriculumDto(
            "First Term",
            100,
            new List<CreateCurriculumEntryDto>
            {
                new CreateCurriculumEntryDto(
                    100,
                    999,
                    "Monday",
                    1
                )
            }
        );

        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => service.CreateAsync(dto));

        Assert.Equal("One or more teachers do not exist.", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowValidationException_WhenTeacherIsFromDifferentSchool()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        ctx.Classes.Add(new Class
        {
            Id = 100,
            Name = "5A",
            SchoolId = 1
        });

        ctx.Subjects.Add(new Subject
        {
            Id = 100,
            Name = "Math"
        });

        ctx.Teachers.Add(new Teacher
        {
            Id = 100,
            UserId = "teacher-user-id",
            SchoolId = 2
        });

        await ctx.SaveChangesAsync();

        var dto = new CreateCurriculumDto(
            "First Term",
            100,
            new List<CreateCurriculumEntryDto>
            {
                new CreateCurriculumEntryDto(
                    100,
                    100,
                    "Monday",
                    1
                )
            }
        );

        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => service.CreateAsync(dto));

        Assert.Equal("All teachers must be from the same school as the class.", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowValidationException_WhenTeacherIsNotAssignedToSubject()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        ctx.Classes.Add(new Class
        {
            Id = 100,
            Name = "5A",
            SchoolId = 1
        });

        ctx.Subjects.Add(new Subject
        {
            Id = 100,
            Name = "Math"
        });

        ctx.Teachers.Add(new Teacher
        {
            Id = 100,
            UserId = "teacher-user-id",
            SchoolId = 1
        });

        await ctx.SaveChangesAsync();

        var dto = new CreateCurriculumDto(
            "First Term",
            100,
            new List<CreateCurriculumEntryDto>
            {
                new CreateCurriculumEntryDto(
                    100,
                    100,
                    "Monday",
                    1
                )
            }
        );

        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => service.CreateAsync(dto));

        Assert.Equal("Teacher 100 is not assigned to subject 100.", exception.Message);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllCurricula()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        var classEntity = new Class
        {
            Id = 100,
            Name = "5A",
            SchoolId = 1
        };

        ctx.Classes.Add(classEntity);

        ctx.Curricula.Add(new Curriculum
        {
            Id = 100,
            Term = "First Term",
            ClassId = 100,
            Class = classEntity
        });

        ctx.Curricula.Add(new Curriculum
        {
            Id = 101,
            Term = "Second Term",
            ClassId = 100,
            Class = classEntity
        });

        await ctx.SaveChangesAsync();

        var result = await service.GetAllAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnCurriculum_WhenItExists()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        var classEntity = new Class
        {
            Id = 100,
            Name = "5A",
            SchoolId = 1
        };

        ctx.Classes.Add(classEntity);

        ctx.Curricula.Add(new Curriculum
        {
            Id = 100,
            Term = "First Term",
            ClassId = 100,
            Class = classEntity
        });

        await ctx.SaveChangesAsync();

        var result = await service.GetAsync(100);

        Assert.NotNull(result);
        Assert.Equal(100, result!.Id);
        Assert.Equal("First Term", result.Term);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnNull_WhenCurriculumDoesNotExist()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        var result = await service.GetAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteCurriculum_WhenItExists()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        var classEntity = new Class
        {
            Id = 100,
            Name = "5A",
            SchoolId = 1
        };

        var curriculum = new Curriculum
        {
            Id = 100,
            Term = "First Term",
            ClassId = 100,
            Class = classEntity,
            Entries = new List<CurriculumEntry>()
        };

        ctx.Classes.Add(classEntity);
        ctx.Curricula.Add(curriculum);

        await ctx.SaveChangesAsync();

        await service.DeleteAsync(100);

        Assert.Empty(ctx.Curricula);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowKeyNotFoundException_WhenCurriculumDoesNotExist()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.DeleteAsync(999));

        Assert.Equal("Curriculum not found", exception.Message);
    }

    [Fact]
    public async Task GetCurrentBySchoolIdAsync_ShouldReturnLatestCurriculumForSchool()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        var classFromSchoolOne = new Class
        {
            Id = 100,
            Name = "5A",
            SchoolId = 1
        };

        var classFromSchoolTwo = new Class
        {
            Id = 101,
            Name = "6A",
            SchoolId = 2
        };

        ctx.Classes.Add(classFromSchoolOne);
        ctx.Classes.Add(classFromSchoolTwo);

        ctx.Curricula.Add(new Curriculum
        {
            Id = 100,
            Term = "Old Term",
            ClassId = 100,
            Class = classFromSchoolOne
        });

        ctx.Curricula.Add(new Curriculum
        {
            Id = 101,
            Term = "Latest Term",
            ClassId = 100,
            Class = classFromSchoolOne
        });

        ctx.Curricula.Add(new Curriculum
        {
            Id = 102,
            Term = "Other School Term",
            ClassId = 101,
            Class = classFromSchoolTwo
        });

        await ctx.SaveChangesAsync();

        var result = await service.GetCurrentBySchoolIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(101, result!.Id);
        Assert.Equal("Latest Term", result.Term);
    }

    [Fact]
    public async Task GetCurrentBySchoolIdAsync_ShouldReturnNull_WhenSchoolHasNoCurriculum()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        var result = await service.GetCurrentBySchoolIdAsync(999);

        Assert.Null(result);
    }
}