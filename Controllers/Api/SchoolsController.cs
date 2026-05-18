using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using school_diary.Dtos;
using school_diary.Services;

namespace school_diary.Controllers;

[Authorize(Roles = "Admin")]
public class SchoolsController : Controller
{
    private readonly ISchoolService _schoolService;

    public SchoolsController(ISchoolService schoolService)
    {
        _schoolService = schoolService;
    }

    public async Task<IActionResult> ListSchool()
    {
        var schools = await _schoolService.GetAllAsync();
        return View(schools);
    }

    [HttpGet]
    public IActionResult AddSchool()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddSchool(CreateSchoolDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        await _schoolService.CreateAsync(dto);
        return RedirectToAction(nameof(ListSchool));
    }

    [HttpGet]
    public async Task<IActionResult> EditSchool(int id)
    {
        var school = await _schoolService.GetAsync(id);
        if (school == null)
            return NotFound();

        var dto = new UpdateSchoolDto
        {
            Name = school.Name,
            Address = school.Address
        };

        ViewBag.Id = id;
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditSchool(int id, UpdateSchoolDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Id = id;
            return View(dto);
        }

        await _schoolService.UpdateAsync(id, dto);
        return RedirectToAction(nameof(ListSchool));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSchool(int id)
    {
        await _schoolService.DeleteAsync(id);
        return RedirectToAction(nameof(ListSchool));
    }
}