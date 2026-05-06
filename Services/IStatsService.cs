using VibeLang.Models;

namespace VibeLang.Services;

public interface IStatsService
{
    Task<UserCourseStats?> GetOrCreateUserCourseStatsAsync(string userId, int courseId);
    Task UpdateUserStatsWithScoreAsync(string userId, int courseId, int score);
    Task<IEnumerable<UserCourseStats>> GetLeaderboardAsync();
    Task<int> GetUserRankAsync(string userId, int courseId);
}
