using school_diary.Dtos;
namespace school_diary.Services;
public interface IDirectorStatisticsService
{
    Task<IReadOnlyList<SubjectAverageDto>> GetAverageGradesPerSubjectAsync(int schoolId);
    Task<IReadOnlyList<TeacherAverageDto>> GetAverageGradesPerTeacherAsync(int schoolId);
    Task<IReadOnlyList<ClassAbsenceDto>> GetAbsencesByClassAsync(int schoolId);
    Task<IReadOnlyList<ClassAverageDto>> GetAverageGradesPerClassAsync(int schoolId);

}