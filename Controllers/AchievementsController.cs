using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VibeLang.Models;
using VibeLang.Services;

namespace VibeLang.Controllers;

/// <summary>
/// AchievementsController – accessible by both Admin and User roles.
/// </summary>
[Authorize(Roles = "Admin,User")]
public class AchievementsController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAchievementService _achievementService;
    private readonly VibeLangDbContext _context;

    public AchievementsController(UserManager<ApplicationUser> userManager, IAchievementService achievementService, VibeLangDbContext context)
    {
        _userManager = userManager;
        _achievementService = achievementService;
        _context = context;
    }

    public async Task<IActionResult> Index()
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
}
