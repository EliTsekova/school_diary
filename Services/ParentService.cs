using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using school_diary.Data;
using school_diary.Dtos;
using school_diary.Models;

namespace school_diary.Services;

public class ParentService : IParentService
{
    private readonly ApplicationDbContext _ctx;
    private readonly IMapper _mapper;
    private readonly UserManager<User> _userManager;

    public ParentService(
        ApplicationDbContext ctx,
        IMapper mapper,
        UserManager<User> userManager)
    {
        _ctx = ctx;
        _mapper = mapper;
        _userManager = userManager;
    }

    public async Task<ParentDto?> GetAsync(int id) =>
        await _ctx.Parents
            .AsNoTracking()
            .Where(p => p.Id == id)
            .ProjectTo<ParentDto>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync();

    public async Task<IReadOnlyList<ParentDto>> GetAllAsync() =>
        await _ctx.Parents
            .AsNoTracking()
            .ProjectTo<ParentDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

    public async Task UpdateAsync(int id, UpdateParentDto dto)
    {
        var entity = await _ctx.Parents
            .Include(p => p.User)
            .SingleOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException("Parent not found");

        if (entity.User == null)
            throw new KeyNotFoundException("Parent user not found");

        entity.User.FirstName = dto.FirstName;
        entity.User.LastName = dto.LastName;
        entity.User.Email = dto.Email;
        entity.User.UserName = dto.Email;
        entity.User.Role = Role.Parent;

        var result = await _userManager.UpdateAsync(entity.User);

        if (!result.Succeeded)
            throw new InvalidOperationException(
                string.Join(", ", result.Errors.Select(e => e.Description)));

        await _ctx.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _ctx.Parents
            .Include(p => p.User)
            .Include(p => p.ParentStudents)
            .SingleOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException("Parent not found");

        var user = entity.User;

        _ctx.ParentStudents.RemoveRange(entity.ParentStudents);
        _ctx.Parents.Remove(entity);

        await _ctx.SaveChangesAsync();

        if (user != null)
        {
            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
                throw new InvalidOperationException(
                    string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }

    public async Task AddChildAsync(int parentId, int studentId)
    {
        var parentExists = await _ctx.Parents
            .AsNoTracking()
            .AnyAsync(p => p.Id == parentId);

        if (!parentExists)
            throw new KeyNotFoundException("Parent not found");

        var studentExists = await _ctx.Students
            .AsNoTracking()
            .AnyAsync(s => s.Id == studentId);

        if (!studentExists)
            throw new KeyNotFoundException("Student not found");

        var exists = await _ctx.ParentStudents
            .AnyAsync(ps => ps.ParentId == parentId && ps.StudentId == studentId);

        if (exists)
            return;

        var parentCount = await _ctx.ParentStudents
            .CountAsync(ps => ps.StudentId == studentId);

        if (parentCount >= 2)
            throw new InvalidOperationException("This student already has two parents.");

        _ctx.ParentStudents.Add(new ParentStudent
        {
            ParentId = parentId,
            StudentId = studentId
        });

        await _ctx.SaveChangesAsync();
    }

    public async Task RemoveChildAsync(int parentId, int studentId)
    {
        var link = await _ctx.ParentStudents
            .SingleOrDefaultAsync(ps => ps.ParentId == parentId && ps.StudentId == studentId);

        if (link is null)
            return;

        _ctx.ParentStudents.Remove(link);

        await _ctx.SaveChangesAsync();
    }

    public async Task AssignStudentsAsync(int parentId, List<int> studentIds)
    {
        studentIds ??= new List<int>();

        var distinctIds = studentIds.Distinct().ToList();

        var parentExists = await _ctx.Parents
            .AsNoTracking()
            .AnyAsync(p => p.Id == parentId);

        if (!parentExists)
            throw new KeyNotFoundException("Parent not found");

        if (distinctIds.Count > 0)
        {
            var existingStudentsCount = await _ctx.Students
                .AsNoTracking()
                .CountAsync(s => distinctIds.Contains(s.Id));

            if (existingStudentsCount != distinctIds.Count)
                throw new KeyNotFoundException("One or more students not found");
        }

        foreach (var studentId in distinctIds)
        {
            var parentCount = await _ctx.ParentStudents
                .CountAsync(ps => ps.StudentId == studentId && ps.ParentId != parentId);

            if (parentCount >= 2)
                throw new InvalidOperationException("This student already has two parents.");
        }

        var existingLinks = await _ctx.ParentStudents
            .Where(ps => ps.ParentId == parentId)
            .ToListAsync();

        _ctx.ParentStudents.RemoveRange(existingLinks);

        foreach (var studentId in distinctIds)
        {
            _ctx.ParentStudents.Add(new ParentStudent
            {
                ParentId = parentId,
                StudentId = studentId
            });
        }

        await _ctx.SaveChangesAsync();
    }

    public async Task<List<string>> GetStudentNamesForParentAsync(int parentId)
    {
        return await _ctx.ParentStudents
            .AsNoTracking()
            .Where(ps => ps.ParentId == parentId)
            .Select(ps => ps.Student.User.FirstName + " " + ps.Student.User.LastName)
            .ToListAsync();
    }

    public async Task<List<int>> GetStudentIdsForParentAsync(int parentId)
    {
        return await _ctx.ParentStudents
            .AsNoTracking()
            .Where(ps => ps.ParentId == parentId)
            .Select(ps => ps.StudentId)
            .ToListAsync();
    }

    public async Task<List<StudentDto>> GetMyStudentsAsync(string userId)
    {
        return await _ctx.ParentStudents
            .AsNoTracking()
            .Where(ps => ps.Parent.UserId == userId)
            .Select(ps => ps.Student)
            .ProjectTo<StudentDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<ParentDto> CreateAsync(CreateParentDto dto)
    {
        var selectedStudentIds = (dto.StudentIds ?? new List<int>())
            .Distinct()
            .ToList();

        foreach (var studentId in selectedStudentIds)
        {
            var parentCount = await _ctx.ParentStudents
                .CountAsync(ps => ps.StudentId == studentId);

            if (parentCount >= 2)
                throw new InvalidOperationException("This student already has two parents.");
        }

        var existingUser = await _userManager.FindByEmailAsync(dto.Email);

        if (existingUser != null)
            throw new InvalidOperationException("A user with this email already exists.");

        var user = new User
        {
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Role = Role.Parent,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
            throw new InvalidOperationException(
                string.Join(", ", result.Errors.Select(e => e.Description)));

        await _userManager.AddToRoleAsync(user, "Parent");

        var parent = new Parent
        {
            UserId = user.Id,
            ParentStudents = new List<ParentStudent>()
        };

        _ctx.Parents.Add(parent);

        await _ctx.SaveChangesAsync();

        foreach (var studentId in selectedStudentIds)
        {
            _ctx.ParentStudents.Add(new ParentStudent
            {
                ParentId = parent.Id,
                StudentId = studentId
            });
        }

        await _ctx.SaveChangesAsync();

        return new ParentDto(
            parent.Id,
            $"{user.FirstName} {user.LastName}",
            user.Email ?? ""
        );
    }
}