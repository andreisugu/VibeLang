// ============================================================
//  LogoutModel — Razor Page Code-Behind (Presentation Layer)
//
//  REFACTORED: Sign-out logic moved to AuthService.LogoutAsync().
//  This class only handles the POST request and redirect.
//  No direct SignInManager dependency here.
// ============================================================
#nullable disable

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VibeLang.Services;

namespace VibeLang.Areas.Identity.Pages.Account
{
    /// <summary>
    /// Handles the logout POST request.
    /// Delegates sign-out to <see cref="IAuthService.LogoutAsync"/>.
    /// </summary>
    public class LogoutModel : PageModel
    {
        // ── Dependency: IAuthService injected by DI ──────────────
        private readonly IAuthService _authService;

        public LogoutModel(IAuthService authService)
        {
            _authService = authService;
        }

        // POST /Identity/Account/Logout
        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            // Delegate sign-out to the service layer (SignInManager.SignOutAsync inside)
            await _authService.LogoutAsync();

            // Redirect to returnUrl if provided, otherwise go to landing page
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
