namespace school_diary.Services;

using school_diary.Dtos;

public interface ISchoolService
{
    Task<SchoolDto?> GetAsync   (int id);
    Task<IReadOnlyList<SchoolDto>> GetAllAsync();
    Task<SchoolDto>  CreateAsync(CreateSchoolDto dto);
    Task             UpdateAsync(int id, UpdateSchoolDto dto);
    Task             DeleteAsync(int id);
    Task<SchoolDto?> GetForDirectorAsync(string directorUserId);
}