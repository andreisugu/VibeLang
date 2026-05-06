using VibeLang.Models;

namespace VibeLang.Repositories;

public interface IUserAchievementRepository : IRepository<UserAchievement>
{
    Task<bool> UserHasAchievementAsync(string userId, int achievementId);
    Task<IEnumerable<UserAchievement>> GetUserAchievementsAsync(string userId);
    Task<int> GetUserAchievementCountAsync(string userId);
    Task<Achievement?> GetAchievementByTitleAsync(string title);
    Task<UserAchievement?> GetUserAchievementByTitleAsync(string userId, string achievementTitle);
}
