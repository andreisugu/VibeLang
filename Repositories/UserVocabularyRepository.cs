using Microsoft.EntityFrameworkCore;
using VibeLang.Models;

namespace VibeLang.Repositories;

public class UserVocabularyRepository : Repository<UserVocabulary>, IUserVocabularyRepository
{
    public UserVocabularyRepository(VibeLangDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<UserVocabulary>> GetUserVocabulariesAsync(string userId)
    {
        return await _context.UserVocabularies
            .Where(uv => uv.UserId == userId)
            .Include(uv => uv.Word)
                .ThenInclude(w => w!.Lesson)
                    .ThenInclude(l => l!.Chapter)
            .ToListAsync();
    }

    public async Task<bool> UserHasVocabularyAsync(string userId, int wordId)
    {
        return await _context.UserVocabularies
            .AnyAsync(uv => uv.UserId == userId && uv.WordId == wordId);
    }

    public async Task<IEnumerable<VocabularyWord>> GetUserVocabularyWordsAsync(string userId)
    {
        var userVocabEntries = await _context.UserVocabularies
            .Where(uv => uv.UserId == userId)
            .Include(uv => uv.Word)
                .ThenInclude(w => w!.Lesson)
                    .ThenInclude(l => l!.Chapter)
            .ToListAsync();

        // De-duplicate by word content - if user learned same word from multiple lessons,
        // only show it once in vocabulary list
        return userVocabEntries
            .Where(uv => uv.Word != null)
            .GroupBy(uv => new { uv.Word!.Word, uv.Word.Translation })
            .Select(g => g.First().Word!)
            .OrderBy(w => w.Word)
            .ToList();
    }

    /// <summary>
    /// Bulk assign vocabulary words to a user (efficient batch operation).
    /// Skips words already assigned to user (handles race conditions gracefully).
    /// </summary>
    public async Task AssignWordsToUserAsync(string userId, IEnumerable<int> wordIds)
    {
        if (!wordIds.Any()) return;

        // Get existing assignments for this user
        var existingWordIds = await _context.UserVocabularies
            .Where(uv => uv.UserId == userId && wordIds.Contains(uv.WordId))
            .Select(uv => uv.WordId)
            .ToHashSetAsync();

        // Filter out words already assigned
        var newWordIds = wordIds.Where(wid => !existingWordIds.Contains(wid)).ToList();
        if (!newWordIds.Any()) return;

        // Bulk insert new assignments
        var newAssignments = newWordIds.Select(wid => new UserVocabulary
        {
            UserId = userId,
            WordId = wid,
            Status = "Learned",
            LastReviewed = DateTime.UtcNow
        }).ToList();

        _context.UserVocabularies.AddRange(newAssignments);
        await _context.SaveChangesAsync();
    }

    /// <summary>Update status of a user's vocabulary word (mark as learned, mastered, etc.).</summary>
    public async Task UpdateVocabularyStatusAsync(string userId, int wordId, string status)
    {
        var userVocab = await _context.UserVocabularies
            .FirstOrDefaultAsync(uv => uv.UserId == userId && uv.WordId == wordId);

        if (userVocab != null)
        {
            userVocab.Status = status;
            userVocab.LastReviewed = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>Remove word from user's vocabulary.</summary>
    public async Task RemoveFromUserVocabularyAsync(string userId, int wordId)
    {
        var userVocab = await _context.UserVocabularies
            .FirstOrDefaultAsync(uv => uv.UserId == userId && uv.WordId == wordId);

        if (userVocab != null)
        {
            _context.UserVocabularies.Remove(userVocab);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>Get vocabulary statistics for a user (counting unique words by content, not by lesson).</summary>
    public async Task<(int Total, int Learned, int Mastered)> GetUserVocabularyStatsAsync(string userId)
    {
        var userVocabs = await _context.UserVocabularies
            .Where(uv => uv.UserId == userId)
            .Include(uv => uv.Word)
            .ToListAsync();

        // Group by word content to count unique words (same word from different lessons = 1)
        var uniqueWords = userVocabs
            .Where(uv => uv.Word != null)
            .GroupBy(uv => new { uv.Word!.Word, uv.Word.Translation })
            .ToList();

        var total = uniqueWords.Count;

        // For each unique word, take the best status (Mastered > Learned > New)
        var learned = uniqueWords
            .Count(g => g.Any(uv => uv.Status == "Mastered" || uv.Status == "Learned"));

        var mastered = uniqueWords
            .Count(g => g.Any(uv => uv.Status == "Mastered"));

        return (total, learned, mastered);
    }
}
