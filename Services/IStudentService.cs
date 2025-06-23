namespace school_diary.Services;
using school_diary.Dtos;

public interface IStudentService
{
    Task<StudentDto?> GetAsync(int id);
    Task<StudentDto>  CreateAsync(CreateStudentDto dto);
    Task UpdateAsync(int id, UpdateStudentDto dto);
    Task DeleteAsync(int id);
}