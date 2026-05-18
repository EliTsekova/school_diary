using Microsoft.EntityFrameworkCore;
using school_diary.Data;

namespace school_diary.Services;

public class StudentNameService : IStudentNameService
{
    private readonly ApplicationDbContext _ctx;

    public StudentNameService(ApplicationDbContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<List<string>> GetStudentNamesByParentIdAsync(int parentId)
    {
        return await _ctx.ParentStudents
            .Where(ps => ps.ParentId == parentId)
            .Include(ps => ps.Student)
            .ThenInclude(s => s.User)
            .Select(ps => ps.Student.User.FirstName + " " + ps.Student.User.LastName)
            .ToListAsync();
    }
}