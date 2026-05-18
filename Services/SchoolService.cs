using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using school_diary.Data;
using school_diary.Dtos;
using school_diary.Models;

namespace school_diary.Services;

public class SchoolService : ISchoolService
{
    private readonly ApplicationDbContext _ctx;
    private readonly IMapper _mapper;

    public SchoolService(ApplicationDbContext ctx, IMapper mapper)
    {
        _ctx = ctx;
        _mapper = mapper;
    }

    public async Task<SchoolDto?> GetAsync(int id) =>
        await _ctx.Schools
            .AsNoTracking()
            .Where(s => s.Id == id)
            .ProjectTo<SchoolDto>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync();

    public async Task<IReadOnlyList<SchoolDto>> GetAllAsync() =>
        await _ctx.Schools
            .AsNoTracking()
            .ProjectTo<SchoolDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

    public async Task<SchoolDto> CreateAsync(CreateSchoolDto dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));

        var entity = _mapper.Map<School>(dto);
        _ctx.Schools.Add(entity);

        try
        {
            await _ctx.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Unable to create school. It may already exist.", ex);
        }

        return await _ctx.Schools
            .AsNoTracking()
            .Where(s => s.Id == entity.Id)
            .ProjectTo<SchoolDto>(_mapper.ConfigurationProvider)
            .SingleAsync();
    }

    public async Task UpdateAsync(int id, UpdateSchoolDto dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));

        var entity = await _ctx.Schools
            .FirstOrDefaultAsync(s => s.Id == id)
            ?? throw new KeyNotFoundException("School not found");

        _mapper.Map(dto, entity);

        try
        {
            await _ctx.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Unable to update school.", ex);
        }
    }

    public async Task DeleteAsync(int id)
    {
        var hasClasses = await _ctx.Classes
            .AsNoTracking()
            .AnyAsync(c => c.SchoolId == id);

        if (hasClasses)
            throw new InvalidOperationException("Cannot delete school because it has classes assigned.");

        var entity = await _ctx.Schools.FindAsync(id)
            ?? throw new KeyNotFoundException("School not found");

        _ctx.Schools.Remove(entity);

        try
        {
            await _ctx.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Unable to delete school because it has related data.", ex);
        }
    }
    public async Task<SchoolDto?> GetForDirectorAsync(string directorUserId) =>
        await _ctx.Schools
            .AsNoTracking()
            .Where(s => _ctx.Directors.Any(d => d.UserId == directorUserId && d.SchoolId == s.Id))
            .ProjectTo<SchoolDto>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync();
}