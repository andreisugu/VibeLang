using VibeLang.Models;

namespace VibeLang.Repositories;

public interface IStatsRepository : IRepository<UserCourseStats>
{
    Task<UserCourseStats?> GetUserCourseStatsAsync(string userId, int courseId);
    Task<IEnumerable<UserCourseStats>> GetLeaderboardAsync(int take = 10);
    Task<int> GetUserRankInCourseAsync(string userId, int courseId);
    Task<UserCourseStats?> GetTopUserInCourseAsync(int courseId);
}
