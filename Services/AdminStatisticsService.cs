using school_diary.Dtos;
using school_diary.Data;
using Microsoft.EntityFrameworkCore;

namespace school_diary.Services;

public class AdminStatisticsService : IAdminStatisticsService
{
    private readonly ApplicationDbContext _ctx;

    public AdminStatisticsService(ApplicationDbContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<IReadOnlyList<SubjectAverageDto>> GetGlobalSubjectAveragesAsync()
    {
        return await _ctx.Grades
            .GroupBy(g => g.Subject.Name)
            .Select(g => new SubjectAverageDto(
                g.Key,
                Math.Round(g.Average(x => x.Value), 2)
            ))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<SubjectAverageDto>> GetSubjectAveragesBySchoolAsync(int schoolId)
    {
        return await _ctx.Grades
            .Where(g => g.Student.SchoolId == schoolId)
            .GroupBy(g => g.Subject.Name)
            .Select(g => new SubjectAverageDto(
                g.Key,
                Math.Round(g.Average(x => x.Value), 2)
            ))
            .ToListAsync();
    }
}