using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using school_diary.Services;

namespace school_diary.Controllers;

[Authorize(Roles = "Admin")]
public class StatisticsController : Controller
{
    private readonly IAdminStatisticsService _statisticsService;

    public StatisticsController(IAdminStatisticsService statisticsService)
    {
        _statisticsService = statisticsService;
    }

    public async Task<IActionResult> Overview(int? schoolId)
    {
        if (schoolId.HasValue)
            ViewBag.SubjectAverages = await _statisticsService.GetSubjectAveragesBySchoolAsync(schoolId.Value);
        else
            ViewBag.SubjectAverages = await _statisticsService.GetGlobalSubjectAveragesAsync();

        return View();
    }
}