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

        // Update streak - Daily completion tracking
        var today = DateTime.UtcNow.Date;
        var lastDate = stats.LastActivityDate?.Date;

        // Reset StreakBrokenToday flag for new day calculations
        stats.StreakBrokenToday = false;

        if (lastDate == null)
        {
            // First activity ever
            stats.CurrentStreak = 1;
            stats.LastActivityDate = DateTime.UtcNow;
        }
        else if (lastDate == today)
        {
            // Already did activity today - don't double-increment streak
            // Just ensure LastActivityDate is updated
            stats.LastActivityDate = DateTime.UtcNow;
        }
        else if (lastDate == today.AddDays(-1))
        {
            // Consecutive day - continue the streak
            stats.CurrentStreak++;
            
            // Update max streak if needed
            if (stats.CurrentStreak > stats.MaxStreakEver)
            {
                stats.MaxStreakEver = stats.CurrentStreak;
            }
            
            stats.LastActivityDate = DateTime.UtcNow;
        }
        else
        {
            // Streak broken (more than 1 day since last activity)
            int oldStreak = stats.CurrentStreak;
            stats.CurrentStreak = 1;
            stats.StreakBrokenToday = true;
            stats.LastActivityDate = DateTime.UtcNow;
            
            // Log the break (optional - for debugging)
            // _logger.LogInformation($"Streak broken for user {userId} in course {courseId}. Was: {oldStreak} days");
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

    /// <summary>Get streak health info: how long until streak is lost</summary>
    public int GetHoursUntilStreakLoss(UserCourseStats stats)
    {
        if (stats.LastActivityDate == null) return 24; // New user has full day

        var lastDate = stats.LastActivityDate.Value.Date;
        var today = DateTime.UtcNow.Date;
        
        if (lastDate == today)
        {
            // Active today - 24 hours until loss
            return 24;
        }
        else if (lastDate == today.AddDays(-1))
        {
            // Last active yesterday - less than 24 hours remaining
            var hoursElapsed = (int)DateTime.UtcNow.Subtract(stats.LastActivityDate.Value).TotalHours;
            return Math.Max(0, 24 - hoursElapsed);
        }
        else
        {
            // Streak already lost
            return 0;
        }
    }

    /// <summary>Get streak status message for UI display</summary>
    public string GetStreakStatusMessage(UserCourseStats stats)
    {
        var hoursRemaining = GetHoursUntilStreakLoss(stats);
        
        if (stats.StreakBrokenToday && stats.CurrentStreak == 1)
        {
            return "⚠️ Streak broken! Start a new one today!";
        }
        
        if (stats.CurrentStreak == 0)
        {
            return "No streak yet. Start learning!";
        }

        if (hoursRemaining == 24)
        {
            return "🔥 Streak active! Full day remaining";
        }
        else if (hoursRemaining > 12)
        {
            return $"🔥 Streak active! {hoursRemaining}h remaining";
        }
        else if (hoursRemaining > 0)
        {
            return $"⏰ Hurry! {hoursRemaining}h before streak ends";
        }
        else
        {
            return "⏳ Complete a lesson today to keep your streak!";
        }
    }
}
