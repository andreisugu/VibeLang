using Microsoft.EntityFrameworkCore;
using VibeLang.Models;
using VibeLang.Repositories;

namespace VibeLang.Services;

public class AchievementService : IAchievementService
{
    private readonly IUserAchievementRepository _userAchievementRepository;
    private readonly VibeLangDbContext _context;
    private readonly ILogger<AchievementService> _logger;

    public AchievementService(IUserAchievementRepository userAchievementRepository, VibeLangDbContext context, ILogger<AchievementService> logger)
    {
        _userAchievementRepository = userAchievementRepository;
        _context = context;
        _logger = logger;
    }

    public async Task<bool> UserHasAchievementAsync(string userId, string achievementTitle)
    {
        var achievement = await _userAchievementRepository.GetAchievementByTitleAsync(achievementTitle);
        if (achievement == null) return false;

        return await _userAchievementRepository.UserHasAchievementAsync(userId, achievement.Id);
    }

    public async Task<Dictionary<int, DateTime>> GetUserAchievementsAsync(string userId)
    {
        var achievements = await _userAchievementRepository.GetUserAchievementsAsync(userId);
        return achievements.ToDictionary(a => a.AchievementId, a => a.EarnedDate);
    }

    public async Task<int> GetUserAchievementCountAsync(string userId)
    {
        return await _userAchievementRepository.GetUserAchievementCountAsync(userId);
    }

    public async Task AwardAchievementIfNotEarnedAsync(string userId, string achievementTitle)
    {
        var achievement = await _userAchievementRepository.GetAchievementByTitleAsync(achievementTitle);
        if (achievement == null) return;

        bool alreadyEarned = await _userAchievementRepository.UserHasAchievementAsync(userId, achievement.Id);
        if (!alreadyEarned)
        {
            var userAchievement = new UserAchievement
            {
                UserId = userId,
                AchievementId = achievement.Id,
                EarnedDate = DateTime.UtcNow
            };
            await _userAchievementRepository.AddAsync(userAchievement);
        }
    }

    public async Task CheckAndAwardAchievementsAsync(string userId, int lessonId, int courseId, int score)
    {
        try
        {
            // 1. Beginner's Start - First lesson completed
            var totalLessonsCompleted = await _context.UserLessonProgresses
                .Where(ulp => ulp.UserId == userId && ulp.IsCompleted)
                .CountAsync();

            if (totalLessonsCompleted >= 1)
            {
                await AwardAchievementIfNotEarnedAsync(userId, "Beginner's Start");
            }

            // 2. Perfect Score - 100% on a quiz
            if (score == 100)
            {
                await AwardAchievementIfNotEarnedAsync(userId, "Perfect Score");
            }

            // 3. Expert Learner - Completed all lessons in a course
            var totalLessonsInCourse = await _context.Lessons
                .Where(l => l.Chapter!.CourseId == courseId)
                .CountAsync();

            var completedLessonsInCourse = await _context.UserLessonProgresses
                .Where(ulp => ulp.UserId == userId && ulp.IsCompleted &&
                       ulp.Lesson!.Chapter!.CourseId == courseId)
                .CountAsync();

            if (totalLessonsInCourse > 0 && completedLessonsInCourse == totalLessonsInCourse)
            {
                await AwardAchievementIfNotEarnedAsync(userId, "Expert Learner");
            }

            // 4. Speed Learner - Complete 5 lessons in one day
            var today = DateTime.UtcNow.Date;
            var lessonsToday = await _context.UserLessonProgresses
                .Where(ulp => ulp.UserId == userId && ulp.IsCompleted && ulp.CompletionDate >= today)
                .CountAsync();

            if (lessonsToday >= 5)
            {
                await AwardAchievementIfNotEarnedAsync(userId, "Speed Learner");
            }

            // 5. Marathon Runner - Maintain a 7-day streak on ANY course
            var maxStreak = await _context.UserCourseStats
                .Where(s => s.UserId == userId)
                .MaxAsync(s => (int?)s.CurrentStreak) ?? 0;

            if (maxStreak >= 7)
            {
                await AwardAchievementIfNotEarnedAsync(userId, "Marathon Runner");
            }

            // 6. Social Butterfly - Reach the top 10 on the leaderboard
            var userXP = await _context.UserCourseStats
                .Where(s => s.UserId == userId && s.CourseId == courseId)
                .Select(s => s.TotalXP)
                .FirstOrDefaultAsync();

            var usersAbove = await _context.UserCourseStats
                .Where(s => s.CourseId == courseId && s.TotalXP > userXP)
                .CountAsync();

            if (usersAbove < 10)
            {
                await AwardAchievementIfNotEarnedAsync(userId, "Social Butterfly");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error checking achievements: {ex.Message}");
        }
    }

    public async Task CheckAchievementsForAllCompletedLessonsAsync(string userId)
    {
        try
        {
            var completedLessons = await _context.UserLessonProgresses
                .Where(ulp => ulp.UserId == userId && ulp.IsCompleted)
                .Include(ulp => ulp.Lesson).ThenInclude(l => l!.Chapter)
                .ToListAsync();

            if (!completedLessons.Any()) return;

            // 1. Beginner's Start
            await AwardAchievementIfNotEarnedAsync(userId, "Beginner's Start");

            // 2. Perfect Score
            if (completedLessons.Any(ul => ul.ScoreAchieved == 100))
            {
                await AwardAchievementIfNotEarnedAsync(userId, "Perfect Score");
            }

            // 3. Expert Learner
            var courseIds = completedLessons
                .Where(ul => ul.Lesson?.Chapter != null)
                .Select(ul => ul.Lesson!.Chapter!.CourseId)
                .Distinct();

            foreach (var courseId in courseIds)
            {
                var totalInCourse = await _context.Lessons
                    .Where(l => l.Chapter!.CourseId == courseId).CountAsync();

                var completedInCourse = completedLessons
                    .Count(ul => ul.Lesson?.Chapter?.CourseId == courseId);

                if (totalInCourse > 0 && completedInCourse == totalInCourse)
                {
                    await AwardAchievementIfNotEarnedAsync(userId, "Expert Learner");
                    break;
                }
            }

            // 4. Speed Learner - check if any single day has 5+ completions
            var lessonsByDay = completedLessons
                .GroupBy(ul => ul.CompletionDate.Date)
                .Any(g => g.Count() >= 5);

            if (lessonsByDay)
            {
                await AwardAchievementIfNotEarnedAsync(userId, "Speed Learner");
            }

            // 5. Marathon Runner
            var maxStreak = await _context.UserCourseStats
                .Where(s => s.UserId == userId)
                .MaxAsync(s => (int?)s.CurrentStreak) ?? 0;

            if (maxStreak >= 7)
            {
                await AwardAchievementIfNotEarnedAsync(userId, "Marathon Runner");
            }

            // 6. Social Butterfly - check best rank across all courses
            var allUserStats = await _context.UserCourseStats
                .Where(s => s.UserId == userId)
                .ToListAsync();

            foreach (var stat in allUserStats)
            {
                var usersAbove = await _context.UserCourseStats
                    .Where(s => s.CourseId == stat.CourseId && s.TotalXP > stat.TotalXP)
                    .CountAsync();

                if (usersAbove < 10)
                {
                    await AwardAchievementIfNotEarnedAsync(userId, "Social Butterfly");
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error checking achievements for all lessons: {ex.Message}");
        }
    }
}
