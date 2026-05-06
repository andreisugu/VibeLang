using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using VibeLang.Models;
using Microsoft.AspNetCore.Authorization;
using VibeLang.Services;

namespace VibeLang.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly VibeLangDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IStatsService _statsService;
    private readonly IQuizService _quizService;
    private readonly IAchievementService _achievementService;
    private readonly IVocabularyService _vocabularyService;
    private readonly ILessonVocabularyService _lessonVocabularyService;

    public HomeController(ILogger<HomeController> logger, VibeLangDbContext context, UserManager<ApplicationUser> userManager,
        IStatsService statsService, IQuizService quizService, IAchievementService achievementService, 
        IVocabularyService vocabularyService, ILessonVocabularyService lessonVocabularyService)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _statsService = statsService;
        _quizService = quizService;
        _achievementService = achievementService;
        _vocabularyService = vocabularyService;
        _lessonVocabularyService = lessonVocabularyService;
    }

    public async Task<IActionResult> Leaderboard()
    {
        var stats = await _statsService.GetLeaderboardAsync();
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

        // 2. Update Course Stats using stats service
        int xpGained = result.Score;
        await _statsService.UpdateUserStatsWithScoreAsync(user.Id, lesson.Chapter!.CourseId, xpGained);

        await _context.SaveChangesAsync();
        
        try
        {
            // 3. Sync vocabulary from lesson JSON (if not already synced)
            await _lessonVocabularyService.SyncVocabularyAsync(lesson);
            
            // 4. Add words to User Vocabulary
            await _vocabularyService.AssignLessonVocabularyToUserAsync(user.Id, lesson.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to sync vocabulary for user {user.Id}, lesson {lesson.Id}: {ex.Message}");
            // Don't fail the entire operation - vocabulary is secondary to lesson completion
        }

        try
        {
            // 5. Check and award achievements
            await _achievementService.CheckAndAwardAchievementsAsync(user.Id, lesson.Id, lesson.Chapter!.CourseId, result.Score);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to check achievements for user {user.Id}: {ex.Message}");
            // Don't fail the entire operation
        }

        var updatedStats = await _statsService.GetOrCreateUserCourseStatsAsync(user.Id, lesson.Chapter!.CourseId);

        return Json(new { success = true, xpAdded = xpGained, totalXP = updatedStats.TotalXP });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitQuiz([FromBody] QuizSubmission submission)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var quiz = await _quizService.GetQuizWithQuestionsAsync(submission.QuizId);
        if (quiz == null) return NotFound();

        // Calculate score using quiz service
        var (correct, total, score) = await _quizService.CalculateQuizScoreAsync(submission.QuizId, submission.Answers);

        // Update User Lesson Progress
        var lesson = quiz.Lesson;
        var progress = await _context.UserLessonProgresses
            .FirstOrDefaultAsync(p => p.UserId == user.Id && p.LessonId == lesson.Id);
        
        if (progress == null)
        {
            progress = new UserLessonProgress
            {
                UserId = user.Id,
                LessonId = lesson.Id,
                IsCompleted = true,
                ScoreAchieved = score,
                CompletionDate = DateTime.UtcNow
            };
            _context.UserLessonProgresses.Add(progress);
        }
        else
        {
            progress.IsCompleted = true;
            progress.ScoreAchieved = Math.Max(progress.ScoreAchieved, score);
            progress.CompletionDate = DateTime.UtcNow;
        }

        // Update Course Stats using stats service
        int xpGained = score;
        await _statsService.UpdateUserStatsWithScoreAsync(user.Id, lesson.Chapter!.CourseId, xpGained);

        await _context.SaveChangesAsync();

        try
        {
            // Sync vocabulary from lesson JSON (if not already synced)
            await _lessonVocabularyService.SyncVocabularyAsync(lesson);

            // Add words to User Vocabulary
            await _vocabularyService.AssignLessonVocabularyToUserAsync(user.Id, lesson.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to sync vocabulary for user {user.Id}, lesson {lesson.Id}: {ex.Message}");
            // Don't fail the entire operation - vocabulary is secondary to quiz completion
        }

        try
        {
            // Check and award achievements
            await _achievementService.CheckAndAwardAchievementsAsync(user.Id, lesson.Id, lesson.Chapter!.CourseId, score);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to check achievements for user {user.Id}: {ex.Message}");
            // Don't fail the entire operation
        }

        var updatedStats = await _statsService.GetOrCreateUserCourseStatsAsync(user.Id, lesson.Chapter!.CourseId);

        return Json(new { 
            success = true, 
            score = correct, 
            total = total, 
            correct = correct,
            percentage = score,
            xpAdded = xpGained,
            totalXP = updatedStats.TotalXP
        });
    }

    public class LessonResult
    {
        public int LessonId { get; set; }
        public int Score { get; set; }
    }

    public class QuizSubmission
    {
        public int QuizId { get; set; }
        public Dictionary<string, string> Answers { get; set; } = new();
    }

    public async Task<IActionResult> Vocabulary()
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
                           ulp.Lesson.Chapter!.CourseId == course.Id)
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
                    .ThenInclude(l => l.Chapter)
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

    public async Task<IActionResult> Achievements()
    {
        var user = await _userManager.GetUserAsync(User);
        
        if (user != null)
        {
            await _achievementService.CheckAchievementsForAllCompletedLessonsAsync(user.Id);
        }
        
        var allAchievements = await _context.Achievements.ToListAsync();
        var earnedAchievements = new Dictionary<int, DateTime>();
        
        if (user != null)
        {
            earnedAchievements = await _achievementService.GetUserAchievementsAsync(user.Id);
        }

        ViewBag.AllAchievements = allAchievements;
        ViewBag.EarnedAchievementIds = earnedAchievements.Keys.ToHashSet();
        ViewBag.EarnedAchievements = earnedAchievements;
        ViewBag.TotalEarned = earnedAchievements.Count;
        ViewBag.TotalAvailable = allAchievements.Count;

        // Calculate progress for each achievement type
        var progressData = new Dictionary<string, dynamic>();
        if (user != null)
        {
            var completedLessons = await _context.UserLessonProgresses
                .Where(ulp => ulp.UserId == user.Id && ulp.IsCompleted)
                .Include(ulp => ulp.Lesson).ThenInclude(l => l!.Chapter)
                .ToListAsync();

            // 1. Beginner's Start
            progressData["Beginner's Start"] = new { Current = Math.Min(completedLessons.Count, 1), Target = 1, Text = $"{Math.Min(completedLessons.Count, 1)}/1 lesson completed" };

            // 2. Perfect Score
            var bestScore = completedLessons.Any() ? completedLessons.Max(ul => ul.ScoreAchieved) : 0;
            progressData["Perfect Score"] = new { Current = bestScore, Target = 100, Text = $"Best score: {bestScore}%" };

            // 3. Expert Learner
            int bestCoursePercent = 0;
            string bestCourseText = "0 lessons";
            var courseIds = completedLessons.Where(ul => ul.Lesson?.Chapter != null).Select(ul => ul.Lesson!.Chapter!.CourseId).Distinct();
            foreach (var courseId in courseIds)
            {
                var total = await _context.Lessons.CountAsync(l => l.Chapter!.CourseId == courseId);
                var completed = completedLessons.Count(ul => ul.Lesson?.Chapter?.CourseId == courseId);
                int percent = total > 0 ? (int)((double)completed / total * 100) : 0;
                if (percent >= bestCoursePercent)
                {
                    bestCoursePercent = percent;
                    bestCourseText = $"{completed}/{total} lessons in best course";
                }
            }
            progressData["Expert Learner"] = new { Current = bestCoursePercent, Target = 100, Text = bestCourseText };

            // 4. Speed Learner
            var bestDayCount = completedLessons.GroupBy(ul => ul.CompletionDate.Date).Select(g => g.Count()).DefaultIfEmpty(0).Max();
            progressData["Speed Learner"] = new { Current = Math.Min(bestDayCount, 5), Target = 5, Text = $"{bestDayCount}/5 lessons in one day (best day)" };

            // 5. Marathon Runner
            var maxStreak = await _context.UserCourseStats.Where(s => s.UserId == user.Id).MaxAsync(s => (int?)s.CurrentStreak) ?? 0;
            progressData["Marathon Runner"] = new { Current = Math.Min(maxStreak, 7), Target = 7, Text = $"{maxStreak}/7 day streak" };

            // 6. Social Butterfly
            int bestRank = 100; // Default high rank
            var userStats = await _context.UserCourseStats.Where(s => s.UserId == user.Id).ToListAsync();
            foreach (var stat in userStats)
            {
                var usersAbove = await _context.UserCourseStats.CountAsync(s => s.CourseId == stat.CourseId && s.TotalXP > stat.TotalXP);
                int rank = usersAbove + 1;
                if (rank < bestRank) bestRank = rank;
            }
            int rankProgress = bestRank <= 10 ? 100 : (bestRank > 100 ? 0 : 100 - (bestRank - 10));
            progressData["Social Butterfly"] = new { Current = Math.Max(0, rankProgress), Target = 100, Text = $"Current best rank: #{bestRank}" };
        }
        
        ViewBag.AchievementProgress = progressData;
        return View(allAchievements);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
