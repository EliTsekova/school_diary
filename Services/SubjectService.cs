using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using school_diary.Data;
using school_diary.Dtos;
using school_diary.Models;

namespace school_diary.Services;

public class SubjectService : ISubjectService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public SubjectService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<SubjectDto> CreateSubjectAsync(SubjectDto dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        var subject = _mapper.Map<Subject>(dto);

        subject.Id = 0;

        _context.Subjects.Add(subject);
        await _context.SaveChangesAsync();

        return await _context.Subjects
            .AsNoTracking()
            .Where(s => s.Id == subject.Id)
            .ProjectTo<SubjectDto>(_mapper.ConfigurationProvider)
            .SingleAsync();
    }

    public async Task<IReadOnlyList<SubjectDto>> GetAllAsync()
    {
        return await _context.Subjects
            .AsNoTracking()
            .ProjectTo<SubjectDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<List<SubjectDto>> GetByIdsAsync(IEnumerable<int> ids)
    {
        var idList = ids?.Distinct().ToList() ?? new List<int>();
        if (idList.Count == 0) return new();

        return await _context.Subjects
            .AsNoTracking()
            .Where(s => idList.Contains(s.Id))
            .ProjectTo<SubjectDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<Dictionary<int, string>> GetNamesByIdsAsync(IEnumerable<int> subjectIds)
    {
        var idList = subjectIds?.Distinct().ToList() ?? new List<int>();
        if (idList.Count == 0) return new();

        return await _context.Subjects
            .AsNoTracking()
            .Where(s => idList.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => s.Name);
    }
}