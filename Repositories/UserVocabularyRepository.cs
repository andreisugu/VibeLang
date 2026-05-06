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

        return userVocabEntries
            .Where(uv => uv.Word != null)
            .Select(uv => uv.Word!)
            .ToList();
    }
}
