namespace school_diary.Services;
using school_diary.Dtos;

public interface IStudentService
{
    Task<StudentDto?> GetAsync(int id);
    Task<StudentDto?> GetByUserIdAsync(string userId);
    Task<CreatedStudentDto> CreateAsync(CreateStudentDto dto);
    Task UpdateAsync(int id, UpdateStudentDto dto);
    Task DeleteAsync(int id);

    Task<List<StudentDto>> GetAllAsync();
    Task CreateRecordAsync(CreateStudentDto dto);
}