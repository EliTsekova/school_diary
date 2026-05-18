using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using school_diary.Data;
using school_diary.Dtos;
using school_diary.Models;

namespace school_diary.Services
{
    public class StudentService : IStudentService
    {
        private readonly ApplicationDbContext _ctx;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public StudentService(
            ApplicationDbContext ctx,
            IMapper mapper,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _ctx = ctx;
            _mapper = mapper;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        private async Task<Class?> ResolveClassAsync(int schoolId, string? className)
        {
            if (string.IsNullOrWhiteSpace(className))
                return null;

            className = className.Trim();

            var existing = await _ctx.Classes
                .SingleOrDefaultAsync(c => c.SchoolId == schoolId && c.Name == className);

            if (existing != null)
                return existing;

            var created = new Class
            {
                SchoolId = schoolId,
                Name = className
            };

            _ctx.Classes.Add(created);
            return created;
        }

        public async Task<CreatedStudentDto> CreateAsync(CreateStudentDto dto)
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

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new ApplicationException($"User creation failed: {errors}");
            }

            const string studentRole = "Student";
            if (!await _roleManager.RoleExistsAsync(studentRole))
                await _roleManager.CreateAsync(new IdentityRole(studentRole));

            await _userManager.AddToRoleAsync(user, studentRole);

            var school = await _ctx.Schools.FindAsync(dto.SchoolId)
                         ?? throw new KeyNotFoundException("School not found");

            var @class = await ResolveClassAsync(dto.SchoolId, dto.ClassName);

            var student = new Student
            {
                UserId = user.Id,
                SchoolId = dto.SchoolId,
                School = school,
                Class = @class
            };

            _ctx.Students.Add(student);
            await _ctx.SaveChangesAsync();

            var classLabel = @class?.Name ?? "Unassigned";

            return new CreatedStudentDto(
                student.Id,
                $"{user.FirstName} {user.LastName}",
                user.Email!,
                student.SchoolId!.Value,
                classLabel,
                dto.Password
            );
        }

        public async Task<List<StudentDto>> GetAllAsync()
        {
            return await _ctx.Students
                .Include(s => s.User)
                .Include(s => s.Class)
                .ProjectTo<StudentDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<StudentDto?> GetAsync(int id)
        {
            return await _ctx.Students
                .Include(s => s.User)
                .Include(s => s.Class)
                .ProjectTo<StudentDto>(_mapper.ConfigurationProvider)
                .SingleOrDefaultAsync(s => s.Id == id);
        }

        public async Task<StudentDto?> GetByUserIdAsync(string userId)
        {
            return await _ctx.Students
                .AsNoTracking()
                .Where(s => s.UserId == userId)
                .ProjectTo<StudentDto>(_mapper.ConfigurationProvider)
                .SingleOrDefaultAsync();
        }

        public async Task UpdateAsync(int id, UpdateStudentDto dto)
        {
            var entity = await _ctx.Students
                .Include(s => s.User)
                .Include(s => s.Class)
                .FirstOrDefaultAsync(s => s.Id == id)
                ?? throw new KeyNotFoundException("Student not found");

            var school = await _ctx.Schools.FindAsync(dto.SchoolId)
                         ?? throw new KeyNotFoundException("School not found");

            entity.SchoolId = dto.SchoolId;
            entity.School = school;

            var @class = await ResolveClassAsync(dto.SchoolId, dto.ClassName);
            entity.Class = @class;
            entity.ClassId = @class?.Id;

            entity.User.FirstName = dto.FirstName;
            entity.User.LastName = dto.LastName;
            entity.User.Email = dto.Email;
            entity.User.UserName = dto.Email;

            await _ctx.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _ctx.Students.FindAsync(id)
                         ?? throw new KeyNotFoundException("Student not found");

            _ctx.Students.Remove(entity);
            await _ctx.SaveChangesAsync();
        }

        public async Task CreateRecordAsync(CreateStudentDto dto)
        {
            await CreateAsync(dto);
        }
    }
}