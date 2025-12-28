using System.ComponentModel.DataAnnotations;

namespace Timeclock_WebApplication.ViewModels
{
    // ViewModel for Register
    public class RegisterViewModel
    {
        [Display(Name = "Email address")]
        [Required(ErrorMessage = "Email address is required")]
        public string EmailAddress { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Confirm password")]
        [Required(ErrorMessage = "Confirm password is required")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password do not match")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "First Name")]
        [Required(ErrorMessage = "First name is required")]
        public string? FirstName { get; set; }

        [Display(Name = "Last Name")]
        [Required(ErrorMessage = "Last name is required")]
        public string? LastName { get; set; }

        [Display(Name = "Employee Number")]
        [Required(ErrorMessage = "Employee number is required")]
        public int? EmployeeNumber { get; set; }

        [Display(Name = "Group")]
        [Required(ErrorMessage = "Group is required")]
        public string? Group { get; set; }
    }
}
