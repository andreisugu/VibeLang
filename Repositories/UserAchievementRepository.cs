using Microsoft.EntityFrameworkCore;
using VibeLang.Models;

namespace VibeLang.Repositories;

public class UserAchievementRepository : Repository<UserAchievement>, IUserAchievementRepository
{
    public UserAchievementRepository(VibeLangDbContext context) : base(context)
    {
    }

    public async Task<bool> UserHasAchievementAsync(string userId, int achievementId)
    {
        return await _context.UserAchievements
            .AnyAsync(ua => ua.UserId == userId && ua.AchievementId == achievementId);
    }

    public async Task<IEnumerable<UserAchievement>> GetUserAchievementsAsync(string userId)
    {
        return await _context.UserAchievements
            .Where(ua => ua.UserId == userId)
            .Include(ua => ua.Achievement)
            .ToListAsync();
    }

    public async Task<int> GetUserAchievementCountAsync(string userId)
    {
        return await _context.UserAchievements
            .Where(ua => ua.UserId == userId)
            .CountAsync();
    }

    public async Task<Achievement?> GetAchievementByTitleAsync(string title)
    {
        return await _context.Achievements
            .FirstOrDefaultAsync(a => a.Title == title);
    }

    public async Task<UserAchievement?> GetUserAchievementByTitleAsync(string userId, string achievementTitle)
    {
        return await _context.UserAchievements
            .Where(ua => ua.UserId == userId && ua.Achievement.Title == achievementTitle)
            .FirstOrDefaultAsync();
    }
}
