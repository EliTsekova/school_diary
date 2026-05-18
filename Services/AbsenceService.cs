using System.ComponentModel.DataAnnotations;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using school_diary.Data;
using school_diary.Dtos;
using school_diary.Models;

namespace school_diary.Services;

public class AbsenceService : IAbsenceService
{
    private readonly ApplicationDbContext _ctx;
    private readonly IMapper _mapper;

    public AbsenceService(ApplicationDbContext ctx, IMapper mapper)
    {
        _ctx = ctx;
        _mapper = mapper;
    }

    public async Task<AbsenceDto?> GetForTeacherAsync(int id, string teacherUserId)
    {
        var teacher = await _ctx.Teachers
            .AsNoTracking()
            .SingleOrDefaultAsync(t => t.UserId == teacherUserId)
            ?? throw new UnauthorizedAccessException("Teacher not found.");

        return await _ctx.Absences
            .AsNoTracking()
            .Where(a => a.Id == id && a.TeacherId == teacher.Id)
            .ProjectTo<AbsenceDto>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync();
    }

    public async Task<IReadOnlyList<AbsenceDto>> GetAllForTeacherAsync(string teacherUserId)
    {
        var teacher = await _ctx.Teachers
            .AsNoTracking()
            .SingleOrDefaultAsync(t => t.UserId == teacherUserId)
            ?? throw new UnauthorizedAccessException("Teacher not found.");

        return await _ctx.Absences
            .AsNoTracking()
            .Where(a => a.TeacherId == teacher.Id)
            .ProjectTo<AbsenceDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<AbsenceDto> CreateAsync(CreateAbsenceDto dto, string teacherUserId)
    {
        var teacher = await _ctx.Teachers
            .Include(t => t.TeacherSubjects)
            .SingleOrDefaultAsync(t => t.UserId == teacherUserId)
            ?? throw new UnauthorizedAccessException("Teacher not found.");

        if (!teacher.TeacherSubjects.Any(ts => ts.SubjectId == dto.SubjectId))
            throw new UnauthorizedAccessException("You are not assigned to this subject.");

        var student = await _ctx.Students.FindAsync(dto.StudentId)
            ?? throw new KeyNotFoundException("Student not found.");

        if (student.SchoolId != teacher.SchoolId)
            throw new UnauthorizedAccessException("You are not teaching at this student's school.");

        var entity = _mapper.Map<Absence>(dto);
        entity.TeacherId = teacher.Id;

        _ctx.Absences.Add(entity);

        await _ctx.SaveChangesAsync();

        return await _ctx.Absences
            .AsNoTracking()
            .Where(a => a.Id == entity.Id)
            .ProjectTo<AbsenceDto>(_mapper.ConfigurationProvider)
            .SingleAsync();
    }

    public async Task<AbsenceDto> UpdateAsync(int id, UpdateAbsenceDto dto, string teacherUserId)
    {
        var teacher = await _ctx.Teachers
            .Include(t => t.TeacherSubjects)
            .SingleOrDefaultAsync(t => t.UserId == teacherUserId)
            ?? throw new UnauthorizedAccessException("Teacher not found.");

        var entity = await _ctx.Absences
            .Include(a => a.Student)
            .SingleOrDefaultAsync(a => a.Id == id && a.TeacherId == teacher.Id)
            ?? throw new KeyNotFoundException("Absence not found.");

        if (!teacher.TeacherSubjects.Any(ts => ts.SubjectId == dto.SubjectId))
            throw new ValidationException("Teacher is not assigned to this subject.");

        if (entity.Student.SchoolId != teacher.SchoolId)
            throw new UnauthorizedAccessException("Student is not in your school.");

        _mapper.Map(dto, entity);

        entity.TeacherId = teacher.Id;

        await _ctx.SaveChangesAsync();

        return await _ctx.Absences
            .Include(a => a.Student)
            .Include(a => a.Subject)
            .Where(a => a.Id == entity.Id)
            .ProjectTo<AbsenceDto>(_mapper.ConfigurationProvider)
            .SingleAsync();
    }

    public async Task DeleteAsync(int id, string teacherUserId)
    {
        var teacher = await _ctx.Teachers
            .SingleOrDefaultAsync(t => t.UserId == teacherUserId)
            ?? throw new UnauthorizedAccessException("Teacher not found.");

        var entity = await _ctx.Absences
            .SingleOrDefaultAsync(a => a.Id == id && a.TeacherId == teacher.Id)
            ?? throw new KeyNotFoundException("Absence not found.");

        _ctx.Absences.Remove(entity);

        await _ctx.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<AbsenceDto>> GetAbsencesForParentAsync(string parentUserId)
    {
        var parent = await _ctx.Parents
            .AsNoTracking()
            .Include(p => p.ParentStudents)
            .SingleOrDefaultAsync(p => p.UserId == parentUserId)
            ?? throw new KeyNotFoundException("Parent not found.");

        var studentIds = parent.ParentStudents.Select(ps => ps.StudentId).ToList();

        return await _ctx.Absences
            .AsNoTracking()
            .Where(a => studentIds.Contains(a.StudentId))
            .ProjectTo<AbsenceDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<int> GetCountAsync(int studentId) =>
        await _ctx.Absences
            .AsNoTracking()
            .CountAsync(a => a.StudentId == studentId);

    public async Task<List<AbsenceDto>> GetByStudentAsync(int studentId) =>
        await _ctx.Absences
            .AsNoTracking()
            .Where(a => a.StudentId == studentId)
            .ProjectTo<AbsenceDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
}