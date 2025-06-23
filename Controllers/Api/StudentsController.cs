namespace school_diary.Controllers.Api;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using school_diary.Dtos;
using school_diary.Services;

[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _srv;
    public StudentsController(IStudentService srv) => _srv = srv;

    // GET /api/students/5
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Director,Teacher,Parent,Student")]
    public async Task<ActionResult<StudentDto>> Get(int id)
        => await _srv.GetAsync(id) is { } s ? Ok(s) : NotFound();

    // POST /api/students
    [HttpPost]
    [Authorize(Roles = "Admin,Director")]
    public async Task<ActionResult<StudentDto>> Post(CreateStudentDto dto)
    {
        var created = await _srv.CreateAsync(dto);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    // PUT /api/students/5
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Director")]
    public async Task<IActionResult> Put(int id, UpdateStudentDto dto)
    {
        await _srv.UpdateAsync(id, dto);
        return NoContent();
    }

    // DELETE /api/students/5
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Director")]
    public async Task<IActionResult> Delete(int id)
    {
        await _srv.DeleteAsync(id);
        return NoContent();
    }
}