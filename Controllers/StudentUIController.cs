using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using school_diary.Data;
using school_diary.Models;

namespace school_diary.Controllers;

[Authorize(Roles = "Student")]
public class StudentUIController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly ApplicationDbContext _db;

    public StudentUIController(
        UserManager<User> userManager,
        ApplicationDbContext db)
    {
        _userManager = userManager;
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User)!;

        var student = await _db.Students
            .Include(s => s.User)
            .Include(s => s.Class)
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (student == null)
            return NotFound("Student not found.");

        ViewBag.Student = student;

        ViewBag.Grades = await _db.Grades
            .AsNoTracking()
            .Include(g => g.Subject)
            .Where(g => g.StudentId == student.Id)
            .OrderByDescending(g => g.CreatedOn)
            .ToListAsync();

        ViewBag.Absences = await _db.Absences
            .AsNoTracking()
            .Include(a => a.Subject)
            .Where(a => a.StudentId == student.Id)
            .OrderByDescending(a => a.Date)
            .ToListAsync();

        ViewBag.Curriculum = await _db.Curricula
            .Include(c => c.Entries)
            .ThenInclude(e => e.Subject)
            .Include(c => c.Entries)
            .ThenInclude(e => e.Teacher)
            .ThenInclude(t => t.User)
            .Where(c => c.ClassId == student.ClassId)
            .OrderByDescending(c => c.Id)
            .FirstOrDefaultAsync();

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(string email, string? newPassword)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return NotFound();

        if (!string.IsNullOrWhiteSpace(email))
        {
            user.Email = email;
            user.UserName = email;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                TempData["Error"] = string.Join(" ", result.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(Index));
            }
        }

        if (!string.IsNullOrWhiteSpace(newPassword))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (!result.Succeeded)
            {
                TempData["Error"] = string.Join(" ", result.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(Index));
            }
        }

        TempData["Success"] = "Profile updated successfully.";

        return RedirectToAction(nameof(Index));
    }
}