using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace school_diary.Controllers;

[Authorize]
public class ProfileController : Controller
{
    public IActionResult EditProfile()
    {
        return View();
    }
}