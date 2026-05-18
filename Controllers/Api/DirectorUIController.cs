using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using school_diary.Data;
using school_diary.Models;
using school_diary.Services;

namespace school_diary.Controllers.Api;

[Authorize(Roles = "Director")]
public class DirectorUIController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly IDirectorService _directorService;
    private readonly IDirectorStatisticsService _statisticsService;
    private readonly ISchoolService _schoolService;
    private readonly ApplicationDbContext _db;

    public DirectorUIController(
        UserManager<User> userManager,
        IDirectorService directorService,
        IDirectorStatisticsService statisticsService,
        ISchoolService schoolService,
        ApplicationDbContext db)
    {
        _userManager = userManager;
        _directorService = directorService;
        _statisticsService = statisticsService;
        _schoolService = schoolService;
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User)!;
        var schoolId = await _directorService.GetSchoolIdByUserId(userId);

        ViewBag.Director = await _db.Directors
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.UserId == userId);

        ViewBag.School = await _schoolService.GetAsync(schoolId);

        ViewBag.Students = await _db.Students
            .AsNoTracking()
            .Include(s => s.User)
            .Include(s => s.Class)
            .Where(s => s.SchoolId == schoolId)
            .OrderBy(s => s.User.FirstName)
            .ThenBy(s => s.User.LastName)
            .ToListAsync();

        ViewBag.Teachers = await _db.Teachers
            .AsNoTracking()
            .Include(t => t.User)
            .Where(t => t.SchoolId == schoolId)
            .OrderBy(t => t.User.FirstName)
            .ThenBy(t => t.User.LastName)
            .ToListAsync();

        ViewBag.Parents = await _db.Parents
            .AsNoTracking()
            .Include(p => p.User)
            .Include(p => p.ParentStudents)
                .ThenInclude(ps => ps.Student)
                    .ThenInclude(s => s.User)
            .Where(p => p.ParentStudents.Any(ps => ps.Student.SchoolId == schoolId))
            .OrderBy(p => p.User.FirstName)
            .ThenBy(p => p.User.LastName)
            .ToListAsync();

        ViewBag.SubjectAverages = await _statisticsService.GetAverageGradesPerSubjectAsync(schoolId);
        ViewBag.TeacherAverages = await _statisticsService.GetAverageGradesPerTeacherAsync(schoolId);
        ViewBag.ClassAverages = await _statisticsService.GetAverageGradesPerClassAsync(schoolId);
        ViewBag.ClassAbsences = await _statisticsService.GetAbsencesByClassAsync(schoolId);

        return View();
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            TempData["Error"] = "Email is required.";
            return RedirectToAction(nameof(Index));
        }

        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return NotFound();

        user.Email = email;
        user.UserName = email;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            TempData["Error"] = string.Join(" ", result.Errors.Select(e => e.Description));
            return RedirectToAction(nameof(Index));
        }

        TempData["Success"] = "Profile updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(string newPassword, string confirmPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword))
        {
            TempData["Error"] = "Enter a new password.";
            return RedirectToAction(nameof(Index));
        }

        if (newPassword != confirmPassword)
        {
            TempData["Error"] = "The passwords do not match.";
            return RedirectToAction(nameof(Index));
        }

        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return NotFound();

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

        if (!result.Succeeded)
        {
            TempData["Error"] = string.Join(" ", result.Errors.Select(e => e.Description));
            return RedirectToAction(nameof(Index));
        }

        TempData["Success"] = "Password changed successfully.";
        return RedirectToAction(nameof(Index));
    }
}