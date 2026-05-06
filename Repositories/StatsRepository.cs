using Microsoft.EntityFrameworkCore;
using VibeLang.Models;

namespace VibeLang.Repositories;

public class StatsRepository : Repository<UserCourseStats>, IStatsRepository
{
    public StatsRepository(VibeLangDbContext context) : base(context)
    {
    }

    public async Task<UserCourseStats?> GetUserCourseStatsAsync(string userId, int courseId)
    {
        return await _context.UserCourseStats
            .FirstOrDefaultAsync(s => s.UserId == userId && s.CourseId == courseId);
    }

    public async Task<IEnumerable<UserCourseStats>> GetLeaderboardAsync(int take = 10)
    {
        return await _context.UserCourseStats
            .Include(s => s.Course)
            .Include(s => s.User)
            .OrderByDescending(s => s.TotalXP)
            .Take(take)
            .ToListAsync();
    }

    public async Task<int> GetUserRankInCourseAsync(string userId, int courseId)
    {
        var userXP = await _context.UserCourseStats
            .Where(s => s.UserId == userId && s.CourseId == courseId)
            .Select(s => s.TotalXP)
            .FirstOrDefaultAsync();

        var usersAbove = await _context.UserCourseStats
            .Where(s => s.CourseId == courseId && s.TotalXP > userXP)
            .CountAsync();

        return usersAbove + 1;
    }

    public async Task<UserCourseStats?> GetTopUserInCourseAsync(int courseId)
    {
        return await _context.UserCourseStats
            .Where(s => s.CourseId == courseId)
            .OrderByDescending(s => s.TotalXP)
            .FirstOrDefaultAsync();
    }
}
