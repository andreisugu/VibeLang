using Microsoft.AspNetCore.Identity;
using VibeLang.Models;

namespace VibeLang.Services;

// ============================================================
//  IAuthService — Authentication Service Contract
//  Extracted from Razor Page code-behind to enforce the
//  Service Layer pattern (Separation of Concerns).
//
//  BEFORE refactoring: Login / Register / Logout logic lived
//    directly inside LoginModel.OnPostAsync(),
//    RegisterModel.OnPostAsync(), and LogoutModel.OnPost().
//
//  AFTER refactoring: Razor pages only handle HTTP concerns
//    (binding, redirects, ModelState). All Identity operations
//    are delegated to this service via Dependency Injection.
// ============================================================

/// <summary>
/// Defines the authentication service contract.
/// Injected into Razor Page models via the constructor to decouple
/// authentication logic from the presentation layer.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new user account with the provided credentials.
    /// Internally calls <c>UserManager.CreateAsync</c> and assigns
    /// the default <c>"User"</c> role, then signs the user in immediately.
    /// </summary>
    /// <param name="email">Email address used as username.</param>
    /// <param name="password">Chosen password — validated against <c>IdentityOptions.Password</c>.</param>
    /// <param name="firstName">Optional first name stored on <see cref="ApplicationUser"/>.</param>
    /// <param name="lastName">Optional last name stored on <see cref="ApplicationUser"/>.</param>
    /// <returns>
    /// An <see cref="IdentityResult"/> indicating success or a list of validation errors
    /// (e.g. duplicate email, password too short).
    /// </returns>
    Task<IdentityResult> RegisterAsync(string email, string password, string? firstName, string? lastName);

    /// <summary>
    /// Authenticates an existing user using their email and password.
    /// Internally calls <c>SignInManager.PasswordSignInAsync</c>.
    /// </summary>
    /// <param name="email">The user's registered email address.</param>
    /// <param name="password">The user's password (compared against the hashed store).</param>
    /// <param name="rememberMe">
    /// When <c>true</c>, issues a persistent cookie that survives browser restarts.
    /// </param>
    /// <returns>
    /// A <see cref="SignInResult"/> with <c>Succeeded</c>, <c>IsLockedOut</c>,
    /// or <c>IsNotAllowed</c> flags.
    /// </returns>
    Task<SignInResult> LoginAsync(string email, string password, bool rememberMe);

    /// <summary>
    /// Signs out the currently authenticated user by clearing their session cookie.
    /// Internally calls <c>SignInManager.SignOutAsync</c>.
    /// </summary>
    Task LogoutAsync();

    /// <summary>
    /// Resolves the <see cref="ApplicationUser"/> from the current HTTP request's
    /// <see cref="System.Security.Claims.ClaimsPrincipal"/>.
    /// Returns <c>null</c> if the user is not authenticated.
    /// </summary>
    /// <param name="principal">The <c>User</c> property from a <c>Controller</c> or <c>PageModel</c>.</param>
    Task<ApplicationUser?> GetCurrentUserAsync(System.Security.Claims.ClaimsPrincipal principal);

    /// <summary>
    /// Returns the primary role name for a given user email.
    /// Priority: <c>"Admin"</c> is returned first if the user holds that role.
    /// Used after login to perform role-based dashboard redirection.
    /// Returns <c>null</c> if the user is not found or has no roles assigned.
    /// </summary>
    /// <param name="email">The user's registered email address.</param>
    Task<string?> GetUserRoleAsync(string email);
}
