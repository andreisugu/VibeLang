using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using VibeLang.Models;

namespace VibeLang.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly VibeLangDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(ILogger<HomeController> logger, VibeLangDbContext context, UserManager<ApplicationUser> userManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Leaderboard()
    {
        // Fetch top stats, including the Course name for the leaderboard
        var stats = await _context.UserCourseStats
            .Include(s => s.Course)
            .OrderByDescending(s => s.TotalXP)
            .Take(10)
            .ToListAsync();
        return View(stats);
    }

    public async Task<IActionResult> Lesson()
    {
        // Fetch a sample lesson with its vocabulary words
        var lesson = await _context.Lessons
            .Include(l => l.VocabularyWords)
            .OrderBy(l => l.Id) // Deterministic order
            .FirstOrDefaultAsync();
        
        return View(lesson);
    }

    public async Task<IActionResult> Quiz()
    {
        // Fetch a sample quiz with questions and options
        var quiz = await _context.Quizzes
            .Include(q => q.Questions)
                .ThenInclude(o => o.Options)
            .OrderBy(q => q.Id) // Deterministic order
            .FirstOrDefaultAsync();

        return View(quiz);
    }

    public async Task<IActionResult> Vocabulary()
    {
        var user = await _userManager.GetUserAsync(User);
        
        // Fetch all words
        var words = await _context.VocabularyWords
            .ToListAsync();
        
        if (user != null)
        {
            var userProgress = await _context.UserVocabularies
                .Where(uv => uv.UserId.ToString() == user.Id) 
                .ToDictionaryAsync(uv => uv.WordId, uv => uv.Status);
            ViewBag.UserProgress = userProgress;
        }
        else
        {
            ViewBag.UserProgress = new Dictionary<int, string>();
        }

        return View(words);
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        ViewBag.FirstName = user?.FirstName ?? user?.UserName ?? "Student";

        if (user != null)
        {
            // Show the user's overall stats on the dashboard
            var stats = await _context.UserCourseStats
                .Where(s => s.UserId.ToString() == user.Id)
                .OrderByDescending(s => s.TotalXP)
                .FirstOrDefaultAsync();
            return View(stats);
        }
        
        return View();
    }

    public async Task<IActionResult> Courses()
    {
        // Fetch all courses with their chapters and lessons
        var courses = await _context.Courses
            .Include(c => c.Language)
            .Include(c => c.Chapters.OrderBy(ch => ch.Order))
                .ThenInclude(ch => ch.Lessons.OrderBy(l => l.Order))
            .ToListAsync();
        return View(courses);
    }

    public IActionResult Profile()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(string firstName, string lastName)
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

        return RedirectToAction(nameof(Profile));
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
