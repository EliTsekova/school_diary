using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using school_diary.Data;
using school_diary.Dtos;
using school_diary.Models;

namespace school_diary.Services;

public class StudentService : IStudentService
{
    private readonly ApplicationDbContext _ctx;
    private readonly IMapper _mapper;
    private readonly UserManager<User> _userManager;

    public StudentService(ApplicationDbContext ctx, IMapper mapper, UserManager<User> userManager)
    {
        _ctx = ctx;
        _mapper = mapper;
        _userManager = userManager;
    }

    public async Task<StudentDto?> GetAsync(int id)
        => await _ctx.Students
            .Include(s => s.User)
            .ProjectTo<StudentDto>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(s => s.Id == id);

    public async Task<StudentDto> CreateAsync(CreateStudentDto dto)
    {
        var user = new User
        {
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            EmailConfirmed = true,
            Role = Role.Student
        };

        var result = await _userManager.CreateAsync(user, "Student123!");

        if (!result.Succeeded)
        {
            var errorMsg = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new ApplicationException($"User creation failed: {errorMsg}");
        }

        var student = new Student
        {
            ClassName = dto.ClassName,
            SchoolId = dto.SchoolId,
            UserId = user.Id
        };
        

        _ctx.Students.Add(student);
        await _ctx.SaveChangesAsync();

        return _mapper.Map<StudentDto>(student);
    }

    public async Task UpdateAsync(int id, UpdateStudentDto dto)
    {
        var entity = await _ctx.Students
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == id)
            ?? throw new KeyNotFoundException("Student not found");

        entity.ClassName = dto.ClassName;
        entity.SchoolId = dto.SchoolId;
        entity.User.FirstName = dto.FirstName;
        entity.User.LastName = dto.LastName;
        entity.User.Email = dto.Email;

        await _ctx.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _ctx.Students.FindAsync(id)
            ?? throw new KeyNotFoundException("Student not found");

        _ctx.Students.Remove(entity);
        await _ctx.SaveChangesAsync();
    }
}
