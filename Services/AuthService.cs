using Microsoft.AspNetCore.Identity;
using VibeLang.Models;

namespace VibeLang.Services;

/// <summary>
/// Implements <see cref="IAuthService"/> using ASP.NET Core Identity's
/// <see cref="UserManager{TUser}"/> and <see cref="SignInManager{TUser}"/>.
/// This service decouples authentication logic from Razor Page code-behind files.
/// </summary>
public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<IdentityResult> RegisterAsync(
        string email,
        string password,
        string? firstName,
        string? lastName)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName
        };

        var result = await _userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            // Assign the default "User" role to every newly registered account.
            // The "User" role must be seeded at startup (see Program.cs).
            await _userManager.AddToRoleAsync(user, "User");
            _logger.LogInformation("New user {Email} registered and assigned the 'User' role.", email);

            // Sign in immediately after registration
            await _signInManager.SignInAsync(user, isPersistent: false);
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<SignInResult> LoginAsync(string email, string password, bool rememberMe)
    {
        var result = await _signInManager.PasswordSignInAsync(
            email,
            password,
            isPersistent: rememberMe,
            lockoutOnFailure: false);

        if (result.Succeeded)
        {
            _logger.LogInformation("User {Email} logged in successfully.", email);
        }
        else
        {
            _logger.LogWarning("Failed login attempt for {Email}.", email);
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User signed out.");
    }

    /// <inheritdoc/>
    public async Task<ApplicationUser?> GetCurrentUserAsync(System.Security.Claims.ClaimsPrincipal principal)
    {
        return await _userManager.GetUserAsync(principal);
    }

    /// <inheritdoc/>
    public async Task<string?> GetUserRoleAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return null;

        // GetRolesAsync returns all roles; we return the first one (priority: Admin > User)
        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Contains("Admin")) return "Admin";
        if (roles.Count > 0) return roles[0];
        return null;
    }
}
