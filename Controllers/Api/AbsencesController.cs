using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using school_diary.Dtos;
using school_diary.Extensions;
using school_diary.Services;

namespace school_diary.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class AbsencesController : ControllerBase
{
    private readonly IAbsenceService _srv;
    public AbsencesController(IAbsenceService srv) => _srv = srv;

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Teacher")]
    public async Task<ActionResult<AbsenceDto>> Get(int id)
    {
        var dto = await _srv.GetForTeacherAsync(id, User.GetUserId());
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpGet]
    [Authorize(Roles = "Teacher")]
    public async Task<ActionResult<IReadOnlyList<AbsenceDto>>> GetAll()
        => Ok(await _srv.GetAllForTeacherAsync(User.GetUserId()));

    [HttpPost]
    [Authorize(Roles = "Teacher")]
    public async Task<ActionResult<AbsenceDto>> Post(CreateAbsenceDto dto)
    {
        var created = await _srv.CreateAsync(dto, User.GetUserId());
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Put(int id, UpdateAbsenceDto dto)
    {
        await _srv.UpdateAsync(id, dto, User.GetUserId());
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Delete(int id)
    {
        await _srv.DeleteAsync(id, User.GetUserId());
        return NoContent();
    }

    [HttpGet("my-children")]
    [Authorize(Roles = "Parent")]
    public async Task<ActionResult<IReadOnlyList<AbsenceDto>>> GetAbsencesForMyChildren()
        => Ok(await _srv.GetAbsencesForParentAsync(User.GetUserId()));
}