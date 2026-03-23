using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using VibeLang.Models;
using Microsoft.AspNetCore.Authorization;

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
        // Fetch top stats, including the Course and User name for the leaderboard
        var stats = await _context.UserCourseStats
            .Include(s => s.Course)
            .Include(s => s.User)
            .OrderByDescending(s => s.TotalXP)
            .Take(10)
            .ToListAsync();
        return View(stats);
    }

    public async Task<IActionResult> Lesson(int? id)
    {
        Lesson? lesson;
        if (id.HasValue)
        {
            lesson = await _context.Lessons
                .Include(l => l.Chapter)
                .FirstOrDefaultAsync(l => l.Id == id.Value);
        }
        else
        {
            lesson = await _context.Lessons
                .Include(l => l.Chapter)
                .OrderBy(l => l.Id)
                .FirstOrDefaultAsync();
        }
        
        return View(lesson);
    }

    [HttpGet]
    public async Task<IActionResult> GetLessonData(int id)
    {
        var lesson = await _context.Lessons.FindAsync(id);
        if (lesson == null || string.IsNullOrEmpty(lesson.ContentJson))
        {
            return NotFound();
        }
        return Content(lesson.ContentJson, "application/json");
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitResult([FromBody] LessonResult result)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var lesson = await _context.Lessons
            .Include(l => l.Chapter)
            .FirstOrDefaultAsync(l => l.Id == result.LessonId);
        
        if (lesson == null) return NotFound();

        // 1. Update Lesson Progress
        var progress = await _context.UserLessonProgresses
            .FirstOrDefaultAsync(p => p.UserId == user.Id && p.LessonId == lesson.Id);
        
        if (progress == null)
        {
            progress = new UserLessonProgress
            {
                UserId = user.Id,
                LessonId = lesson.Id,
                IsCompleted = true,
                ScoreAchieved = result.Score,
                CompletionDate = DateTime.UtcNow
            };
            _context.UserLessonProgresses.Add(progress);
        }
        else
        {
            progress.IsCompleted = true;
            progress.ScoreAchieved = Math.Max(progress.ScoreAchieved, result.Score);
            progress.CompletionDate = DateTime.UtcNow;
        }

        // 2. Update Course Stats (XP and Streak)
        var stats = await _context.UserCourseStats
            .FirstOrDefaultAsync(s => s.UserId == user.Id && s.CourseId == lesson.Chapter!.CourseId);
        
        if (stats == null)
        {
            stats = new UserCourseStats
            {
                UserId = user.Id,
                CourseId = lesson.Chapter!.CourseId,
                TotalXP = result.Score,
                CurrentStreak = 1
            };
            _context.UserCourseStats.Add(stats);
        }
        else
        {
            stats.TotalXP += result.Score;
            // Simple streak logic: if last lesson was today or yesterday, increment or keep
            // For now, just incrementing for simplicity
            stats.CurrentStreak++;
        }

        await _context.SaveChangesAsync();

        return Json(new { success = true, xpAdded = result.Score, totalXP = stats.TotalXP });
    }

    public class LessonResult
    {
        public int LessonId { get; set; }
        public int Score { get; set; }
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
                .Where(uv => uv.UserId == user.Id) 
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

        int? lastLessonId = null;

        if (user != null)
        {
            // 1. Get user's latest lesson activity
            var lastProgress = await _context.UserLessonProgresses
                .Include(p => p.Lesson)
                    .ThenInclude(l => l!.Chapter)
                .Where(p => p.UserId == user.Id)
                .OrderByDescending(p => p.CompletionDate)
                .FirstOrDefaultAsync();

            if (lastProgress != null && lastProgress.Lesson != null)
            {
                // If the last lesson was passed (score >= 50), try to find the NEXT one
                if (lastProgress.ScoreAchieved >= 50)
                {
                    // Find next lesson in same chapter
                    var nextLesson = await _context.Lessons
                        .Where(l => l.ChapterId == lastProgress.Lesson.ChapterId && l.Order > lastProgress.Lesson.Order)
                        .OrderBy(l => l.Order)
                        .FirstOrDefaultAsync();

                    if (nextLesson != null)
                    {
                        lastLessonId = nextLesson.Id;
                    }
                    else
                    {
                        // Try next chapter in same course
                        var nextChapter = await _context.Chapters
                            .Where(c => c.CourseId == lastProgress.Lesson.Chapter!.CourseId && c.Order > lastProgress.Lesson.Chapter!.Order)
                            .OrderBy(c => c.Order)
                            .FirstOrDefaultAsync();

                        if (nextChapter != null)
                        {
                            var firstLessonInNextChapter = await _context.Lessons
                                .Where(l => l.ChapterId == nextChapter.Id)
                                .OrderBy(l => l.Order)
                                .FirstOrDefaultAsync();
                            lastLessonId = firstLessonInNextChapter?.Id;
                        }
                    }
                }
                
                // If we didn't find a next lesson, or the last one was failed, just use the last one
                if (lastLessonId == null)
                {
                    lastLessonId = lastProgress.LessonId;
                }
            }

            // Fallback for new users: get first lesson ever
            if (lastLessonId == null)
            {
                var firstLesson = await _context.Lessons.OrderBy(l => l.Id).FirstOrDefaultAsync();
                lastLessonId = firstLesson?.Id;
            }

            if (lastLessonId != null)
            {
                ViewBag.ContinueLesson = await _context.Lessons
                    .Include(l => l.Chapter)
                        .ThenInclude(c => c!.Course)
                    .FirstOrDefaultAsync(l => l.Id == lastLessonId);
            }

            // 2. Show the user's overall stats on the dashboard
            var stats = await _context.UserCourseStats
                .Where(s => s.UserId == user.Id)
                .OrderByDescending(s => s.TotalXP)
                .FirstOrDefaultAsync();
            
            ViewBag.LastLessonId = lastLessonId;
            return View(stats);
        }
        
        return View();
    }

    public async Task<IActionResult> Courses()
    {
        // Fetch all courses with their chapters and lessons
        var courses = await _context.Courses
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
