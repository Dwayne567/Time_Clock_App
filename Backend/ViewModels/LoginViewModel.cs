using System.ComponentModel.DataAnnotations;

namespace Timeclock_WebApplication.ViewModels
{
    // ViewModel for Login
    public class LoginViewModel
    {
        [Display(Name = "Email Address")]
        [Required(ErrorMessage = "Email address is required")]
        public string EmailAddress { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
