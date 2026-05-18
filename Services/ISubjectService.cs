using school_diary.Dtos;
using System.Threading.Tasks;

namespace school_diary.Services;


public interface ISubjectService
{
    Task<SubjectDto> CreateSubjectAsync(SubjectDto dto);
    Task<IReadOnlyList<SubjectDto>> GetAllAsync();
    Task<List<SubjectDto>> GetByIdsAsync(IEnumerable<int> ids);
    Task<Dictionary<int, string>> GetNamesByIdsAsync(IEnumerable<int> subjectIds);


}
