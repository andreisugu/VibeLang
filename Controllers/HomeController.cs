using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using VibeLang.Models;

namespace VibeLang.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Courses()
    {
        return View();
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
