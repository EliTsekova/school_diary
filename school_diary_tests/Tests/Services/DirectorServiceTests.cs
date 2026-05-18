using AutoMapper;
using Microsoft.EntityFrameworkCore;
using school_diary.Data;
using school_diary.Dtos;
using school_diary.Models;
using school_diary.Services;
using Xunit;

namespace school_diary.school_diary_tests.Tests;

public class DirectorServiceTests
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
            cfg.CreateMap<CreateDirectorDto, Director>();
            cfg.CreateMap<Director, DirectorDto>()
                .ConstructUsing(d => new DirectorDto(d.Id, d.UserId, d.SchoolId));
        });

        return config.CreateMapper();
    }

    private static DirectorService CreateService(ApplicationDbContext ctx)
    {
        return new DirectorService(ctx, CreateMapper());
    }

    private static User CreateUser(string id, string email)
    {
        return new User
        {
            Id = id,
            UserName = email,
            Email = email,
            FirstName = "Test",
            LastName = "User",
            Role = Role.Director
        };
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
    public async Task CreateAsync_ShouldCreateDirector_WhenDataIsValid()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        ctx.Users.Add(CreateUser("user-1", "director@test.com"));
        ctx.Schools.Add(CreateSchool(1));

        await ctx.SaveChangesAsync();

        var dto = new CreateDirectorDto
        {
            UserId = "user-1",
            SchoolId = 1
        };

        var result = await service.CreateAsync(dto);

        Assert.NotNull(result);
        Assert.Equal("user-1", result.UserId);
        Assert.Equal(1, result.SchoolId);
        Assert.Equal(1, ctx.Directors.Count());
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowKeyNotFoundException_WhenUserDoesNotExist()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        ctx.Schools.Add(CreateSchool(1));
        await ctx.SaveChangesAsync();

        var dto = new CreateDirectorDto
        {
            UserId = "missing-user",
            SchoolId = 1
        };

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.CreateAsync(dto));

        Assert.Equal("User not found", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowKeyNotFoundException_WhenSchoolDoesNotExist()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        ctx.Users.Add(CreateUser("user-1", "director@test.com"));
        await ctx.SaveChangesAsync();

        var dto = new CreateDirectorDto
        {
            UserId = "user-1",
            SchoolId = 999
        };

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.CreateAsync(dto));

        Assert.Equal("School not found", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowInvalidOperationException_WhenSchoolAlreadyHasDirector()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        ctx.Users.Add(CreateUser("user-1", "director1@test.com"));
        ctx.Users.Add(CreateUser("user-2", "director2@test.com"));
        ctx.Schools.Add(CreateSchool(1));

        ctx.Directors.Add(new Director
        {
            Id = 1,
            UserId = "user-1",
            SchoolId = 1
        });

        await ctx.SaveChangesAsync();

        var dto = new CreateDirectorDto
        {
            UserId = "user-2",
            SchoolId = 1
        };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateAsync(dto));

        Assert.Equal("School already has director", exception.Message);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllDirectors()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        ctx.Directors.Add(new Director
        {
            Id = 1,
            UserId = "user-1",
            SchoolId = 1
        });

        ctx.Directors.Add(new Director
        {
            Id = 2,
            UserId = "user-2",
            SchoolId = 2
        });

        await ctx.SaveChangesAsync();

        var result = await service.GetAllAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnDirector_WhenDirectorExists()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        ctx.Directors.Add(new Director
        {
            Id = 1,
            UserId = "user-1",
            SchoolId = 1
        });

        await ctx.SaveChangesAsync();

        var result = await service.GetAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result!.Id);
        Assert.Equal("user-1", result.UserId);
        Assert.Equal(1, result.SchoolId);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnNull_WhenDirectorDoesNotExist()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        var result = await service.GetAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task ChangeSchoolAsync_ShouldChangeDirectorSchool()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        ctx.Schools.Add(CreateSchool(1));
        ctx.Schools.Add(CreateSchool(2));

        ctx.Directors.Add(new Director
        {
            Id = 1,
            UserId = "user-1",
            SchoolId = 1
        });

        await ctx.SaveChangesAsync();

        await service.ChangeSchoolAsync(1, 2);

        var director = await ctx.Directors.FindAsync(1);

        Assert.NotNull(director);
        Assert.Equal(2, director!.SchoolId);
    }

    [Fact]
    public async Task ChangeSchoolAsync_ShouldThrowKeyNotFoundException_WhenDirectorDoesNotExist()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.ChangeSchoolAsync(999, 1));

        Assert.Equal("Director not found", exception.Message);
    }

    [Fact]
    public async Task ChangeSchoolAsync_ShouldThrowInvalidOperationException_WhenSchoolAlreadyHasOtherDirector()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        ctx.Directors.Add(new Director
        {
            Id = 1,
            UserId = "user-1",
            SchoolId = 1
        });

        ctx.Directors.Add(new Director
        {
            Id = 2,
            UserId = "user-2",
            SchoolId = 2
        });

        await ctx.SaveChangesAsync();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ChangeSchoolAsync(1, 2));

        Assert.Equal("School already has director", exception.Message);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteDirector_WhenDirectorExists()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        ctx.Directors.Add(new Director
        {
            Id = 1,
            UserId = "user-1",
            SchoolId = 1
        });

        await ctx.SaveChangesAsync();

        await service.DeleteAsync(1);

        Assert.Empty(ctx.Directors);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowKeyNotFoundException_WhenDirectorDoesNotExist()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.DeleteAsync(999));

        Assert.Equal("Director not found", exception.Message);
    }

    [Fact]
    public async Task GetSchoolIdByUserId_ShouldReturnSchoolId_WhenDirectorExists()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        ctx.Directors.Add(new Director
        {
            Id = 1,
            UserId = "user-1",
            SchoolId = 5
        });

        await ctx.SaveChangesAsync();

        var result = await service.GetSchoolIdByUserId("user-1");

        Assert.Equal(5, result);
    }

    [Fact]
    public async Task GetSchoolIdByUserId_ShouldThrowKeyNotFoundException_WhenDirectorDoesNotExist()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.GetSchoolIdByUserId("missing-user"));

        Assert.Equal("Director not found", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateDirectorUserAndSchool()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        var user = CreateUser("user-1", "old@test.com");

        ctx.Users.Add(user);
        ctx.Schools.Add(CreateSchool(1));
        ctx.Schools.Add(CreateSchool(2));

        ctx.Directors.Add(new Director
        {
            Id = 1,
            UserId = "user-1",
            User = user,
            SchoolId = 1
        });

        await ctx.SaveChangesAsync();

        var dto = new UpdateDirectorDto(
            "NewFirst",
            "NewLast",
            "new@test.com",
            2
        );

        await service.UpdateAsync(1, dto);

        var director = await ctx.Directors
            .Include(d => d.User)
            .FirstAsync(d => d.Id == 1);

        Assert.Equal(2, director.SchoolId);
        Assert.Equal("NewFirst", director.User.FirstName);
        Assert.Equal("NewLast", director.User.LastName);
        Assert.Equal("new@test.com", director.User.Email);
        Assert.Equal("new@test.com", director.User.UserName);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowKeyNotFoundException_WhenDirectorDoesNotExist()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        var dto = new UpdateDirectorDto(
            "First",
            "Last",
            "test@test.com",
            1
        );

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.UpdateAsync(999, dto));

        Assert.Equal("Director not found", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowKeyNotFoundException_WhenSchoolDoesNotExist()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        var user = CreateUser("user-1", "old@test.com");

        ctx.Users.Add(user);

        ctx.Directors.Add(new Director
        {
            Id = 1,
            UserId = "user-1",
            User = user,
            SchoolId = 1
        });

        await ctx.SaveChangesAsync();

        var dto = new UpdateDirectorDto(
            "First",
            "Last",
            "new@test.com",
            999
        );

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.UpdateAsync(1, dto));

        Assert.Equal("School not found", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowInvalidOperationException_WhenSchoolAlreadyHasOtherDirector()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        var user1 = CreateUser("user-1", "director1@test.com");
        var user2 = CreateUser("user-2", "director2@test.com");

        ctx.Users.Add(user1);
        ctx.Users.Add(user2);

        ctx.Schools.Add(CreateSchool(1));
        ctx.Schools.Add(CreateSchool(2));

        ctx.Directors.Add(new Director
        {
            Id = 1,
            UserId = "user-1",
            User = user1,
            SchoolId = 1
        });

        ctx.Directors.Add(new Director
        {
            Id = 2,
            UserId = "user-2",
            User = user2,
            SchoolId = 2
        });

        await ctx.SaveChangesAsync();

        var dto = new UpdateDirectorDto(
            "First",
            "Last",
            "new@test.com",
            2
        );

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.UpdateAsync(1, dto));

        Assert.Equal("School already has director", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowInvalidOperationException_WhenEmailAlreadyExists()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx);

        var user1 = CreateUser("user-1", "director1@test.com");
        var user2 = CreateUser("user-2", "taken@test.com");

        ctx.Users.Add(user1);
        ctx.Users.Add(user2);

        ctx.Schools.Add(CreateSchool(1));

        ctx.Directors.Add(new Director
        {
            Id = 1,
            UserId = "user-1",
            User = user1,
            SchoolId = 1
        });

        await ctx.SaveChangesAsync();

        var dto = new UpdateDirectorDto(
            "First",
            "Last",
            "taken@test.com",
            1
        );

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.UpdateAsync(1, dto));

        Assert.Equal("Email already exists", exception.Message);
    }
}
