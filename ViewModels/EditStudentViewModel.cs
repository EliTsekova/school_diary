using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace school_diary.ViewModels
{
    public class EditStudentViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "New Password (optional)")]
        public string? NewPassword { get; set; }

        [Required(ErrorMessage = "Please select a school")]
        [Display(Name = "School")]
        public int? SelectedSchoolId { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> Schools { get; set; }

        [Required(ErrorMessage = "Please select a class")]
        [Display(Name = "Class")]
        public string SelectedClassName { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> Classes { get; set; }
    }
}