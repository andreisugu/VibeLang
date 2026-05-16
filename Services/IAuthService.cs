using Microsoft.AspNetCore.Identity;
using VibeLang.Models;

namespace VibeLang.Services;

/// <summary>
/// Defines the authentication service contract for user registration and login operations.
/// Extracted from Razor Page code-behind to follow the Service Layer pattern.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new user account with the given credentials and profile information.
    /// Assigns the default "User" role after successful creation.
    /// </summary>
    /// <param name="email">The user's email address (used as username).</param>
    /// <param name="password">The chosen password (validated against Identity options).</param>
    /// <param name="firstName">Optional first name for the user profile.</param>
    /// <param name="lastName">Optional last name for the user profile.</param>
    /// <returns>An <see cref="IdentityResult"/> indicating success or containing errors.</returns>
    Task<IdentityResult> RegisterAsync(string email, string password, string? firstName, string? lastName);

    /// <summary>
    /// Signs in an existing user with the provided credentials.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <param name="password">The user's password.</param>
    /// <param name="rememberMe">Whether to persist the authentication cookie across sessions.</param>
    /// <returns>A <see cref="SignInResult"/> indicating success or the reason for failure.</returns>
    Task<SignInResult> LoginAsync(string email, string password, bool rememberMe);

    /// <summary>
    /// Signs out the currently authenticated user.
    /// </summary>
    Task LogoutAsync();

    /// <summary>
    /// Returns the currently signed-in <see cref="ApplicationUser"/>, or null if not authenticated.
    /// </summary>
    /// <param name="principal">The current <see cref="System.Security.Claims.ClaimsPrincipal"/>.</param>
    Task<ApplicationUser?> GetCurrentUserAsync(System.Security.Claims.ClaimsPrincipal principal);
}
