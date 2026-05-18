using school_diary.Dtos;

namespace school_diary.Services;

public interface IAbsenceService
{
    Task<AbsenceDto?> GetForTeacherAsync(int id, string teacherUserId);
    Task<IReadOnlyList<AbsenceDto>> GetAllForTeacherAsync(string teacherUserId);

    Task<AbsenceDto> CreateAsync(CreateAbsenceDto dto, string teacherUserId);
    Task<AbsenceDto> UpdateAsync(int id, UpdateAbsenceDto dto, string teacherUserId);
    Task DeleteAsync(int id, string teacherUserId);

    Task<IReadOnlyList<AbsenceDto>> GetAbsencesForParentAsync(string parentUserId);

    Task<int> GetCountAsync(int studentId);
    Task<List<AbsenceDto>> GetByStudentAsync(int studentId);
}