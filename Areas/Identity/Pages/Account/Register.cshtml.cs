// ============================================================
//  RegisterModel — Razor Page Code-Behind (Presentation Layer)
//
//  REFACTORED: Authentication logic has been moved OUT of this
//  file and into AuthService (Service Layer). This class only:
//    1. Binds and validates form input (DataAnnotations)
//    2. Delegates to IAuthService
//    3. Handles redirects and ModelState errors
//
//  NO direct calls to UserManager or SignInManager here.
// ============================================================
#nullable disable

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VibeLang.Services;

namespace VibeLang.Areas.Identity.Pages.Account
{
    /// <summary>
    /// Handles the user registration page (GET + POST).
    /// Authentication logic is fully delegated to <see cref="IAuthService"/>.
    /// </summary>
    public class RegisterModel : PageModel
    {
        // ── Dependency: IAuthService injected by DI ──────────────
        //    This is the ONLY Identity-related dependency.
        //    UserManager and SignInManager are hidden behind the interface.
        private readonly IAuthService _authService;

        public RegisterModel(IAuthService authService)
        {
            _authService = authService;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        // ── Input model: validated by DataAnnotations before OnPostAsync runs ──
        public class InputModel
        {
            [Required(ErrorMessage = "First name is required.")]
            [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters.")]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Required(ErrorMessage = "Last name is required.")]
            [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters.")]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [Required(ErrorMessage = "Email address is required.")]
            [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Password is required.")]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm Password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        // GET /Identity/Account/Register
        public IActionResult OnGet(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            return Page();
        }

        // POST /Identity/Account/Register
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            // 1. Validate form input via DataAnnotations
            if (!ModelState.IsValid)
            {
                return Page(); // redisplay form with validation errors
            }

            // 2. Delegate ALL registration logic to the service layer
            //    (UserManager.CreateAsync, role assignment, SignInAsync)
            var result = await _authService.RegisterAsync(
                Input.Email,
                Input.Password,
                Input.FirstName,
                Input.LastName);

            // 3. On success: role-based redirect to the correct dashboard
            if (result.Succeeded)
            {
                // New users always get the "User" role (see AuthService.RegisterAsync).
                // Admins cannot self-register; they must be promoted by another Admin.
                if (string.IsNullOrEmpty(returnUrl) || returnUrl == Url.Content("~/"))
                {
                    return RedirectToAction("Courses", "Home"); // Learner dashboard
                }
                return LocalRedirect(returnUrl);
            }

            // 4. On failure: surface IdentityResult errors back to the form
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }
    }
}
