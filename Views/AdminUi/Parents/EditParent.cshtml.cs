using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using school_diary.Dtos;
using school_diary.Services;
using System.ComponentModel.DataAnnotations;

namespace school_diary.Views.AdminUi.Parents;

public class EditModel : PageModel
{
    private readonly IParentService _parentService;
    private readonly IStudentService _studentService;

    public EditModel(IParentService parentService, IStudentService studentService)
    {
        _parentService = parentService;
        _studentService = studentService;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    [BindProperty]
    public ParentInputModel Input { get; set; } = new();

    public List<SelectListItem> StudentOptions { get; set; } = new();

    public class ParentInputModel
    {
        [Required]
        public string FirstName { get; set; } = "";

        [Required]
        public string LastName { get; set; } = "";

        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        public List<int> SelectedStudentIds { get; set; } = new();
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var parent = await _parentService.GetAsync(Id);

        if (parent == null)
            return NotFound();

        var names = parent.FullName.Split(' ', 2);

        Input = new ParentInputModel
        {
            FirstName = names[0],
            LastName = names.Length > 1 ? names[1] : "",
            Email = parent.Email,
            SelectedStudentIds = await _parentService.GetStudentIdsForParentAsync(Id)
        };

        await LoadStudentsAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadStudentsAsync();
            return Page();
        }

        var updateDto = new UpdateParentDto(
            Input.FirstName,
            Input.LastName,
            Input.Email
        );

        await _parentService.UpdateAsync(Id, updateDto);
        await _parentService.AssignStudentsAsync(Id, Input.SelectedStudentIds);

        return RedirectToPage("/AdminUi/Parents/Index");
    }

    private async Task LoadStudentsAsync()
    {
        var allStudents = await _studentService.GetAllAsync();

        StudentOptions = allStudents.Select(s => new SelectListItem
        {
            Value = s.Id.ToString(),
            Text = s.FullName,
            Selected = Input.SelectedStudentIds.Contains(s.Id)
        }).ToList();
    }
}