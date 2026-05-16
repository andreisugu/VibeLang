using Microsoft.AspNetCore.Identity;
using VibeLang.Models;

namespace VibeLang.Services;

// ============================================================
//  AuthService — Authentication Service Implementation
//
//  This class is the single point of truth for all Identity
//  operations in VibeLang. It is registered in Program.cs as:
//
//      builder.Services.AddScoped<IAuthService, AuthService>();
//
//  Razor Pages (Login, Register, Logout) receive this service
//  via Constructor Injection and never reference UserManager
//  or SignInManager directly, ensuring full separation between
//  the presentation layer and the Identity infrastructure.
// ============================================================

/// <summary>
/// Implements <see cref="IAuthService"/> using ASP.NET Core Identity's
/// <see cref="UserManager{TUser}"/> and <see cref="SignInManager{TUser}"/>.
///
/// <para>
/// <b>Separation of Concerns:</b> Razor Page code-behind files
/// (<c>LoginModel</c>, <c>RegisterModel</c>, <c>LogoutModel</c>)
/// contain <em>zero</em> Identity API calls. They only interact with
/// this service through the <see cref="IAuthService"/> interface.
/// </para>
/// </summary>
public class AuthService : IAuthService
{
    // Identity infrastructure — injected via DI, never newed up manually.
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<AuthService> _logger;

    /// <summary>
    /// Constructor — receives Identity services via Dependency Injection.
    /// </summary>
    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    // ─────────────────────────────────────────────────────────
    //  REGISTER
    // ─────────────────────────────────────────────────────────

    /// <inheritdoc/>
    /// <remarks>
    /// Steps performed:
    /// <list type="number">
    ///   <item>Creates the <see cref="ApplicationUser"/> entity.</item>
    ///   <item>Calls <c>UserManager.CreateAsync</c> to hash the password and persist the user.</item>
    ///   <item>On success: assigns the <c>"User"</c> role via <c>UserManager.AddToRoleAsync</c>.</item>
    ///   <item>Signs the new user in immediately via <c>SignInManager.SignInAsync</c>.</item>
    ///   <item>On failure: returns the <see cref="IdentityResult"/> errors to the caller.</item>
    /// </list>
    /// </remarks>
    public async Task<IdentityResult> RegisterAsync(
        string email,
        string password,
        string? firstName,
        string? lastName)
    {
        // Build the user entity — UserName is set to email (Identity standard)
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName?.Trim(),
            LastName = lastName?.Trim()
        };

        // Persist user + hashed password via Identity
        var result = await _userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            // Assign the default "User" role. The role is seeded at startup in Program.cs.
            await _userManager.AddToRoleAsync(user, "User");
            _logger.LogInformation(
                "New account created for {Email}. Role 'User' assigned.", email);

            // Sign in immediately so the user lands on the dashboard without a separate login step.
            await _signInManager.SignInAsync(user, isPersistent: false);
            _logger.LogInformation("User {Email} signed in after registration.", email);
        }
        else
        {
            // Log each Identity validation error (e.g. duplicate email, weak password)
            foreach (var error in result.Errors)
            {
                _logger.LogWarning(
                    "Registration failed for {Email}: [{Code}] {Description}",
                    email, error.Code, error.Description);
            }
        }

        return result;
    }

    // ─────────────────────────────────────────────────────────
    //  LOGIN
    // ─────────────────────────────────────────────────────────

    /// <inheritdoc/>
    /// <remarks>
    /// Calls <c>SignInManager.PasswordSignInAsync</c> which:
    /// <list type="bullet">
    ///   <item>Looks up the user by username (email).</item>
    ///   <item>Verifies the password hash.</item>
    ///   <item>Issues the authentication cookie on success.</item>
    ///   <item>Returns <see cref="SignInResult.Failed"/> without revealing why (security).</item>
    /// </list>
    /// Lockout is disabled here; it can be enabled via <c>lockoutOnFailure: true</c>
    /// and configured through <c>IdentityOptions.Lockout</c> in Program.cs.
    /// </remarks>
    public async Task<SignInResult> LoginAsync(string email, string password, bool rememberMe)
    {
        var result = await _signInManager.PasswordSignInAsync(
            userName: email,
            password: password,
            isPersistent: rememberMe,
            lockoutOnFailure: false);

        if (result.Succeeded)
        {
            _logger.LogInformation("User {Email} authenticated successfully.", email);
        }
        else if (result.IsLockedOut)
        {
            _logger.LogWarning("User {Email} account is locked out.", email);
        }
        else
        {
            _logger.LogWarning("Failed login attempt for {Email}.", email);
        }

        return result;
    }

    // ─────────────────────────────────────────────────────────
    //  LOGOUT
    // ─────────────────────────────────────────────────────────

    /// <inheritdoc/>
    /// <remarks>
    /// Calls <c>SignInManager.SignOutAsync</c> which clears the
    /// authentication cookie from the response, effectively ending
    /// the user's session.
    /// </remarks>
    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("A user has signed out.");
    }

    // ─────────────────────────────────────────────────────────
    //  HELPERS
    // ─────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ApplicationUser?> GetCurrentUserAsync(
        System.Security.Claims.ClaimsPrincipal principal)
    {
        // Resolves the user from the ClaimsPrincipal (reads the NameIdentifier claim).
        return await _userManager.GetUserAsync(principal);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Admin role is checked first so that accounts holding both
    /// Admin and User roles are correctly classified as Admin.
    /// </remarks>
    public async Task<string?> GetUserRoleAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            _logger.LogWarning("GetUserRoleAsync: user not found for email {Email}.", email);
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);

        // Priority: Admin takes precedence over any other role
        if (roles.Contains("Admin")) return "Admin";
        if (roles.Count > 0) return roles[0];

        _logger.LogWarning("User {Email} has no roles assigned.", email);
        return null;
    }
}
