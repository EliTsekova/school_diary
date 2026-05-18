using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using school_diary.Data;
using school_diary.Dtos;
using school_diary.Models;
using school_diary.Services;
using Xunit;

namespace school_diary.school_diary_tests.Tests;

public class AbsenceServiceTests
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
            cfg.CreateMap<CreateAbsenceDto, Absence>();
            cfg.CreateMap<UpdateAbsenceDto, Absence>();
            cfg.CreateMap<Absence, AbsenceDto>();
        });

        return config.CreateMapper();
    }

    private static AbsenceService CreateService(ApplicationDbContext ctx)
        => new AbsenceService(ctx, CreateMapper());

    [Fact]
    public async Task CreateAsync_ShouldCreateAbsence_WhenTeacherIsAssignedToSubjectAndStudentIsInSameSchool()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        ctx.Subjects.Add(new Subject
        {
            Id = 1,
            Name = "Math"
        });

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

        ctx.Students.Add(new Student
        {
            Id = 1,
            UserId = "student-user-id",
            SchoolId = 1
        });

        await ctx.SaveChangesAsync();

        var dto = new CreateAbsenceDto(
            1,
            1,
            DateTime.UtcNow,
            false
        );

        var result = await service.CreateAsync(dto, "teacher-user-id");

        Assert.NotNull(result);
        Assert.Equal(1, result.StudentId);
        Assert.Equal(1, result.SubjectId);
        Assert.Equal(1, ctx.Absences.Count());
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowUnauthorized_WhenTeacherDoesNotExist()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        var dto = new CreateAbsenceDto(
            1,
            1,
            DateTime.UtcNow,
            false
        );

        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.CreateAsync(dto, "missing-teacher"));

        Assert.Equal("Teacher not found.", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowUnauthorized_WhenTeacherIsNotAssignedToSubject()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

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
                    SubjectId = 2
                }
            }
        });

        await ctx.SaveChangesAsync();

        var dto = new CreateAbsenceDto(
            1,
            1,
            DateTime.UtcNow,
            false
        );

        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.CreateAsync(dto, "teacher-user-id"));

        Assert.Equal("You are not assigned to this subject.", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowKeyNotFound_WhenStudentDoesNotExist()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

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

        var dto = new CreateAbsenceDto(
            999,
            1,
            DateTime.UtcNow,
            false
        );

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.CreateAsync(dto, "teacher-user-id"));

        Assert.Equal("Student not found.", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowUnauthorized_WhenStudentIsInDifferentSchool()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

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

        ctx.Students.Add(new Student
        {
            Id = 1,
            UserId = "student-user-id",
            SchoolId = 2
        });

        await ctx.SaveChangesAsync();

        var dto = new CreateAbsenceDto(
            1,
            1,
            DateTime.UtcNow,
            false
        );

        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.CreateAsync(dto, "teacher-user-id"));

        Assert.Equal("You are not teaching at this student's school.", exception.Message);
    }

    [Fact]
    public async Task GetCountAsync_ShouldReturnAbsenceCountForStudent()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        ctx.Absences.AddRange(
            new Absence
            {
                Id = 1,
                StudentId = 1,
                SubjectId = 1,
                TeacherId = 1
            },
            new Absence
            {
                Id = 2,
                StudentId = 1,
                SubjectId = 1,
                TeacherId = 1
            },
            new Absence
            {
                Id = 3,
                StudentId = 2,
                SubjectId = 1,
                TeacherId = 1
            }
        );

        await ctx.SaveChangesAsync();

        var count = await service.GetCountAsync(1);

        Assert.Equal(2, count);
    }

    [Fact]
    public async Task GetByStudentAsync_ShouldReturnOnlyStudentAbsences()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        ctx.Absences.AddRange(
            new Absence
            {
                Id = 1,
                StudentId = 1,
                SubjectId = 1,
                TeacherId = 1
            },
            new Absence
            {
                Id = 2,
                StudentId = 1,
                SubjectId = 2,
                TeacherId = 1
            },
            new Absence
            {
                Id = 3,
                StudentId = 2,
                SubjectId = 1,
                TeacherId = 1
            }
        );

        await ctx.SaveChangesAsync();

        var result = await service.GetByStudentAsync(1);

        Assert.Equal(2, result.Count);

        foreach (var absence in result)
        {
            Assert.Equal(1, absence.StudentId);
        }
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteAbsence()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        ctx.Teachers.Add(new Teacher
        {
            Id = 1,
            UserId = "teacher-user-id",
            SchoolId = 1
        });

        ctx.Absences.Add(new Absence
        {
            Id = 1,
            StudentId = 1,
            SubjectId = 1,
            TeacherId = 1
        });

        await ctx.SaveChangesAsync();

        await service.DeleteAsync(1, "teacher-user-id");

        Assert.Empty(ctx.Absences);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenAbsenceDoesNotExist()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        ctx.Teachers.Add(new Teacher
        {
            Id = 1,
            UserId = "teacher-user-id"
        });

        await ctx.SaveChangesAsync();

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.DeleteAsync(999, "teacher-user-id"));
    }

    [Fact]
    public async Task GetAbsencesForParentAsync_ShouldReturnOnlyChildAbsences()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        ctx.Parents.Add(new Parent
        {
            Id = 1,
            UserId = "parent-user-id",
            ParentStudents = new List<ParentStudent>
            {
                new ParentStudent
                {
                    ParentId = 1,
                    StudentId = 1
                }
            }
        });

        ctx.Absences.AddRange(
            new Absence
            {
                Id = 1,
                StudentId = 1,
                SubjectId = 1,
                TeacherId = 1
            },
            new Absence
            {
                Id = 2,
                StudentId = 2,
                SubjectId = 1,
                TeacherId = 1
            }
        );

        await ctx.SaveChangesAsync();

        var result = await service.GetAbsencesForParentAsync("parent-user-id");

        Assert.Single(result);
        Assert.Equal(1, result.First().StudentId);
    }
}