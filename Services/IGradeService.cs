using school_diary.Dtos;

namespace school_diary.Services;

public interface IGradeService
{
    Task<GradeDto?> GetAsync(int id);
    Task<IReadOnlyList<GradeDto>> GetAllAsync();

    Task<GradeDto?> GetForTeacherAsync(int id, string teacherUserId);
    Task<IReadOnlyList<GradeDto>> GetAllForTeacherAsync(string teacherUserId);

    Task<GradeDto?> GetForParentAsync(int id, string parentUserId);

    Task<GradeDto> CreateAsync(CreateGradeDto dto, string currentUserId);

    Task UpdateAsync(int id, UpdateGradeDto dto, string teacherUserId);

    Task DeleteAsync(int id, string teacherUserId);
    Task DeleteAsAdminAsync(int id);

    Task<IReadOnlyList<GradeDto>> GetGradesForParentAsync(string parentUserId);

    Task<GradeDto?> GetLastGradeAsync(int studentId);
    Task<List<GradeDto>> GetByStudentAsync(int studentId);
}