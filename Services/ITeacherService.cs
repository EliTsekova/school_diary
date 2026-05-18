using school_diary.Dtos;

namespace school_diary.Services;

public interface ITeacherService
{
    Task<TeacherDto?>                 GetAsync (int id);
    Task<IReadOnlyList<TeacherDto>>   GetAllAsync();

    Task<TeacherDto> CreateAsync(CreateTeacherDto dto);
    Task<TeacherDto> UpdateAsync(int id, UpdateTeacherDto dto);
    Task DeleteAsync(int id);
    
    Task<TeacherDto> AddSubjectsAsync(int id, IReadOnlyList<int> subjectIds);
    
    Task<List<int>> GetSubjectIdsForTeacherAsync(int teacherId);
    Task<TeacherDto?> GetByUserIdAsync(string userId);
    Task<List<StudentDto>> GetMyStudentsAsync(int teacherId);



}