using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using VibeLang.Models;
using VibeLang.Services;

namespace VibeLang.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly VibeLangDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IStatsService _statsService;
    private readonly IAchievementService _achievementService;

    public HomeController(ILogger<HomeController> logger, VibeLangDbContext context, UserManager<ApplicationUser> userManager,
        IStatsService statsService, IAchievementService achievementService)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _statsService = statsService;
        _achievementService = achievementService;
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

            // 2. Show the user's overall stats on the dashboard (get first stats as aggregate)
            var stats = await _context.UserCourseStats
                .Where(s => s.UserId == user.Id)
                .OrderByDescending(s => s.TotalXP)
                .FirstOrDefaultAsync();
            
            // 3. Count lessons completed
            var lessonsCompleted = await _context.UserLessonProgresses
                .Where(ulp => ulp.UserId == user.Id && ulp.IsCompleted)
                .CountAsync();
            ViewBag.LessonsCompleted = lessonsCompleted;

            // 4. Calculate user level (1 level per 500 XP)
            ViewBag.CurrentLevel = Math.Max(1, ((stats?.TotalXP ?? 0) / 500) + 1);

            // 5. Get achievements count using service
            var achievementCount = await _achievementService.GetUserAchievementCountAsync(user.Id);
            ViewBag.CurrentAchievementCount = achievementCount;

            // 6. Get course progress for each course
            var courses = await _context.Courses
                .Include(c => c.Chapters)
                    .ThenInclude(ch => ch.Lessons)
                .ToListAsync();

            var courseProgress = new List<dynamic>();
            foreach (var course in courses)
            {
                var totalLessons = course.Chapters.SelectMany(ch => ch.Lessons).Count();
                if (totalLessons == 0) continue;

                var completedLessons = await _context.UserLessonProgresses
                    .Where(ulp => ulp.UserId == user.Id && ulp.IsCompleted && 
                           ulp.Lesson != null && ulp.Lesson.Chapter != null && ulp.Lesson.Chapter.CourseId == course.Id)
                    .CountAsync();

                courseProgress.Add(new
                {
                    CourseName = course.Title,
                    TotalLessons = totalLessons,
                    CompletedLessons = completedLessons
                });
            }
            ViewBag.CourseProgress = courseProgress;

            // 7. Get recent activity (last 5 completed lessons)
            var recentActivity = await _context.UserLessonProgresses
                .Where(ulp => ulp.UserId == user.Id && ulp.IsCompleted)
                .Include(ulp => ulp.Lesson)
                    .ThenInclude(l => l!.Chapter)
                        .ThenInclude(ch => ch!.Course)
                .OrderByDescending(ulp => ulp.CompletionDate)
                .Take(5)
                .Select(ulp => new
                {
                    LessonTitle = ulp.Lesson!.Title,
                    ChapterTitle = ulp.Lesson!.Chapter!.Title,
                    CourseName = ulp.Lesson!.Chapter!.Course!.Title,
                    Score = ulp.ScoreAchieved,
                    CompletionDate = ulp.CompletionDate
                })
                .ToListAsync();
            ViewBag.RecentActivity = recentActivity;

            // 8. Get streak status information
            if (stats != null)
            {
                ViewBag.StreakStatus = _statsService.GetStreakStatusMessage(stats);
                ViewBag.HoursUntilStreakLoss = _statsService.GetHoursUntilStreakLoss(stats);
                ViewBag.MaxStreak = stats.MaxStreakEver;
                ViewBag.StreakBrokenToday = stats.StreakBrokenToday;
            }

            ViewBag.LastLessonId = lastLessonId;
            return View(stats);
        }
        
        return View();
    }

    public async Task<IActionResult> Courses()
    {
        var user = await _userManager.GetUserAsync(User);

        // Fetch all courses with their chapters and lessons
        var courses = await _context.Courses
            .Include(c => c.Chapters.OrderBy(ch => ch.Order))
                .ThenInclude(ch => ch.Lessons.OrderBy(l => l.Order))
            .ToListAsync();

        if (user != null)
        {
            // Get completed lesson IDs for the current user
            var completedLessonIds = await _context.UserLessonProgresses
                .Where(ulp => ulp.UserId == user.Id && ulp.IsCompleted)
                .Select(ulp => ulp.LessonId)
                .ToListAsync();
            
            ViewBag.CompletedLessonIds = completedLessonIds;
        }
        else
        {
            ViewBag.CompletedLessonIds = new List<int>();
        }

        return View(courses);
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
    