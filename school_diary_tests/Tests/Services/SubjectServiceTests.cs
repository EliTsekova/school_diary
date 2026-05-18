namespace school_diary.school_diary_tests.Tests;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using school_diary.Data;
using school_diary.Dtos;
using school_diary.Models;
using school_diary.Services;
using Xunit;

public class SubjectServiceTests
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
            cfg.CreateMap<Subject, SubjectDto>();
            cfg.CreateMap<SubjectDto, Subject>();
        });

        return config.CreateMapper();
    }

    private static SubjectService CreateService(ApplicationDbContext ctx)
    {
        return new SubjectService(ctx, CreateMapper());
    }

    [Fact]
    public async Task CreateSubjectAsync_ShouldCreateSubject()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        var dto = new SubjectDto(999, "Math");

        var result = await service.CreateSubjectAsync(dto);

        Assert.NotNull(result);
        Assert.NotEqual(999, result.Id);
        Assert.Equal("Math", result.Name);
        Assert.Single(ctx.Subjects);
    }

    [Fact]
    public async Task CreateSubjectAsync_ShouldThrowArgumentNullException_WhenDtoIsNull()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => service.CreateSubjectAsync(null!));
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllSubjects()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        ctx.Subjects.Add(new Subject { Id = 1, Name = "Math" });
        ctx.Subjects.Add(new Subject { Id = 2, Name = "History" });

        await ctx.SaveChangesAsync();

        var result = await service.GetAllAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetByIdsAsync_ShouldReturnOnlySubjectsWithGivenIds()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        ctx.Subjects.Add(new Subject { Id = 1, Name = "Math" });
        ctx.Subjects.Add(new Subject { Id = 2, Name = "History" });
        ctx.Subjects.Add(new Subject { Id = 3, Name = "Biology" });

        await ctx.SaveChangesAsync();

        var result = await service.GetByIdsAsync(new List<int> { 1, 3 });

        Assert.Equal(2, result.Count);
        Assert.Contains(result, s => s.Id == 1);
        Assert.Contains(result, s => s.Id == 3);
        Assert.DoesNotContain(result, s => s.Id == 2);
    }

    [Fact]
    public async Task GetByIdsAsync_ShouldIgnoreDuplicateIds()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        ctx.Subjects.Add(new Subject { Id = 1, Name = "Math" });

        await ctx.SaveChangesAsync();

        var result = await service.GetByIdsAsync(new List<int> { 1, 1, 1 });

        Assert.Single(result);
        Assert.Equal(1, result.First().Id);
    }

    [Fact]
    public async Task GetByIdsAsync_ShouldReturnEmptyList_WhenIdsAreNull()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        var result = await service.GetByIdsAsync(null!);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdsAsync_ShouldReturnEmptyList_WhenIdsAreEmpty()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        var result = await service.GetByIdsAsync(new List<int>());

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetNamesByIdsAsync_ShouldReturnDictionaryWithSubjectNames()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        ctx.Subjects.Add(new Subject { Id = 1, Name = "Math" });
        ctx.Subjects.Add(new Subject { Id = 2, Name = "History" });
        ctx.Subjects.Add(new Subject { Id = 3, Name = "Biology" });

        await ctx.SaveChangesAsync();

        var result = await service.GetNamesByIdsAsync(new List<int> { 1, 3 });

        Assert.Equal(2, result.Count);
        Assert.Equal("Math", result[1]);
        Assert.Equal("Biology", result[3]);
        Assert.False(result.ContainsKey(2));
    }

    [Fact]
    public async Task GetNamesByIdsAsync_ShouldIgnoreDuplicateIds()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        ctx.Subjects.Add(new Subject { Id = 1, Name = "Math" });

        await ctx.SaveChangesAsync();

        var result = await service.GetNamesByIdsAsync(new List<int> { 1, 1, 1 });

        Assert.Single(result);
        Assert.Equal("Math", result[1]);
    }

    [Fact]
    public async Task GetNamesByIdsAsync_ShouldReturnEmptyDictionary_WhenIdsAreNull()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        var result = await service.GetNamesByIdsAsync(null!);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetNamesByIdsAsync_ShouldReturnEmptyDictionary_WhenIdsAreEmpty()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        var result = await service.GetNamesByIdsAsync(new List<int>());

        Assert.Empty(result);
    }
}