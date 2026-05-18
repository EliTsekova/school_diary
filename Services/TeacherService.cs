using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using school_diary.Data;
using school_diary.Dtos;
using school_diary.Extensions;
using school_diary.Models;

namespace school_diary.Services;

public class TeacherService : ITeacherService
{
    private readonly ApplicationDbContext _ctx;
    private readonly IMapper _mapper;
    private readonly UserManager<User> _userManager;

    public TeacherService(ApplicationDbContext ctx, IMapper mapper, UserManager<User> userManager)
    {
        _ctx = ctx;
        _mapper = mapper;
        _userManager = userManager;
    }

    public async Task<TeacherDto?> GetAsync(int id) =>
        await _ctx.Teachers
            .AsNoTracking()
            .Where(t => t.Id == id)
            .ProjectTo<TeacherDto>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync();

    public async Task<IReadOnlyList<TeacherDto>> GetAllAsync() =>
        await _ctx.Teachers
            .AsNoTracking()
            .ProjectTo<TeacherDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

    public async Task<TeacherDto?> GetByUserIdAsync(string userId) =>
        await _ctx.Teachers
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .ProjectTo<TeacherDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

    public async Task<List<int>> GetSubjectIdsForTeacherAsync(int teacherId) =>
        await _ctx.TeacherSubjects
            .AsNoTracking()
            .Where(ts => ts.TeacherId == teacherId)
            .Select(ts => ts.SubjectId)
            .ToListAsync();

    public async Task<List<StudentDto>> GetMyStudentsAsync(int teacherId)
    {
        var teacher = await _ctx.Teachers
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == teacherId);

        if (teacher == null)
            return new List<StudentDto>();

        var classIds = ParseClassIds(teacher.AssignedClasses);

        return await _ctx.Students
            .AsNoTracking()
            .Where(s => s.ClassId.HasValue && classIds.Contains(s.ClassId.Value))
            .ProjectTo<StudentDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<TeacherDto> CreateAsync(CreateTeacherDto dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        var schoolExists = await _ctx.Schools
            .AsNoTracking()
            .AnyAsync(s => s.Id == dto.SchoolId);

        if (!schoolExists)
            throw new KeyNotFoundException("School not found.");

        var subjectIds = (dto.SubjectIds ?? new List<int>()).Distinct().ToList();
        var classIds = (dto.ClassIds ?? new List<int>()).Distinct().ToList();

        if (subjectIds.Count > 0)
        {
            var existingSubjectCount = await _ctx.Subjects
                .AsNoTracking()
                .CountAsync(s => subjectIds.Contains(s.Id));

            if (existingSubjectCount != subjectIds.Count)
                throw new KeyNotFoundException("One or more subjects not found.");
        }

        if (classIds.Count > 0)
        {
            var existingClassCount = await _ctx.Classes
                .AsNoTracking()
                .CountAsync(c => classIds.Contains(c.Id));

            if (existingClassCount != classIds.Count)
                throw new KeyNotFoundException("One or more classes not found.");
        }

        var user = await _userManager.FindByEmailAsync(dto.Email);

        if (user is null)
        {
            user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                UserName = dto.Email,
                Role = Role.Teacher,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            result.CheckErrors();
        }

        if (await _ctx.Teachers.AsNoTracking().AnyAsync(t => t.UserId == user.Id))
            throw new InvalidOperationException("User is already a teacher.");

        if (await _ctx.Students.AsNoTracking().AnyAsync(s => s.UserId == user.Id) ||
            await _ctx.Parents.AsNoTracking().AnyAsync(p => p.UserId == user.Id) ||
            await _ctx.Directors.AsNoTracking().AnyAsync(d => d.UserId == user.Id))
            throw new InvalidOperationException("User is already assigned to another role profile.");

        if (!await _userManager.IsInRoleAsync(user, "Teacher"))
        {
            var roleResult = await _userManager.AddToRoleAsync(user, "Teacher");
            roleResult.CheckErrors();
        }

        var teacher = new Teacher
        {
            UserId = user.Id,
            SchoolId = dto.SchoolId,

            AssignedClasses = string.Join(",",
                dto.ClassIds ?? new List<int>()),

            TeacherSubjects = subjectIds
                .Select(sid => new TeacherSubject
                {
                    SubjectId = sid
                })
                .ToList()
        };

        _ctx.Teachers.Add(teacher);
        await _ctx.SaveChangesAsync();

        return await GetAsync(teacher.Id)
               ?? throw new KeyNotFoundException("Teacher not found.");
    }

    public async Task<TeacherDto> UpdateAsync(int id, UpdateTeacherDto dto)
    {
        var teacher = await _ctx.Teachers
            .Include(t => t.TeacherSubjects)
            .SingleOrDefaultAsync(t => t.Id == id)
            ?? throw new KeyNotFoundException("Teacher not found.");

        var user = await _userManager.FindByIdAsync(teacher.UserId)
            ?? throw new KeyNotFoundException("Teacher user not found.");

        var subjectIds = (dto.SubjectIds ?? new List<int>()).Distinct().ToList();
        var classIds = (dto.ClassIds ?? new List<int>()).Distinct().ToList();

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.Email = dto.Email;
        user.UserName = dto.Email;
        user.Role = Role.Teacher;

        var updateUserResult = await _userManager.UpdateAsync(user);
        updateUserResult.CheckErrors();

        if (!string.IsNullOrWhiteSpace(dto.NewPassword))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var passwordResult = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);
            passwordResult.CheckErrors();
        }

        teacher.SchoolId = dto.SchoolId;
        teacher.AssignedClasses = string.Join(",",
            dto.ClassIds ?? new List<int>());

        teacher.TeacherSubjects.Clear();

        foreach (var subjectId in subjectIds)
        {
            teacher.TeacherSubjects.Add(new TeacherSubject
            {
                TeacherId = teacher.Id,
                SubjectId = subjectId
            });
        }

        await _ctx.SaveChangesAsync();

        return await GetAsync(id)
               ?? throw new KeyNotFoundException("Teacher not found.");
    }

    public async Task DeleteAsync(int id)
    {
        var teacher = await _ctx.Teachers
                          .Include(t => t.TeacherSubjects)
                          .SingleOrDefaultAsync(t => t.Id == id)
                      ?? throw new KeyNotFoundException("Teacher not found.");

        var user = await _userManager.FindByIdAsync(teacher.UserId);

        var curriculumEntries = await _ctx.CurriculumEntries
            .Where(ce => ce.TeacherId == id)
            .ToListAsync();

        _ctx.CurriculumEntries.RemoveRange(curriculumEntries);

        var grades = await _ctx.Grades
            .Where(g => g.TeacherId == id)
            .ToListAsync();

        foreach (var grade in grades)
        {
            grade.TeacherId = null;
        }

        _ctx.TeacherSubjects.RemoveRange(teacher.TeacherSubjects);

        _ctx.Teachers.Remove(teacher);

        await _ctx.SaveChangesAsync();

        if (user != null)
        {
            var result = await _userManager.DeleteAsync(user);
            result.CheckErrors();
        }
    }

    public async Task<TeacherDto> AddSubjectsAsync(int id, IReadOnlyList<int> subjectIds)
    {
        var teacher = await _ctx.Teachers
            .Include(t => t.TeacherSubjects)
            .SingleOrDefaultAsync(t => t.Id == id)
            ?? throw new KeyNotFoundException("Teacher not found.");

        var ids = (subjectIds ?? Array.Empty<int>()).Distinct().ToList();

        teacher.TeacherSubjects.Clear();

        foreach (var subjectId in ids)
        {
            teacher.TeacherSubjects.Add(new TeacherSubject
            {
                TeacherId = teacher.Id,
                SubjectId = subjectId
            });
        }

        await _ctx.SaveChangesAsync();

        return await GetAsync(id)
               ?? throw new KeyNotFoundException("Teacher not found.");
    }

    private static List<int> ParseClassIds(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new List<int>();

        return value
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => int.TryParse(x, out var id) ? id : 0)
            .Where(id => id > 0)
            .Distinct()
            .ToList();
    }
}