using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using school_diary.Dtos;
using school_diary.Services;
using System.ComponentModel.DataAnnotations;

namespace school_diary.Views.AdminUi
{
    public class AddTeacherModel : PageModel
    {
        private readonly ITeacherService _teacherService;
        private readonly ISchoolService _schoolService;

        public AddTeacherModel(
            ITeacherService teacherService,
            ISchoolService schoolService)
        {
            _teacherService = teacherService;
            _schoolService = schoolService;
        }

        [BindProperty]
        public TeacherInputModel Input { get; set; } = new();

        public List<SelectListItem> SchoolOptions { get; set; } = new();

        public async Task OnGetAsync()
        {
            await LoadSchoolsAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadSchoolsAsync();

            if (!ModelState.IsValid)
                return Page();

            var subjectIds = ParseIds(Input.SubjectIds);
            var classIds = ParseIds(Input.ClassIds);

            var dto = new CreateTeacherDto(
                Input.FirstName,
                Input.LastName,
                Input.Email,
                Input.Password,
                Input.SchoolId,
                subjectIds,
                classIds
            );

            try
            {
                await _teacherService.CreateAsync(dto);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return Page();
            }

            TempData["SuccessMessage"] = "The teacher has been created successfully!";
            return RedirectToPage("Index");
        }

        private async Task LoadSchoolsAsync()
        {
            var schools = await _schoolService.GetAllAsync();

            SchoolOptions = schools
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                })
                .ToList();
        }

        private static List<int> ParseIds(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return new List<int>();

            return value
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => int.TryParse(id.Trim(), out var parsed) ? parsed : -1)
                .Where(id => id > 0)
                .Distinct()
                .ToList();
        }

        public class TeacherInputModel
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

            [Required]
            public int SchoolId { get; set; }

            public string SubjectIds { get; set; } = string.Empty;

            public string ClassIds { get; set; } = string.Empty;
        }
    }
}