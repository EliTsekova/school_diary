using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using school_diary.Data;
using school_diary.Dtos;
using school_diary.Extensions;
using school_diary.Models;
using school_diary.Services;

namespace school_diary.Controllers;

[Authorize(Roles = "Teacher")]
public class TeacherUIController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly ITeacherService _teacherService;
    private readonly IGradeService _gradeService;
    private readonly IAbsenceService _absenceService;
    private readonly ISubjectService _subjectService;
    private readonly ApplicationDbContext _db;

    public TeacherUIController(
        UserManager<User> userManager,
        ITeacherService teacherService,
        IGradeService gradeService,
        IAbsenceService absenceService,
        ISubjectService subjectService,
        ApplicationDbContext db)
    {
        _userManager = userManager;
        _teacherService = teacherService;
        _gradeService = gradeService;
        _absenceService = absenceService;
        _subjectService = subjectService;
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User)!;
        var teacher = await _teacherService.GetByUserIdAsync(userId);

        if (teacher == null)
            return NotFound("Teacher not found.");

        ViewBag.Teacher = teacher;
        ViewBag.Students = await _teacherService.GetMyStudentsAsync(teacher.Id);
        ViewBag.Subjects = await _subjectService.GetByIdsAsync(teacher.SubjectIds);

        ViewBag.Grades = await _db.Grades
            .AsNoTracking()
            .Include(g => g.Student)
                .ThenInclude(s => s.User)
            .Include(g => g.Subject)
            .Where(g => g.TeacherId == teacher.Id)
            .OrderByDescending(g => g.CreatedOn)
            .ToListAsync();

        ViewBag.Absences = await _db.Absences
            .AsNoTracking()
            .Include(a => a.Student)
                .ThenInclude(s => s.User)
            .Include(a => a.Subject)
            .Where(a => teacher.SubjectIds.Contains(a.SubjectId))
            .OrderByDescending(a => a.Date)
            .ToListAsync();

        return View();
    }

    public class EditProfileInputModel
    {
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddGrade(CreateGradeDto dto)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Invalid grade data.";
            return RedirectToAction(nameof(Index));
        }

        var userId = _userManager.GetUserId(User)!;

        await _gradeService.CreateAsync(dto, userId);

        TempData["Success"] = "The grade has been added.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditGrade(int id, int value)
    {
        if (value < 2 || value > 6)
        {
            TempData["Message"] = "The grade must be between 2 and 6.";
            return RedirectToAction(nameof(Index));
        }

        await _gradeService.UpdateAsync(id, new UpdateGradeDto(value), User.GetUserId());

        TempData["Message"] = "The grade has been updated successfully.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteGrade(int id)
    {
        var userId = _userManager.GetUserId(User)!;

        await _gradeService.DeleteAsync(id, userId);

        TempData["Success"] = "The grade has been deleted.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddAbsence(CreateAbsenceDto dto)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Invalid absence data.";
            return RedirectToAction(nameof(Index));
        }

        var userId = _userManager.GetUserId(User)!;

        await _absenceService.CreateAsync(dto, userId);

        TempData["Success"] = "The absence has been added.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAbsence(int id)
    {
        var userId = _userManager.GetUserId(User)!;

        await _absenceService.DeleteAsync(id, userId);

        TempData["Success"] = "The absence has been deleted.";

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

        TempData["Success"] = "The password has been changed successfully.";

        return RedirectToAction(nameof(Index));
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile(EditProfileInputModel input)
    {
        if (string.IsNullOrWhiteSpace(input.FullName) ||
            string.IsNullOrWhiteSpace(input.Email))
        {
            TempData["Error"] = "Name and email are required.";
            return RedirectToAction(nameof(Index));
        }

        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return NotFound();

        var names = input.FullName.Split(' ', 2);

        user.FirstName = names.ElementAtOrDefault(0) ?? "";
        user.LastName = names.ElementAtOrDefault(1) ?? "";
        user.Email = input.Email;
        user.UserName = input.Email;
        user.Role = Role.Teacher;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            TempData["Error"] = string.Join(" ", result.Errors.Select(e => e.Description));
            return RedirectToAction(nameof(Index));
        }

        TempData["Success"] = "Profile updated successfully.";

        return RedirectToAction(nameof(Index));
    }
}