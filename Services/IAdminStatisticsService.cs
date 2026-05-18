using school_diary.Dtos;
namespace school_diary.Services;

public interface IAdminStatisticsService
{
    Task<IReadOnlyList<SubjectAverageDto>> GetGlobalSubjectAveragesAsync();
    Task<IReadOnlyList<SubjectAverageDto>> GetSubjectAveragesBySchoolAsync(int schoolId);
}
