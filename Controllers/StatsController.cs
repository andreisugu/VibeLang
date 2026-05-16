using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VibeLang.Services;

namespace VibeLang.Controllers;

/// <summary>
/// StatsController – leaderboard visible to both Admin and User roles.
/// </summary>
[Authorize(Roles = "Admin,User")]
public class StatsController : Controller
{
    private readonly IStatsService _statsService;

    public StatsController(IStatsService statsService)
    {
        _statsService = statsService;
    }

    public async Task<IActionResult> Leaderboard()
    {
        var stats = await _statsService.GetLeaderboardAsync();
        return View(stats);
    }
}
