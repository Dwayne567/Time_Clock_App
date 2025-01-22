using System.ComponentModel.DataAnnotations;

namespace FT_TTMS_WebApplication.ViewModels
{
    // ViewModel for Forgot Password
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}