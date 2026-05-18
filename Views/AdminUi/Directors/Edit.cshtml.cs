using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using school_diary.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace school_diary.Views.AdminUi.Directors;

public class EditModel : PageModel
{
    private readonly ApplicationDbContext _ctx;

    public EditModel(ApplicationDbContext ctx)
    {
        _ctx = ctx;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    [BindProperty]
    public DirectorInputModel Input { get; set; } = new();

    public List<SelectListItem> SchoolOptions { get; set; } = new();

    public class DirectorInputModel
    {
        [Required]
        public string FirstName { get; set; } = "";

        [Required]
        public string LastName { get; set; } = "";

        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        public int? SchoolId { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var director = await _ctx.Directors
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == Id);

        if (director == null)
            return NotFound();

        Input = new DirectorInputModel
        {
            FirstName = director.User.FirstName,
            LastName = director.User.LastName,
            Email = director.User.Email,
            SchoolId = director.SchoolId
        };

        await LoadSchoolsAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadSchoolsAsync();
            return Page();
        }

        var director = await _ctx.Directors
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == Id);

        if (director == null)
            return NotFound();

        director.User.FirstName = Input.FirstName;
        director.User.LastName = Input.LastName;
        director.User.Email = Input.Email;
        director.User.UserName = Input.Email;
        director.SchoolId = Input.SchoolId ?? 0;

        await _ctx.SaveChangesAsync();

        return RedirectToPage("Index");
    }

    private async Task LoadSchoolsAsync()
    {
        SchoolOptions = await _ctx.Schools
            .Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = s.Name
            })
            .ToListAsync();
    }
}