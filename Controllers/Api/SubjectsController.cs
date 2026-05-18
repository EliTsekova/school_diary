using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using school_diary.Dtos;
using school_diary.Services;

namespace school_diary.Controllers;

[Authorize(Roles = "Admin")]
public class SubjectsController : Controller
{
    private readonly ISubjectService _subjectService;

    public SubjectsController(ISubjectService subjectService)
    {
        _subjectService = subjectService;
    }

    public async Task<IActionResult> ListSubject()
    {
        var subjects = await _subjectService.GetAllAsync();
        return View(subjects);
    }

    [HttpGet]
    public IActionResult AddSubject()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddSubject(SubjectDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        await _subjectService.CreateSubjectAsync(dto);
        return RedirectToAction(nameof(ListSubject));
    }
}