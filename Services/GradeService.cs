using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using school_diary.Data;
using school_diary.Dtos;
using school_diary.Models;

namespace school_diary.Services;

public class GradeService : IGradeService
{
    private readonly ApplicationDbContext _ctx;
    private readonly IMapper _mapper;

    public GradeService(ApplicationDbContext ctx, IMapper mapper)
    {
        _ctx = ctx;
        _mapper = mapper;
    }

    public async Task<GradeDto?> GetAsync(int id) =>
        await _ctx.Grades
            .AsNoTracking()
            .Where(g => g.Id == id)
            .ProjectTo<GradeDto>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync();

    public async Task<IReadOnlyList<GradeDto>> GetAllAsync() =>
        await _ctx.Grades
            .AsNoTracking()
            .ProjectTo<GradeDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

    public async Task DeleteAsAdminAsync(int id)
    {
        var entity = await _ctx.Grades.FindAsync(id)
                     ?? throw new KeyNotFoundException("Grade not found");

        _ctx.Grades.Remove(entity);

        await _ctx.SaveChangesAsync();
    }

    public async Task<GradeDto?> GetForTeacherAsync(int id, string teacherUserId)
    {
        var teacherId = await _ctx.Teachers
            .AsNoTracking()
            .Where(t => t.UserId == teacherUserId)
            .Select(t => t.Id)
            .SingleOrDefaultAsync();

        if (teacherId == 0)
            throw new UnauthorizedAccessException("Teacher not found.");

        return await _ctx.Grades
            .AsNoTracking()
            .Where(g => g.Id == id && g.TeacherId == teacherId)
            .ProjectTo<GradeDto>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync();
    }

    public async Task<IReadOnlyList<GradeDto>> GetAllForTeacherAsync(string teacherUserId)
    {
        var teacherId = await _ctx.Teachers
            .AsNoTracking()
            .Where(t => t.UserId == teacherUserId)
            .Select(t => t.Id)
            .SingleOrDefaultAsync();

        if (teacherId == 0)
            throw new UnauthorizedAccessException("Teacher not found.");

        return await _ctx.Grades
            .AsNoTracking()
            .Where(g => g.TeacherId == teacherId)
            .ProjectTo<GradeDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<GradeDto?> GetForParentAsync(int id, string parentUserId)
    {
        return await _ctx.Grades
            .AsNoTracking()
            .Where(g => g.Id == id)
            .Where(g => _ctx.ParentStudents.Any(ps =>
                ps.Parent.UserId == parentUserId && ps.StudentId == g.StudentId))
            .ProjectTo<GradeDto>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync();
    }

    public async Task<GradeDto> CreateAsync(CreateGradeDto dto, string currentUserId)
    {
        var student = await _ctx.Students
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.Id == dto.StudentId)
            ?? throw new KeyNotFoundException("Student not found");

        var teacher = await _ctx.Teachers
            .Include(t => t.TeacherSubjects)
            .SingleOrDefaultAsync(t => t.UserId == currentUserId)
            ?? throw new UnauthorizedAccessException("Teacher not found");

        if (!teacher.TeacherSubjects.Any(ts => ts.SubjectId == dto.SubjectId))
            throw new UnauthorizedAccessException("You are not assigned to this subject.");

        if (student.SchoolId != teacher.SchoolId)
            throw new UnauthorizedAccessException("You are not teaching at this student's school.");

        var entity = _mapper.Map<Grade>(dto);
        entity.TeacherId = teacher.Id;
        entity.CreatedOn = DateTime.UtcNow;

        _ctx.Grades.Add(entity);

        await _ctx.SaveChangesAsync();

        return await _ctx.Grades
            .AsNoTracking()
            .Where(g => g.Id == entity.Id)
            .ProjectTo<GradeDto>(_mapper.ConfigurationProvider)
            .SingleAsync();
    }

    public async Task UpdateAsync(int id, UpdateGradeDto dto, string teacherUserId)
    {
        var entity = await _ctx.Grades
            .Include(g => g.Student)
            .Include(g => g.Teacher)
            .ThenInclude(t => t.TeacherSubjects)
            .FirstOrDefaultAsync(g => g.Id == id)
            ?? throw new KeyNotFoundException("Grade not found");

        if (entity.Teacher.UserId != teacherUserId)
            throw new UnauthorizedAccessException("You can edit only your own grades.");

        if (!entity.Teacher.TeacherSubjects.Any(ts => ts.SubjectId == entity.SubjectId))
            throw new UnauthorizedAccessException("You are not assigned to this subject.");

        if (entity.Student.SchoolId != entity.Teacher.SchoolId)
            throw new UnauthorizedAccessException("You are not teaching at this student's school.");

        _mapper.Map(dto, entity);

        await _ctx.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id, string teacherUserId)
    {
        var entity = await _ctx.Grades
            .Include(g => g.Teacher)
            .FirstOrDefaultAsync(g => g.Id == id)
            ?? throw new KeyNotFoundException("Grade not found");

        if (entity.Teacher.UserId != teacherUserId)
            throw new UnauthorizedAccessException("You can delete only your own grades.");

        _ctx.Grades.Remove(entity);

        await _ctx.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<GradeDto>> GetGradesForParentAsync(string parentUserId)
    {
        var parent = await _ctx.Parents
            .AsNoTracking()
            .Include(p => p.ParentStudents)
            .SingleOrDefaultAsync(p => p.UserId == parentUserId)
            ?? throw new KeyNotFoundException("Parent not found.");

        var studentIds = parent.ParentStudents.Select(ps => ps.StudentId).ToList();

        return await _ctx.Grades
            .AsNoTracking()
            .Where(g => studentIds.Contains(g.StudentId))
            .ProjectTo<GradeDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<GradeDto?> GetLastGradeAsync(int studentId)
    {
        return await _ctx.Grades
            .AsNoTracking()
            .Where(g => g.StudentId == studentId)
            .OrderByDescending(g => g.CreatedOn)
            .ProjectTo<GradeDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();
    }

    public async Task<List<GradeDto>> GetByStudentAsync(int studentId)
    {
        return await _ctx.Grades
            .AsNoTracking()
            .Where(g => g.StudentId == studentId)
            .ProjectTo<GradeDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }
}