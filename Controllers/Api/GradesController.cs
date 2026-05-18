using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using school_diary.Dtos;
using school_diary.Services;
using school_diary.Extensions;

namespace school_diary.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class GradesController : ControllerBase
{
    private readonly IGradeService _srv;
    public GradesController(IGradeService srv) => _srv = srv;

    [HttpGet]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<IReadOnlyList<GradeDto>>> GetAll()
    {
        var userId = User.GetUserId();

        if (User.IsInRole("Admin"))
            return Ok(await _srv.GetAllAsync());

        return Ok(await _srv.GetAllForTeacherAsync(userId));
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Teacher,Parent")]
    public async Task<ActionResult<GradeDto>> Get(int id)
    {
        var userId = User.GetUserId();

        GradeDto? dto;

        if (User.IsInRole("Admin"))
            dto = await _srv.GetAsync(id);
        else if (User.IsInRole("Teacher"))
            dto = await _srv.GetForTeacherAsync(id, userId);
        else 
            dto = await _srv.GetForParentAsync(id, userId);

        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    [Authorize(Roles = "Teacher")]
    public async Task<ActionResult<GradeDto>> Post(CreateGradeDto dto)
    {
        var created = await _srv.CreateAsync(dto, User.GetUserId());
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Put(int id, UpdateGradeDto dto)
    {
        await _srv.UpdateAsync(id, dto, User.GetUserId());
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        if (User.IsInRole("Admin"))
        {
            await _srv.DeleteAsAdminAsync(id);
            return NoContent();
        }

        await _srv.DeleteAsync(id, User.GetUserId());
        return NoContent();
    }

    [HttpGet("my-children")]
    [Authorize(Roles = "Parent")]
    public async Task<ActionResult<IReadOnlyList<GradeDto>>> GetGradesForMyChildren()
    {
        var userId = User.GetUserId();
        var result = await _srv.GetGradesForParentAsync(userId);
        return Ok(result);
    }
}