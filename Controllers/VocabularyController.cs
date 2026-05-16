using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using VibeLang.Models;
using VibeLang.Services;

namespace VibeLang.Controllers;

/// <summary>
/// VocabularyController – accessible by both Admin and User roles.
/// </summary>
[Authorize(Roles = "Admin,User")]
public class VocabularyController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IVocabularyService _vocabularyService;

    public VocabularyController(UserManager<ApplicationUser> userManager, IVocabularyService vocabularyService)
    {
        _userManager = userManager;
        _vocabularyService = vocabularyService;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);

        var words = new List<VocabularyWord>();
        var userProgress = new Dictionary<int, string>();

        if (user != null)
        {
            words = (await _vocabularyService.GetUserVocabularyAsync(user.Id)).ToList();
            userProgress = await _vocabularyService.GetUserVocabularyProgressAsync(user.Id);
        }

        ViewBag.UserProgress = userProgress;

        return View(words);
    }
}
