using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace school_diary.ViewModels
{
    public class AddStudentViewModel
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required]
        [Display(Name = "School")]
        public int SelectedSchoolId { get; set; }

        [Required]
        [Display(Name = "Class")]
        public string SelectedClassName { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> Schools { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> Classes { get; set; }
    }
}