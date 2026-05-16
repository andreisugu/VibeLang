// Licensed to the .NET Foundation under one or more agreements.
// Adapted from ASP.NET Core Identity scaffolded template.
// Logout logic delegated to IAuthService (service layer).
#nullable disable

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VibeLang.Services;

namespace VibeLang.Areas.Identity.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly IAuthService _authService;

        public LogoutModel(IAuthService authService)
        {
            _authService = authService;
        }

        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            // Delegate sign-out logic to the service layer
            await _authService.LogoutAsync();

            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                // Redirect to home page after logout
                return RedirectToPage("/Index");
            }
        }
    }
}
