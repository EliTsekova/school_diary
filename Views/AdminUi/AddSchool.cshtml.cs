using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using school_diary.Dtos;
using school_diary.Services;

namespace school_diary.Views.AdminUi
{
    public class AddSchoolModel : PageModel
    {
        private readonly ISchoolService _schoolService;

        public AddSchoolModel(ISchoolService schoolService)
        {
            _schoolService = schoolService;
        }

        [BindProperty]
        public CreateSchoolDto Input { get; set; } = new();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            await _schoolService.CreateAsync(Input);

            TempData["SuccessMessage"] = "School added successfully!";

            return RedirectToPage("Index");
        }
    }
}
