using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using school_diary.Dtos;
using school_diary.Services;
using System.ComponentModel.DataAnnotations;

namespace school_diary.Views.AdminUi
{
    public class AddSubjectModel : PageModel
    {
        private readonly ISubjectService _subjectService;

        public AddSubjectModel(ISubjectService subjectService)
        {
            _subjectService = subjectService;
        }

        [BindProperty]
        public SubjectInputModel Input { get; set; } = new();

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var dto = new SubjectDto(0, Input.Name);

            await _subjectService.CreateSubjectAsync(dto);

            TempData["SuccessMessage"] = "Subject added successfully!";

            return RedirectToPage("Index");
        }

        public class SubjectInputModel
        {
            [Required]
            public string Name { get; set; } = string.Empty;
        }
    }
}