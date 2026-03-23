using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VibeLang.Models;

namespace VibeLang.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly VibeLangDbContext _context;

    public HomeController(ILogger<HomeController> logger, VibeLangDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> Courses()
    {
        var languages = await _context.Languages.ToListAsync();
        return View(languages);
    }

    public IActionResult Leaderboard()
    {
        return View();
    }

    public IActionResult Lesson()
    {
        return View();
    }

    public IActionResult Profile()
    {
        return View();
    }

    public IActionResult Quiz()
    {
        return View();
    }

    public IActionResult Vocabulary()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
