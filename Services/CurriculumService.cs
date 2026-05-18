using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using school_diary.Data;
using school_diary.Dtos;
using school_diary.Models;
using System.ComponentModel.DataAnnotations;

namespace school_diary.Services;

public class CurriculumService : ICurriculumService
{
    private readonly ApplicationDbContext _ctx;
    private readonly IMapper _mapper;

    public CurriculumService(ApplicationDbContext ctx, IMapper mapper)
    {
        _ctx = ctx;
        _mapper = mapper;
    }

    public async Task<CurriculumDto> CreateAsync(CreateCurriculumDto dto)
    {
        if (dto.Entries == null || dto.Entries.Count == 0)
            throw new ValidationException("Curriculum entries are required.");

        var cls = await _ctx.Classes
            .AsNoTracking()
            .Select(c => new { c.Id, c.SchoolId })
            .SingleOrDefaultAsync(c => c.Id == dto.ClassId);

        if (cls == null)
            throw new KeyNotFoundException("Class not found");

        var duplicateSlot = dto.Entries
            .GroupBy(e => new { e.DayOfWeek, e.Period })
            .Any(g => g.Count() > 1);

        if (duplicateSlot)
            throw new ValidationException("Only one subject is allowed for the same day and period.");

        var subjectIds = dto.Entries.Select(e => e.SubjectId).Distinct().ToList();

        var existingSubjectIds = await _ctx.Subjects
            .AsNoTracking()
            .Where(s => subjectIds.Contains(s.Id))
            .Select(s => s.Id)
            .ToListAsync();

        if (existingSubjectIds.Count != subjectIds.Count)
            throw new ValidationException("One or more subjects do not exist.");

        var teacherIds = dto.Entries.Select(e => e.TeacherId).Distinct().ToList();

        var teachers = await _ctx.Teachers
            .AsNoTracking()
            .Where(t => teacherIds.Contains(t.Id))
            .Select(t => new { t.Id, t.SchoolId })
            .ToListAsync();

        if (teachers.Count != teacherIds.Count)
            throw new ValidationException("One or more teachers do not exist.");

        if (teachers.Any(t => t.SchoolId != cls.SchoolId))
            throw new ValidationException("All teachers must be from the same school as the class.");

        var allowedPairs = await _ctx.TeacherSubjects
            .AsNoTracking()
            .Where(ts => teacherIds.Contains(ts.TeacherId) && subjectIds.Contains(ts.SubjectId))
            .Select(ts => new { ts.TeacherId, ts.SubjectId })
            .ToListAsync();

        var allowedSet = allowedPairs
            .Select(x => (x.TeacherId, x.SubjectId))
            .ToHashSet();

        foreach (var e in dto.Entries)
        {
            if (!allowedSet.Contains((e.TeacherId, e.SubjectId)))
                throw new ValidationException($"Teacher {e.TeacherId} is not assigned to subject {e.SubjectId}.");
        }

        var entity = new Curriculum
        {
            Term = dto.Term,
            ClassId = dto.ClassId,
            Entries = dto.Entries.Select(e => new CurriculumEntry
            {
                SubjectId = e.SubjectId,
                TeacherId = e.TeacherId,
                DayOfWeek = e.DayOfWeek,
                Period = e.Period
            }).ToList()
        };

        _ctx.Curricula.Add(entity);

        try
        {
            await _ctx.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new Exception(ex.InnerException?.Message ?? ex.Message);
        }

        return await _ctx.Curricula
            .AsNoTracking()
            .Where(c => c.Id == entity.Id)
            .ProjectTo<CurriculumDto>(_mapper.ConfigurationProvider)
            .SingleAsync();
    }

    public async Task<IReadOnlyList<CurriculumDto>> GetAllAsync()
    {
        return await _ctx.Curricula
            .AsNoTracking()
            .ProjectTo<CurriculumDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<CurriculumDto?> GetAsync(int id)
    {
        return await _ctx.Curricula
            .AsNoTracking()
            .Where(c => c.Id == id)
            .ProjectTo<CurriculumDto>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _ctx.Curricula
            .Include(c => c.Entries)
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException("Curriculum not found");

        _ctx.Curricula.Remove(entity);

        await _ctx.SaveChangesAsync();
    }

    public async Task<CurriculumDto?> GetCurrentBySchoolIdAsync(int schoolId)
    {
        return await _ctx.Curricula
            .AsNoTracking()
            .Where(c => c.Class.SchoolId == schoolId)
            .OrderByDescending(c => c.Id)
            .ProjectTo<CurriculumDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();
    }
}