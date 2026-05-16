using Microsoft.AspNetCore.Identity;

namespace VibeLang.Models;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// Server-relative URL of the user's profile picture, e.g. /uploads/profile-pictures/abc.jpg.
    /// Null when no picture has been uploaded; the UI falls back to an initials avatar.
    /// </summary>
    public string? ProfilePicturePath { get; set; }
}
