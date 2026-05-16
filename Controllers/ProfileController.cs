using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using VibeLang.Models;

namespace VibeLang.Controllers;

/// <summary>
/// ProfileController – accessible by both Admin and User roles.
/// </summary>
[Authorize(Roles = "Admin,User")]
public class ProfileController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ProfileController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(string firstName, string lastName)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        user.FirstName = firstName;
        user.LastName = lastName;

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            TempData["StatusMessage"] = "Profile updated successfully!";
        }
        else
        {
            TempData["StatusMessage"] = "Error updating profile.";
        }

        return RedirectToAction(nameof(Index));
    }
}
