using FT_TTMS_WebApplication.Data;
using FT_TTMS_WebApplication.Interfaces;
using FT_TTMS_WebApplication.Models;
using FT_TTMS_WebApplication.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace FT_TTMS_WebApplication.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _context;

        public AccountController(UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager, IEmailSender emailSender, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _context = context;
        }

        // GET: Login
        [HttpGet]
        public IActionResult Login()
        {
            var response = new LoginViewModel();
            return View();
        }

        // POST: Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel)
        {
            if (!ModelState.IsValid) return View(loginViewModel);

            var user = await _userManager.FindByEmailAsync(loginViewModel.EmailAddress);

            if (user != null)
            {
                // Check if the email is confirmed
                if (!await _userManager.IsEmailConfirmedAsync(user))
                {
                    TempData["Error"] = "You need to confirm your email before logging in.";
                    return View(loginViewModel);
                }

                // Check password
                var passwordCheck = await _userManager.CheckPasswordAsync(user, loginViewModel.Password);
                if (passwordCheck)
                {
                    // Sign in the user
                    var result = await _signInManager.PasswordSignInAsync(user, loginViewModel.Password, false, false);
                    if (result.Succeeded)
                    {
                        var roles = await _userManager.GetRolesAsync(user);
                        if (roles.Contains(UserRoles.Admin))
                        {
                            return RedirectToAction("Index", "Admin");
                        }
                        else
                        {
                            return RedirectToAction("Index", "Dashboard", new { userId = user.Id });
                        }
                    }
                }
                // Password is incorrect
                TempData["Error"] = "Wrong credentials. Please try again.";
                return View(loginViewModel);
            }

            // User not found
            TempData["Error"] = "Wrong credentials. Please try again.";
            return View(loginViewModel);
        }

        // GET: Register
        [HttpGet]
        public IActionResult Register()
        {
            var response = new RegisterViewModel();
            return View(response);
        }

        // POST: Register
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
        {
            if (!ModelState.IsValid) return View(registerViewModel);

            var user = await _userManager.FindByEmailAsync(registerViewModel.EmailAddress);
            if (user != null)
            {
                TempData["Error"] = "This email address is already in use";
                return View(registerViewModel);
            }

            var newUser = new AppUser()
            {
                Email = registerViewModel.EmailAddress,
                UserName = registerViewModel.EmailAddress,
                FirstName = registerViewModel.FirstName,
                LastName = registerViewModel.LastName,
                EmployeeNumber = registerViewModel.EmployeeNumber,
                Group = registerViewModel.Group
            };

            var newUserResponse = await _userManager.CreateAsync(newUser, registerViewModel.Password);
            _context.Passwords.Add(new Passwords { EmailAddress = newUser.Email, Password = registerViewModel.Password });
            await _context.SaveChangesAsync();

            if (newUserResponse.Succeeded)
            {
                await _userManager.AddToRoleAsync(newUser, UserRoles.User);

                // Send confirmation email
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
                var encodedToken = WebUtility.UrlEncode(token); // URL-encode the token
                var confirmationLink = Url.Action("ConfirmEmail", "Account", new { userId = newUser.Id, token = encodedToken }, protocol: HttpContext.Request.Scheme);

                // Create the email body with a hyperlink
                var emailBody = $"Please confirm your email by clicking this link: <a href='{confirmationLink}'>Confirm your email</a>";

                await _emailSender.SendEmailAsync(registerViewModel.EmailAddress, "Confirm your email", emailBody);

                return RedirectToAction("RegisterConfirmation");
            }

            // Handle errors if user creation fails
            foreach (var error in newUserResponse.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(registerViewModel);
        }

        // GET: Register Confirmation
        [HttpGet]
        public IActionResult RegisterConfirmation()
        {
            return View();
        }

        // GET: Confirm Email
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                // Log error: missing userId or token
                Console.WriteLine("ConfirmEmail: Missing userId or token.");
                return View("Error");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                // Log error: user not found
                Console.WriteLine($"ConfirmEmail: User with ID {userId} not found.");
                return RedirectToAction("Index", "Home");
            }

            // Decode the token if it was URL-encoded
            var decodedToken = WebUtility.UrlDecode(token);

            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
            if (result.Succeeded)
            {
                return View(true); // Email confirmed successfully
            }

            // Log error: confirmation failed
            Console.WriteLine($"ConfirmEmail: Email confirmation failed for user ID {userId} with token {token}. Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            return View(false); // Email confirmation failed
        }

        // GET: Logout
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        // GET: Forgot Password
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: Forgot Password
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Inform the user that the account does not exist
                ModelState.AddModelError(string.Empty, "No account found with that email.");
                return View(model);
            }
            
            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                // Inform the user that the email is not confirmed
                ModelState.AddModelError(string.Empty, "Email not confirmed.");
                return View(model);
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Action(nameof(ResetPassword), "Account", new { token, email = model.Email }, protocol: Request.Scheme);
            await _emailSender.SendEmailAsync(model.Email, "Reset Password",
                $"Please reset your password by clicking here: <a href='{callbackUrl}'>link</a>");

            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        // GET: Forgot Password Confirmation
        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        // GET: Reset Password
        [HttpGet]
        public IActionResult ResetPassword(string token = null)
        {
            if (token == null)
            {
                return View("Error");
            }

            var model = new ResetPasswordViewModel { Token = token };
            return View(model);
        }

        // POST: Reset Password
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Instead of redirecting, add an error message to the ModelState
                ModelState.AddModelError(string.Empty, "No account found with that email address.");
                return View(model); // Return the view with the model to display the error
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            _context.Passwords.Add(new Passwords { EmailAddress = user.Email, Password = model.Password });
            await _context.SaveChangesAsync();
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            foreach (var error in result.Errors)
            {
                if (error.Code == "InvalidToken") // This is an example; the actual code may vary
                {
                    ModelState.AddModelError(string.Empty, "The provided information is incorrect.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model); // Make sure to return the model here as well for consistency
        }

        // GET: Reset Password Confirmation
        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }
    }
}