using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using school_diary.Data;
using school_diary.Dtos;
using school_diary.Services;
using System.ComponentModel.DataAnnotations;

namespace school_diary.Views.AdminUi;

public class AddParentModel : PageModel
{
    private readonly IParentService _parentService;
    private readonly ApplicationDbContext _context;

    public AddParentModel(
        IParentService parentService,
        ApplicationDbContext context)
    {
        _parentService = parentService;
        _context = context;
    }

    [BindProperty]
    public ParentInputModel Input { get; set; } = new();

    public List<SelectListItem> StudentOptions { get; set; } = new();

    public async Task OnGetAsync()
    {
        await LoadStudentsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadStudentsAsync();

        if (!ModelState.IsValid)
            return Page();

        var createDto = new CreateParentDto(
            Input.FirstName,
            Input.LastName,
            Input.Email,
            Input.Password,
            Input.SelectedStudentIds ?? new List<int>()
        );

        await _parentService.CreateAsync(createDto);

        TempData["SuccessMessage"] = "The parent has been created successfully!";

        return RedirectToPage("Index");
    }

    private async Task LoadStudentsAsync()
    {
        var students = await _context.Students
            .Include(s => s.User)
            .Include(s => s.Class)
            .ToListAsync();

        StudentOptions = students
            .Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = $"{s.User.FirstName} {s.User.LastName} ({s.Class?.Name ?? "No class"})"
            })
            .ToList();
    }

    public class ParentInputModel
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "The password must be at least 6 characters long.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select at least one student.")]
        public List<int>? SelectedStudentIds { get; set; }
    }
}