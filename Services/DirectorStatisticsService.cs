using Microsoft.EntityFrameworkCore;
using school_diary.Data;
using school_diary.Dtos;

namespace school_diary.Services;

public class DirectorStatisticsService : IDirectorStatisticsService
{
    private readonly ApplicationDbContext _ctx;

    public DirectorStatisticsService(ApplicationDbContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<IReadOnlyList<SubjectAverageDto>> GetAverageGradesPerSubjectAsync(int schoolId)
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

    public async Task<IReadOnlyList<TeacherAverageDto>> GetAverageGradesPerTeacherAsync(int schoolId)
    {
        return await _ctx.Grades
            .Where(g => g.Student.SchoolId == schoolId)
            .GroupBy(g => new { g.Teacher.UserId, g.Teacher.User.FirstName, g.Teacher.User.LastName })
            .Select(g => new TeacherAverageDto(
                g.Key.UserId,
                g.Key.FirstName + " " + g.Key.LastName,
                Math.Round(g.Average(x => x.Value), 2)
            ))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<ClassAbsenceDto>> GetAbsencesByClassAsync(int schoolId)
    {
        var grouped =
            from a in _ctx.Absences
            where a.Student.SchoolId == schoolId
            group a by a.Student.ClassId into grp
            select new
            {
                ClassId = grp.Key,
                Count = grp.Count()
            };

        var result =
            from g in grouped
            join c in _ctx.Classes on g.ClassId equals c.Id into cs
            from c in cs.DefaultIfEmpty()
            select new ClassAbsenceDto(
                c != null ? c.Name : "Unassigned",
                g.Count
            );

        return await result.ToListAsync();
    }

    public async Task<IReadOnlyList<ClassAverageDto>> GetAverageGradesPerClassAsync(int schoolId)
    {
        var grouped =
            from g in _ctx.Grades
            where g.Student.SchoolId == schoolId
            group g by g.Student.ClassId into grp
            select new
            {
                ClassId = grp.Key,
                Avg = Math.Round(grp.Average(x => x.Value), 2)
            };

        var result =
            from g in grouped
            join c in _ctx.Classes on g.ClassId equals c.Id into cs
            from c in cs.DefaultIfEmpty()
            select new ClassAverageDto(
                c != null ? c.Name : "Unassigned",
                g.Avg
            );

        return await result.ToListAsync();
    }
}