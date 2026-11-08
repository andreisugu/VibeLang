using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using VibeLang.Models;

namespace VibeLang.Browser.Services;

/// <summary>
/// Real ASP.NET Core Identity users and password hashes, with a browser session
/// (localStorage user id) instead of the server's authentication cookie.
/// </summary>
public class BrowserAuthService
{
    private const string Key = "vibelang-user-id";
    private readonly UserManager<ApplicationUser> _users;
    private readonly VibeLangDbContext _db;
    private readonly IJSRuntime _js;
    private readonly SnapshotService _snapshot;
    private Task? _initTask;

    public BrowserAuthService(UserManager<ApplicationUser> users, VibeLangDbContext db, IJSRuntime js, SnapshotService snapshot)
    {
        _users = users;
        _db = db;
        _js = js;
        _snapshot = snapshot;
    }

    public ApplicationUser? User { get; private set; }
    public bool IsSignedIn => User is not null;
    public bool IsAdmin { get; private set; }
    public event Action? Changed;

    public Task InitializeAsync() => _initTask ??= LoadAsync();

    private async Task LoadAsync()
    {
        var id = await _js.InvokeAsync<string?>("localStorage.getItem", Key);
        if (string.IsNullOrWhiteSpace(id)) return;

        User = await _users.FindByIdAsync(id);
        if (User is not null)
        {
            IsAdmin = await IsUserAdminAsync(User);
            Changed?.Invoke();
        }
    }

    public async Task<ApplicationUser?> GetUserAsync()
    {
        await InitializeAsync();
        return User;
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        var user = await _users.FindByEmailAsync(email.Trim());
        if (user is null) return false;
        if (!await _users.CheckPasswordAsync(user, password)) return false;

        await SetUserAsync(user);
        return true;
    }

    public async Task<(bool Ok, string? Error)> RegisterAsync(string email, string password, string? firstName, string? lastName)
    {
        var user = new ApplicationUser
        {
            UserName = email.Trim(),
            Email = email.Trim(),
            EmailConfirmed = true,
            FirstName = firstName?.Trim(),
            LastName = lastName?.Trim(),
        };

        var result = await _users.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            return (false, string.Join(" ", result.Errors.Select(e => e.Description)));
        }

        var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == "User");
        if (role is not null)
        {
            _db.UserRoles.Add(new IdentityUserRole<string> { UserId = user.Id, RoleId = role.Id });
            await _db.SaveChangesAsync();
        }

        await SetUserAsync(user);
        return (true, null);
    }

    public void NotifyChanged() => Changed?.Invoke();

    public async Task LogoutAsync()
    {
        User = null;
        IsAdmin = false;
        await _js.InvokeVoidAsync("localStorage.removeItem", Key);
        Changed?.Invoke();
    }

    private async Task<bool> IsUserAdminAsync(ApplicationUser user)
    {
        var adminRoleId = await _db.Roles.Where(r => r.Name == "Admin").Select(r => r.Id).FirstOrDefaultAsync();
        return adminRoleId is not null &&
               await _db.UserRoles.AnyAsync(ur => ur.UserId == user.Id && ur.RoleId == adminRoleId);
    }

    private async Task SetUserAsync(ApplicationUser user)
    {
        User = user;
        IsAdmin = await IsUserAdminAsync(user);
        await _js.InvokeVoidAsync("localStorage.setItem", Key, user.Id);
        await _snapshot.SaveAsync(_db);
        Changed?.Invoke();
    }
}
