namespace school_diary.Services;
using school_diary.Dtos;

public interface IDirectorService
{
    Task<IReadOnlyList<DirectorDto>> GetAllAsync();
    Task<DirectorDto?>  GetAsync(int id);
    Task<DirectorDto>   CreateAsync(CreateDirectorDto dto);
    Task                ChangeSchoolAsync(int id, int schoolId);
    Task                DeleteAsync(int id);
    Task<int> GetSchoolIdByUserId(string userId);
    Task UpdateAsync(int id, UpdateDirectorDto dto);


}