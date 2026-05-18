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
        var userId = _userManager.GetUserId(User)!;

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
        ViewBag.Children = parent.ParentStudents.Select(ps => ps.Student).ToList();

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
}