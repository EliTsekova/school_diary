namespace school_diary.Services;

public interface IStudentNameService
{
    Task<List<string>> GetStudentNamesByParentIdAsync(int parentId);
}