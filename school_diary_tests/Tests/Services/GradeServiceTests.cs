namespace school_diary.school_diary_tests.Tests;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using school_diary.Data;
using school_diary.Dtos;
using school_diary.Models;
using school_diary.Services;
using Xunit;


public class GradeServiceTests
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
            cfg.CreateMap<CreateGradeDto, Grade>();
            cfg.CreateMap<UpdateGradeDto, Grade>();

            cfg.CreateMap<Grade, GradeDto>()
                .ConstructUsing(g => new GradeDto(
                    g.Id,
                    g.Value,
                    g.StudentId,
                    g.TeacherId ?? 0,
                    g.SubjectId,
                    g.CreatedOn
                ));
        });

        return config.CreateMapper();
    }

    private static GradeService CreateService(ApplicationDbContext ctx)
    {
        return new GradeService(ctx, CreateMapper());
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateGrade_WhenTeacherIsAssignedAndStudentIsInSameSchool()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        ctx.Students.Add(new Student
        {
            Id = 1,
            UserId = "student-user-id",
            SchoolId = 1
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

        ctx.Subjects.Add(new Subject
        {
            Id = 1,
            Name = "Math"
        });

        await ctx.SaveChangesAsync();

        var dto = new CreateGradeDto(
            6,
            1,
            1
        );

        var result = await service.CreateAsync(dto, "teacher-user-id");

        Assert.NotNull(result);
        Assert.Equal(6, result.Value);
        Assert.Equal(1, result.StudentId);
        Assert.Equal(1, result.SubjectId);
        Assert.Equal(1, result.TeacherId);
        Assert.Equal(1, ctx.Grades.Count());
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowKeyNotFoundException_WhenStudentDoesNotExist()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        var dto = new CreateGradeDto(
            6,
            999,
            1
        );

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.CreateAsync(dto, "teacher-user-id"));

        Assert.Equal("Student not found", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowUnauthorizedAccessException_WhenTeacherDoesNotExist()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        ctx.Students.Add(new Student
        {
            Id = 1,
            UserId = "student-user-id",
            SchoolId = 1
        });

        await ctx.SaveChangesAsync();

        var dto = new CreateGradeDto(
            6,
            1,
            1
        );

        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.CreateAsync(dto, "missing-teacher"));

        Assert.Equal("Teacher not found", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowUnauthorizedAccessException_WhenTeacherIsNotAssignedToSubject()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        ctx.Students.Add(new Student
        {
            Id = 1,
            UserId = "student-user-id",
            SchoolId = 1
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
                    SubjectId = 2
                }
            }
        });

        await ctx.SaveChangesAsync();

        var dto = new CreateGradeDto(
            6,
            1,
            1
        );

        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.CreateAsync(dto, "teacher-user-id"));

        Assert.Equal("You are not assigned to this subject.", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowUnauthorizedAccessException_WhenStudentIsInDifferentSchool()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        ctx.Students.Add(new Student
        {
            Id = 1,
            UserId = "student-user-id",
            SchoolId = 2
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

        await ctx.SaveChangesAsync();

        var dto = new CreateGradeDto(
            6,
            1,
            1
        );

        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.CreateAsync(dto, "teacher-user-id"));

        Assert.Equal("You are not teaching at this student's school.", exception.Message);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllGrades()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        ctx.Grades.Add(new Grade
        {
            Id = 1,
            Value = 6,
            StudentId = 1,
            SubjectId = 1,
            TeacherId = 1,
            CreatedOn = DateTime.UtcNow
        });

        ctx.Grades.Add(new Grade
        {
            Id = 2,
            Value = 5,
            StudentId = 2,
            SubjectId = 1,
            TeacherId = 1,
            CreatedOn = DateTime.UtcNow
        });

        await ctx.SaveChangesAsync();

        var result = await service.GetAllAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnGrade_WhenGradeExists()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        ctx.Grades.Add(new Grade
        {
            Id = 1,
            Value = 6,
            StudentId = 1,
            SubjectId = 1,
            TeacherId = 1,
            CreatedOn = DateTime.UtcNow
        });

        await ctx.SaveChangesAsync();

        var result = await service.GetAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result!.Id);
        Assert.Equal(6, result.Value);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnNull_WhenGradeDoesNotExist()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        var result = await service.GetAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetForTeacherAsync_ShouldReturnGrade_WhenGradeBelongsToTeacher()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        ctx.Teachers.Add(new Teacher
        {
            Id = 1,
            UserId = "teacher-user-id",
            SchoolId = 1
        });

        ctx.Grades.Add(new Grade
        {
            Id = 1,
            Value = 6,
            StudentId = 1,
            SubjectId = 1,
            TeacherId = 1,
            CreatedOn = DateTime.UtcNow
        });

        await ctx.SaveChangesAsync();

        var result = await service.GetForTeacherAsync(1, "teacher-user-id");

        Assert.NotNull(result);
        Assert.Equal(1, result!.Id);
        Assert.Equal(1, result.TeacherId);
    }

    [Fact]
    public async Task GetForTeacherAsync_ShouldReturnNull_WhenGradeDoesNotBelongToTeacher()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        ctx.Teachers.Add(new Teacher
        {
            Id = 1,
            UserId = "teacher-user-id",
            SchoolId = 1
        });

        ctx.Grades.Add(new Grade
        {
            Id = 1,
            Value = 6,
            StudentId = 1,
            SubjectId = 1,
            TeacherId = 2,
            CreatedOn = DateTime.UtcNow
        });

        await ctx.SaveChangesAsync();

        var result = await service.GetForTeacherAsync(1, "teacher-user-id");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetForTeacherAsync_ShouldThrowUnauthorizedAccessException_WhenTeacherDoesNotExist()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.GetForTeacherAsync(1, "missing-teacher"));

        Assert.Equal("Teacher not found.", exception.Message);
    }

    [Fact]
    public async Task GetAllForTeacherAsync_ShouldReturnOnlyTeacherGrades()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        ctx.Teachers.Add(new Teacher
        {
            Id = 1,
            UserId = "teacher-user-id",
            SchoolId = 1
        });

        ctx.Grades.Add(new Grade
        {
            Id = 1,
            Value = 6,
            StudentId = 1,
            SubjectId = 1,
            TeacherId = 1,
            CreatedOn = DateTime.UtcNow
        });

        ctx.Grades.Add(new Grade
        {
            Id = 2,
            Value = 4,
            StudentId = 2,
            SubjectId = 1,
            TeacherId = 2,
            CreatedOn = DateTime.UtcNow
        });

        await ctx.SaveChangesAsync();

        var result = await service.GetAllForTeacherAsync("teacher-user-id");

        Assert.Single(result);
        Assert.Equal(1, result.First().TeacherId);
    }

    [Fact]
    public async Task DeleteAsAdminAsync_ShouldDeleteGrade_WhenGradeExists()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        ctx.Grades.Add(new Grade
        {
            Id = 1,
            Value = 6,
            StudentId = 1,
            SubjectId = 1,
            TeacherId = 1,
            CreatedOn = DateTime.UtcNow
        });

        await ctx.SaveChangesAsync();

        await service.DeleteAsAdminAsync(1);

        Assert.Empty(ctx.Grades);
    }

    [Fact]
    public async Task DeleteAsAdminAsync_ShouldThrowKeyNotFoundException_WhenGradeDoesNotExist()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.DeleteAsAdminAsync(999));

        Assert.Equal("Grade not found", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateGrade_WhenTeacherOwnsGrade()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        var student = new Student
        {
            Id = 1,
            UserId = "student-user-id",
            SchoolId = 1
        };

        var teacher = new Teacher
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
        };

        ctx.Students.Add(student);
        ctx.Teachers.Add(teacher);

        ctx.Grades.Add(new Grade
        {
            Id = 1,
            Value = 4,
            StudentId = 1,
            Student = student,
            SubjectId = 1,
            TeacherId = 1,
            Teacher = teacher,
            CreatedOn = DateTime.UtcNow
        });

        await ctx.SaveChangesAsync();

        var dto = new UpdateGradeDto(6);

        await service.UpdateAsync(1, dto, "teacher-user-id");

        var grade = await ctx.Grades.FindAsync(1);

        Assert.NotNull(grade);
        Assert.Equal(6, grade!.Value);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowKeyNotFoundException_WhenGradeDoesNotExist()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        var dto = new UpdateGradeDto(6);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.UpdateAsync(999, dto, "teacher-user-id"));

        Assert.Equal("Grade not found", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowUnauthorizedAccessException_WhenTeacherDoesNotOwnGrade()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        var student = new Student
        {
            Id = 1,
            UserId = "student-user-id",
            SchoolId = 1
        };

        var teacher = new Teacher
        {
            Id = 1,
            UserId = "teacher-owner-id",
            SchoolId = 1,
            TeacherSubjects = new List<TeacherSubject>
            {
                new TeacherSubject
                {
                    TeacherId = 1,
                    SubjectId = 1
                }
            }
        };

        ctx.Students.Add(student);
        ctx.Teachers.Add(teacher);

        ctx.Grades.Add(new Grade
        {
            Id = 1,
            Value = 5,
            StudentId = 1,
            Student = student,
            SubjectId = 1,
            TeacherId = 1,
            Teacher = teacher,
            CreatedOn = DateTime.UtcNow
        });

        await ctx.SaveChangesAsync();

        var dto = new UpdateGradeDto(6);

        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.UpdateAsync(1, dto, "another-teacher-id"));

        Assert.Equal("You can edit only your own grades.", exception.Message);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteGrade_WhenTeacherOwnsGrade()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        var teacher = new Teacher
        {
            Id = 1,
            UserId = "teacher-user-id",
            SchoolId = 1
        };

        ctx.Teachers.Add(teacher);

        ctx.Grades.Add(new Grade
        {
            Id = 1,
            Value = 6,
            StudentId = 1,
            SubjectId = 1,
            TeacherId = 1,
            Teacher = teacher,
            CreatedOn = DateTime.UtcNow
        });

        await ctx.SaveChangesAsync();

        await service.DeleteAsync(1, "teacher-user-id");

        Assert.Empty(ctx.Grades);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowUnauthorizedAccessException_WhenTeacherDoesNotOwnGrade()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        var teacher = new Teacher
        {
            Id = 1,
            UserId = "teacher-owner-id",
            SchoolId = 1
        };

        ctx.Teachers.Add(teacher);

        ctx.Grades.Add(new Grade
        {
            Id = 1,
            Value = 6,
            StudentId = 1,
            SubjectId = 1,
            TeacherId = 1,
            Teacher = teacher,
            CreatedOn = DateTime.UtcNow
        });

        await ctx.SaveChangesAsync();

        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.DeleteAsync(1, "another-teacher-id"));

        Assert.Equal("You can delete only your own grades.", exception.Message);
    }

    [Fact]
    public async Task GetGradesForParentAsync_ShouldReturnOnlyGradesForParentsChildren()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        var parent = new Parent
        {
            Id = 1,
            UserId = "parent-user-id"
        };

        ctx.Parents.Add(parent);

        ctx.ParentStudents.Add(new ParentStudent
        {
            Id = 1,
            ParentId = 1,
            Parent = parent,
            StudentId = 1
        });

        ctx.Grades.Add(new Grade
        {
            Id = 1,
            Value = 6,
            StudentId = 1,
            SubjectId = 1,
            TeacherId = 1,
            CreatedOn = DateTime.UtcNow
        });

        ctx.Grades.Add(new Grade
        {
            Id = 2,
            Value = 4,
            StudentId = 2,
            SubjectId = 1,
            TeacherId = 1,
            CreatedOn = DateTime.UtcNow
        });

        await ctx.SaveChangesAsync();

        var result = await service.GetGradesForParentAsync("parent-user-id");

        Assert.Single(result);
        Assert.Equal(1, result.First().StudentId);
    }

    [Fact]
    public async Task GetGradesForParentAsync_ShouldThrowKeyNotFoundException_WhenParentDoesNotExist()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.GetGradesForParentAsync("missing-parent"));

        Assert.Equal("Parent not found.", exception.Message);
    }

    [Fact]
    public async Task GetLastGradeAsync_ShouldReturnLatestGradeForStudent()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        ctx.Grades.Add(new Grade
        {
            Id = 1,
            Value = 4,
            StudentId = 1,
            SubjectId = 1,
            TeacherId = 1,
            CreatedOn = DateTime.UtcNow.AddDays(-1)
        });

        ctx.Grades.Add(new Grade
        {
            Id = 2,
            Value = 6,
            StudentId = 1,
            SubjectId = 1,
            TeacherId = 1,
            CreatedOn = DateTime.UtcNow
        });

        await ctx.SaveChangesAsync();

        var result = await service.GetLastGradeAsync(1);

        Assert.NotNull(result);
        Assert.Equal(2, result!.Id);
        Assert.Equal(6, result.Value);
    }

    [Fact]
    public async Task GetByStudentAsync_ShouldReturnOnlyGradesForGivenStudent()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        ctx.Grades.Add(new Grade
        {
            Id = 1,
            Value = 6,
            StudentId = 1,
            SubjectId = 1,
            TeacherId = 1,
            CreatedOn = DateTime.UtcNow
        });

        ctx.Grades.Add(new Grade
        {
            Id = 2,
            Value = 5,
            StudentId = 1,
            SubjectId = 2,
            TeacherId = 1,
            CreatedOn = DateTime.UtcNow
        });

        ctx.Grades.Add(new Grade
        {
            Id = 3,
            Value = 4,
            StudentId = 2,
            SubjectId = 1,
            TeacherId = 1,
            CreatedOn = DateTime.UtcNow
        });

        await ctx.SaveChangesAsync();

        var result = await service.GetByStudentAsync(1);

        Assert.Equal(2, result.Count);

        foreach (var grade in result)
        {
            Assert.Equal(1, grade.StudentId);
        }
    }
}