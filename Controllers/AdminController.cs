using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VibeLang.Controllers;

/// <summary>
/// Admin-only controller. Access is restricted to users with the "Admin" role.
/// Regular users will be redirected to the AccessDenied page.
/// </summary>
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
