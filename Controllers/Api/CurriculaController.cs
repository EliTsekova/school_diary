using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using school_diary.Dtos;
using school_diary.Services;

namespace school_diary.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Director")]
public class CurriculaController : ControllerBase
{
    private readonly ICurriculumService _srv;
    public CurriculaController(ICurriculumService srv) => _srv = srv;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CurriculumDto>>> GetAll() =>
        Ok(await _srv.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CurriculumDto>> Get(int id) =>
        await _srv.GetAsync(id) is { } dto ? Ok(dto) : NotFound();

    [HttpPost]
    public async Task<ActionResult<CurriculumDto>> Post(CreateCurriculumDto dto)
    {
        var created = await _srv.CreateAsync(dto);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _srv.DeleteAsync(id);
        return NoContent();
    }
}