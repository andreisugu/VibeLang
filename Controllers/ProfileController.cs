using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VibeLang.Models;

namespace VibeLang.Controllers;

/// <summary>
/// ProfileController – accessible by both Admin and User roles.
/// Handles profile info updates and profile picture uploads.
/// </summary>
[Authorize(Roles = "Admin,User")]
public class ProfileController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    // Allowed image MIME types and their corresponding extensions
    private static readonly Dictionary<string, string> _allowedTypes = new()
    {
        { "image/jpeg", ".jpg" },
        { "image/png",  ".png" },
        { "image/gif",  ".gif" },
        { "image/webp", ".webp" }
    };

    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB
    private const string UploadFolder = "uploads/profile-pictures";

    public ProfileController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    // GET /profile
    public IActionResult Index()
    {
        return View();
    }

    // POST /profile/update  — update first/last name
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(string firstName, string lastName)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        user.FirstName = firstName;
        user.LastName = lastName;

        var result = await _userManager.UpdateAsync(user);
        TempData["StatusMessage"] = result.Succeeded
            ? "Profile updated successfully!"
            : "Error updating profile. Please try again.";

        return RedirectToAction(nameof(Index));
    }

    // POST /profile/uploadpicture  — upload a new profile picture
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadPicture(IFormFile profilePicture)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        // --- Validation ---
        if (profilePicture == null || profilePicture.Length == 0)
        {
            TempData["PictureError"] = "Please select a file to upload.";
            return RedirectToAction(nameof(Index));
        }

        if (profilePicture.Length > MaxFileSizeBytes)
        {
            TempData["PictureError"] = "File is too large. Maximum size is 5 MB.";
            return RedirectToAction(nameof(Index));
        }

        if (!_allowedTypes.TryGetValue(profilePicture.ContentType.ToLower(), out var extension))
        {
            TempData["PictureError"] = "Invalid file type. Please upload a JPEG, PNG, GIF, or WebP image.";
            return RedirectToAction(nameof(Index));
        }

        // --- Save file to wwwroot/uploads/profile-pictures ---
        var wwwroot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var uploadDir = Path.Combine(wwwroot, UploadFolder);
        Directory.CreateDirectory(uploadDir); // ensure folder exists

        // Delete old picture if one exists
        if (!string.IsNullOrEmpty(user.ProfilePicturePath))
        {
            var oldPath = Path.Combine(wwwroot, user.ProfilePicturePath.TrimStart('/'));
            if (System.IO.File.Exists(oldPath))
                System.IO.File.Delete(oldPath);
        }

        // Use a GUID filename to avoid collisions and path-traversal attacks
        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadDir, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await profilePicture.CopyToAsync(stream);
        }

        // Store the server-relative URL (accessible from the browser)
        user.ProfilePicturePath = $"/{UploadFolder}/{fileName}";
        var result = await _userManager.UpdateAsync(user);

        TempData["StatusMessage"] = result.Succeeded
            ? "Profile picture updated successfully!"
            : "Error saving profile picture. Please try again.";

        return RedirectToAction(nameof(Index));
    }

    // POST /profile/removepicture  — remove profile picture
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemovePicture()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        if (!string.IsNullOrEmpty(user.ProfilePicturePath))
        {
            var wwwroot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var oldPath = Path.Combine(wwwroot, user.ProfilePicturePath.TrimStart('/'));
            if (System.IO.File.Exists(oldPath))
                System.IO.File.Delete(oldPath);

            user.ProfilePicturePath = null;
            await _userManager.UpdateAsync(user);
        }

        TempData["StatusMessage"] = "Profile picture removed.";
        return RedirectToAction(nameof(Index));
    }
}
