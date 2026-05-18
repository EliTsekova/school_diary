using school_diary.Dtos;

namespace school_diary.Services;

public interface IParentService
{
    Task<ParentDto?>              GetAsync(int id);
    Task<IReadOnlyList<ParentDto>>GetAllAsync();
    Task<ParentDto>               CreateAsync(CreateParentDto dto);
    Task                          UpdateAsync(int id, UpdateParentDto dto);
    Task                          DeleteAsync(int id);

    // Methods for managing parent-child relationships
    Task AddChildAsync   (int parentId, int studentId);
    Task RemoveChildAsync(int parentId, int studentId);
    Task AssignStudentsAsync(int parentId, List<int> studentIds);
    Task<List<string>> GetStudentNamesForParentAsync(int parentId);
    Task<List<int>> GetStudentIdsForParentAsync(int parentId);
    Task<List<StudentDto>> GetMyStudentsAsync(string userId);



}