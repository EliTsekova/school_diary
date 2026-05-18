using school_diary.Dtos;

namespace school_diary.Services;

public interface ICurriculumService
{
    Task<CurriculumDto> CreateAsync(CreateCurriculumDto dto);
    Task<IReadOnlyList<CurriculumDto>> GetAllAsync();
    Task<CurriculumDto?> GetAsync(int id);
    Task DeleteAsync(int id);
    
    Task<CurriculumDto?> GetCurrentBySchoolIdAsync(int schoolId);

}