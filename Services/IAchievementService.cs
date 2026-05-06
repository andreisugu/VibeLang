using VibeLang.Models;

namespace VibeLang.Services;

public interface IAchievementService
{
    Task<bool> UserHasAchievementAsync(string userId, string achievementTitle);
    Task<Dictionary<int, DateTime>> GetUserAchievementsAsync(string userId);
    Task<int> GetUserAchievementCountAsync(string userId);
    Task AwardAchievementIfNotEarnedAsync(string userId, string achievementTitle);
    Task CheckAndAwardAchievementsAsync(string userId, int lessonId, int courseId, int score);
    Task CheckAchievementsForAllCompletedLessonsAsync(string userId);
}
