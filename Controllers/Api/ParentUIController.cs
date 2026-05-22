using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using school_diary.Data;
using school_diary.Models;

namespace school_diary.Controllers.Api;

[Authorize(Roles = "Parent")]
public class ParentUIController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly ApplicationDbContext _db;

    public ParentUIController(UserManager<User> userManager, ApplicationDbContext db)
    {
        _userManager = userManager;
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var parent = await _db.Parents
            .Include(p => p.User)
            .Include(p => p.ParentStudents)
                .ThenInclude(ps => ps.Student)
                    .ThenInclude(s => s.User)
            .Include(p => p.ParentStudents)
                .ThenInclude(ps => ps.Student)
                    .ThenInclude(s => s.Class)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (parent == null)
            return NotFound("Parent not found.");

        var studentIds = parent.ParentStudents
            .Select(ps => ps.StudentId)
            .ToList();

        ViewBag.Parent = parent;
        ViewBag.Children = parent.ParentStudents
            .Select(ps => ps.Student)
            .ToList();

        ViewBag.Grades = await _db.Grades
            .AsNoTracking()
            .Include(g => g.Student)
                .ThenInclude(s => s.User)
            .Include(g => g.Subject)
            .Where(g => studentIds.Contains(g.StudentId))
            .OrderByDescending(g => g.CreatedOn)
            .ToListAsync();

        ViewBag.Absences = await _db.Absences
            .AsNoTracking()
            .Include(a => a.Student)
                .ThenInclude(s => s.User)
            .Include(a => a.Subject)
            .Where(a => studentIds.Contains(a.StudentId))
            .OrderByDescending(a => a.Date)
            .ToListAsync();

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(
        string fullName,
        string email,
        string? newPassword,
        string? confirmPassword)
    {
        if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email))
        {
            TempData["Error"] = "Name and email are required.";
            return RedirectToAction(nameof(Index));
        }

        if (!string.IsNullOrWhiteSpace(newPassword) && newPassword != confirmPassword)
        {
            TempData["Error"] = "The passwords do not match.";
            return RedirectToAction(nameof(Index));
        }

        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return Unauthorized();

        var names = fullName.Trim()
            .Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);

        user.FirstName = names.ElementAtOrDefault(0) ?? "";
        user.LastName = names.ElementAtOrDefault(1) ?? "";
        user.Email = email.Trim();
        user.UserName = email.Trim();
        user.NormalizedEmail = _userManager.NormalizeEmail(email.Trim());
        user.NormalizedUserName = _userManager.NormalizeName(email.Trim());
        user.Role = Role.Parent;

        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            TempData["Error"] = string.Join(" ", updateResult.Errors.Select(e => e.Description));
            return RedirectToAction(nameof(Index));
        }

        if (!string.IsNullOrWhiteSpace(newPassword))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var passwordResult = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (!passwordResult.Succeeded)
            {
                TempData["Error"] = string.Join(" ", passwordResult.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(Index));
            }
        }

        TempData["Success"] = "Profile updated successfully.";
        return RedirectToAction(nameof(Index));
    }
}