using Timeclock_WebApplication.Data;
using Timeclock_WebApplication.Models;
using Timeclock_WebApplication.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Timeclock_WebApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public AccountController(UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        // POST: api/Account/Login
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel loginViewModel)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(loginViewModel.EmailAddress);

            if (user != null)
            {
                // Check password
                var passwordCheck = await _userManager.CheckPasswordAsync(user, loginViewModel.Password);
                if (passwordCheck)
                {
                    // Sign in the user
                    var result = await _signInManager.PasswordSignInAsync(user, loginViewModel.Password, false, false);
                    if (result.Succeeded)
                    {
                        var roles = await _userManager.GetRolesAsync(user);
                        return Ok(new { 
                            Message = "Login successful", 
                            UserId = user.Id, 
                            Roles = roles,
                            IsAdmin = roles.Contains(UserRoles.Admin)
                        });
                    }
                }
                return Unauthorized("Wrong credentials. Please try again.");
            }

            return Unauthorized("Wrong credentials. Please try again.");
        }

        // POST: api/Account/Register
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel registerViewModel)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(registerViewModel.EmailAddress);
            if (user != null)
            {
                return BadRequest("This email address is already in use");
            }

            var newUser = new AppUser()
            {
                Email = registerViewModel.EmailAddress,
                UserName = registerViewModel.EmailAddress,
                FirstName = registerViewModel.FirstName,
                LastName = registerViewModel.LastName,
                EmployeeNumber = registerViewModel.EmployeeNumber,
                Group = registerViewModel.Group,
                EmailConfirmed = true // Auto-confirm email
            };

            var newUserResponse = await _userManager.CreateAsync(newUser, registerViewModel.Password);

            if (newUserResponse.Succeeded)
            {
                await _userManager.AddToRoleAsync(newUser, UserRoles.User);
                return Ok("Registration successful. You can now log in.");
            }

            foreach (var error in newUserResponse.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return BadRequest(ModelState);
        }

        // POST: api/Account/Logout
        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { message = "Logged out successfully" });
        }
    }
}
