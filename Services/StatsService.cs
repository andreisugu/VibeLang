using VibeLang.Models;
using VibeLang.Repositories;

namespace VibeLang.Services;

public class StatsService : IStatsService
{
    private readonly IStatsRepository _statsRepository;

    public StatsService(IStatsRepository statsRepository)
    {
        _statsRepository = statsRepository;
    }

    public async Task<UserCourseStats?> GetOrCreateUserCourseStatsAsync(string userId, int courseId)
    {
        var stats = await _statsRepository.GetUserCourseStatsAsync(userId, courseId);
        if (stats != null) return stats;

        // Create new stats
        var newStats = new UserCourseStats
        {
            UserId = userId,
            CourseId = courseId,
            TotalXP = 0,
            CurrentStreak = 1,
            LastActivityDate = DateTime.UtcNow
        };

        await _statsRepository.AddAsync(newStats);
        return newStats;
    }

    public async Task UpdateUserStatsWithScoreAsync(string userId, int courseId, int score)
    {
        var stats = await GetOrCreateUserCourseStatsAsync(userId, courseId);
        if (stats == null) return;

        stats.TotalXP += score;

        // Update streak
        var today = DateTime.UtcNow.Date;
        var lastDate = stats.LastActivityDate?.Date;

        if (lastDate != today)
        {
            if (lastDate == today.AddDays(-1))
            {
                stats.CurrentStreak++; // consecutive day
            }
            else
            {
                stats.CurrentStreak = 1; // broke the streak
            }
            stats.LastActivityDate = DateTime.UtcNow;
        }

        await _statsRepository.UpdateAsync(stats);
    }

    public async Task<IEnumerable<UserCourseStats>> GetLeaderboardAsync()
    {
        return await _statsRepository.GetLeaderboardAsync(10);
    }

    public async Task<int> GetUserRankAsync(string userId, int courseId)
    {
        return await _statsRepository.GetUserRankInCourseAsync(userId, courseId);
    }
}
