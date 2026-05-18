using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using school_diary.Dtos;
using school_diary.Services;

namespace school_diary.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminStatisticsService _stats;
    private readonly ITeacherService _teacherService;
    private readonly IStudentService _studentService;
    private readonly IParentService _parentService;
    private readonly IDirectorService _directorService;
    private readonly ISchoolService _schoolService;
    private readonly ISubjectService _subjectService;

    public AdminController(
        IAdminStatisticsService stats,
        ITeacherService teacherService,
        IStudentService studentService,
        IParentService parentService,
        IDirectorService directorService,
        ISchoolService schoolService,
        ISubjectService subjectService)
    {
        _stats = stats;
        _teacherService = teacherService;
        _studentService = studentService;
        _parentService = parentService;
        _directorService = directorService;
        _schoolService = schoolService;
        _subjectService = subjectService;
    }

    [HttpGet("statistics/global-subject-averages")]
    public async Task<ActionResult<IReadOnlyList<SubjectAverageDto>>> GetGlobalSubjectAverages()
    {
        var result = await _stats.GetGlobalSubjectAveragesAsync();

        return Ok(result);
    }

    [HttpGet("statistics/school/{schoolId}/subject-averages")]
    public async Task<ActionResult<IReadOnlyList<SubjectAverageDto>>> GetBySchool(int schoolId)
    {
        var result = await _stats.GetSubjectAveragesBySchoolAsync(schoolId);

        return Ok(result);
    }

    [HttpPost("create-teacher")]
    public async Task<ActionResult<TeacherDto>> CreateTeacher([FromBody] CreateTeacherDto dto)
    {
        var result = await _teacherService.CreateAsync(dto);

        return CreatedAtAction(nameof(CreateTeacher), result);
    }

    [HttpPost("create-student")]
    public async Task<ActionResult<StudentDto>> CreateStudent([FromBody] CreateStudentDto dto)
    {
        var result = await _studentService.CreateAsync(dto);

        return CreatedAtAction(nameof(CreateStudent), result);
    }

    [HttpPost("create-parent")]
    public async Task<ActionResult<ParentDto>> CreateParent([FromBody] CreateParentDto dto)
    {
        var result = await _parentService.CreateAsync(dto);

        return CreatedAtAction(nameof(CreateParent), result);
    }

    [HttpPost("create-director")]
    public async Task<ActionResult<DirectorDto>> CreateDirector([FromBody] CreateDirectorDto dto)
    {
        var result = await _directorService.CreateAsync(dto);

        return CreatedAtAction(nameof(CreateDirector), result);
    }

    [HttpPost("create-school")]
    public async Task<ActionResult<SchoolDto>> CreateSchool([FromBody] CreateSchoolDto dto)
    {
        var result = await _schoolService.CreateAsync(dto);

        return CreatedAtAction(nameof(CreateSchool), result);
    }

    [HttpPost("create-subject")]
    public async Task<ActionResult<SubjectDto>> CreateSubject([FromBody] SubjectDto dto)
    {
        var result = await _subjectService.CreateSubjectAsync(dto);

        return CreatedAtAction(nameof(CreateSubject), result);
    }
}