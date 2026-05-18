namespace school_diary.Services;

using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using school_diary.Data;
using school_diary.Dtos;
using school_diary.Models;

public class DirectorService : IDirectorService
{
    private readonly ApplicationDbContext _ctx;
    private readonly IMapper _mapper;

    public DirectorService(ApplicationDbContext ctx, IMapper mapper)
    {
        _ctx = ctx;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<DirectorDto>> GetAllAsync()
        => await _ctx.Directors
            .ProjectTo<DirectorDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

    public async Task<DirectorDto?> GetAsync(int id)
        => await _ctx.Directors
            .ProjectTo<DirectorDto>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(d => d.Id == id);

    public async Task<DirectorDto> CreateAsync(CreateDirectorDto dto)
    {
        var userExists = await _ctx.Users.AnyAsync(u => u.Id == dto.UserId);

        if (!userExists)
            throw new KeyNotFoundException("User not found");

        var schoolExists = await _ctx.Schools.AnyAsync(s => s.Id == dto.SchoolId);

        if (!schoolExists)
            throw new KeyNotFoundException("School not found");

        var already = await _ctx.Directors
            .AnyAsync(d => d.SchoolId == dto.SchoolId);

        if (already)
            throw new InvalidOperationException("School already has director");

        var entity = _mapper.Map<Director>(dto);

        _ctx.Directors.Add(entity);

        await _ctx.SaveChangesAsync();

        return _mapper.Map<DirectorDto>(entity);
    }

    public async Task ChangeSchoolAsync(int id, int schoolId)
    {
        var dir = await _ctx.Directors.FindAsync(id)
                  ?? throw new KeyNotFoundException("Director not found");

        var schoolHasOtherDirector = await _ctx.Directors
            .AnyAsync(d => d.SchoolId == schoolId && d.Id != id);

        if (schoolHasOtherDirector)
            throw new InvalidOperationException("School already has director");

        dir.SchoolId = schoolId;

        await _ctx.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var dir = await _ctx.Directors.FindAsync(id)
                  ?? throw new KeyNotFoundException("Director not found");

        _ctx.Directors.Remove(dir);

        await _ctx.SaveChangesAsync();
    }

    public async Task<int> GetSchoolIdByUserId(string userId)
    {
        var dir = await _ctx.Directors
                      .SingleOrDefaultAsync(d => d.UserId == userId)
                  ?? throw new KeyNotFoundException("Director not found");

        return dir.SchoolId;
    }

    public async Task UpdateAsync(int id, UpdateDirectorDto dto)
    {
        var director = await _ctx.Directors
                           .Include(d => d.User)
                           .FirstOrDefaultAsync(d => d.Id == id)
                       ?? throw new KeyNotFoundException("Director not found");

        var schoolExists = await _ctx.Schools
            .AnyAsync(s => s.Id == dto.SchoolId);

        if (!schoolExists)
            throw new KeyNotFoundException("School not found");

        var schoolHasOtherDirector = await _ctx.Directors
            .AnyAsync(d => d.SchoolId == dto.SchoolId && d.Id != id);

        if (schoolHasOtherDirector)
            throw new InvalidOperationException("School already has director");

        var emailTaken = await _ctx.Users
            .AnyAsync(u => u.Email == dto.Email && u.Id != director.UserId);

        if (emailTaken)
            throw new InvalidOperationException("Email already exists");

        director.User.FirstName = dto.FirstName;
        director.User.LastName = dto.LastName;
        director.User.Email = dto.Email;
        director.User.UserName = dto.Email;
        director.SchoolId = dto.SchoolId;

        await _ctx.SaveChangesAsync();
    }
}