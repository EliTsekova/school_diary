using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace school_diary.Controllers;

[Authorize]
public class RoleRedirectController : Controller
{
    public IActionResult Index()
    {
        if (User.IsInRole("Admin"))
            return RedirectToAction("Index", "AdminUi");

        if (User.IsInRole("Director"))
            return RedirectToAction("Index", "DirectorUI");

        if (User.IsInRole("Teacher"))
            return RedirectToAction("Index", "TeacherUI");

        if (User.IsInRole("Parent"))
            return RedirectToAction("Index", "ParentUI");

        if (User.IsInRole("Student"))
            return RedirectToAction("Index", "StudentUI");

        return RedirectToAction("Index", "Home");
    }
}