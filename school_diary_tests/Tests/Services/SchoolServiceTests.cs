namespace school_diary.school_diary_tests.Tests;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using school_diary.Data;
using school_diary.Dtos;
using school_diary.Models;
using school_diary.Services;
using Xunit;


public class SchoolServiceTests
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
            cfg.CreateMap<CreateSchoolDto, School>();
            cfg.CreateMap<UpdateSchoolDto, School>();

            cfg.CreateMap<School, SchoolDto>()
                .ConstructUsing(s => new SchoolDto(
                    s.Id,
                    s.Name,
                    s.Address
                ));
        });

        return config.CreateMapper();
    }

    private static SchoolService CreateService(ApplicationDbContext ctx)
    {
        return new SchoolService(ctx, CreateMapper());
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateSchool_WhenDtoIsValid()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        var dto = new CreateSchoolDto
        {
            Name = "Test School",
            Address = "Test Address"
        };

        var result = await service.CreateAsync(dto);

        Assert.NotNull(result);
        Assert.Equal("Test School", result.Name);
        Assert.Equal("Test Address", result.Address);
        Assert.Equal(1, ctx.Schools.Count());
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowArgumentNullException_WhenDtoIsNull()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => service.CreateAsync(null!));
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllSchools()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        ctx.Schools.Add(new School
        {
            Id = 1,
            Name = "School 1",
            Address = "Address 1"
        });

        ctx.Schools.Add(new School
        {
            Id = 2,
            Name = "School 2",
            Address = "Address 2"
        });

        await ctx.SaveChangesAsync();

        var result = await service.GetAllAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnSchool_WhenSchoolExists()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        ctx.Schools.Add(new School
        {
            Id = 1,
            Name = "School 1",
            Address = "Address 1"
        });

        await ctx.SaveChangesAsync();

        var result = await service.GetAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result!.Id);
        Assert.Equal("School 1", result.Name);
        Assert.Equal("Address 1", result.Address);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnNull_WhenSchoolDoesNotExist()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        var result = await service.GetAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateSchool_WhenSchoolExists()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        ctx.Schools.Add(new School
        {
            Id = 1,
            Name = "Old School",
            Address = "Old Address"
        });

        await ctx.SaveChangesAsync();

        var dto = new UpdateSchoolDto
        {
            Name = "New School",
            Address = "New Address"
        };

        await service.UpdateAsync(1, dto);

        var school = await ctx.Schools.FindAsync(1);

        Assert.NotNull(school);
        Assert.Equal("New School", school!.Name);
        Assert.Equal("New Address", school.Address);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowArgumentNullException_WhenDtoIsNull()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => service.UpdateAsync(1, null!));
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowKeyNotFoundException_WhenSchoolDoesNotExist()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        var dto = new UpdateSchoolDto
        {
            Name = "New School",
            Address = "New Address"
        };

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.UpdateAsync(999, dto));

        Assert.Equal("School not found", exception.Message);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteSchool_WhenSchoolHasNoClasses()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        ctx.Schools.Add(new School
        {
            Id = 1,
            Name = "School 1",
            Address = "Address 1"
        });

        await ctx.SaveChangesAsync();

        await service.DeleteAsync(1);

        Assert.Empty(ctx.Schools);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowInvalidOperationException_WhenSchoolHasClasses()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        ctx.Schools.Add(new School
        {
            Id = 1,
            Name = "School 1",
            Address = "Address 1"
        });

        ctx.Classes.Add(new Class
        {
            Id = 1,
            Name = "5A",
            SchoolId = 1
        });

        await ctx.SaveChangesAsync();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.DeleteAsync(1));

        Assert.Equal("Cannot delete school because it has classes assigned.", exception.Message);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowKeyNotFoundException_WhenSchoolDoesNotExist()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.DeleteAsync(999));

        Assert.Equal("School not found", exception.Message);
    }

    [Fact]
    public async Task GetForDirectorAsync_ShouldReturnSchool_WhenDirectorBelongsToSchool()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        ctx.Schools.Add(new School
        {
            Id = 1,
            Name = "Director School",
            Address = "Director Address"
        });

        ctx.Schools.Add(new School
        {
            Id = 2,
            Name = "Other School",
            Address = "Other Address"
        });

        ctx.Directors.Add(new Director
        {
            Id = 1,
            UserId = "director-user-id",
            SchoolId = 1
        });

        await ctx.SaveChangesAsync();

        var result = await service.GetForDirectorAsync("director-user-id");

        Assert.NotNull(result);
        Assert.Equal(1, result!.Id);
        Assert.Equal("Director School", result.Name);
    }

    [Fact]
    public async Task GetForDirectorAsync_ShouldReturnNull_WhenDirectorDoesNotExist()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        ctx.Schools.Add(new School
        {
            Id = 1,
            Name = "School 1",
            Address = "Address 1"
        });

        await ctx.SaveChangesAsync();

        var result = await service.GetForDirectorAsync("missing-director");

        Assert.Null(result);
    }
}