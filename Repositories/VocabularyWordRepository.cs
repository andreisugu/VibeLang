using Microsoft.EntityFrameworkCore;
using VibeLang.Models;

namespace VibeLang.Repositories;

public class VocabularyWordRepository : Repository<VocabularyWord>, IVocabularyWordRepository
{
    public VocabularyWordRepository(VibeLangDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<VocabularyWord>> GetWordsByLessonAsync(int lessonId)
    {
        return await _context.VocabularyWords
            .Where(w => w.LessonId == lessonId)
            .ToListAsync();
    }

    public async Task<bool> WordExistsInLessonAsync(int lessonId, string word)
    {
        return await _context.VocabularyWords
            .AnyAsync(vw => vw.LessonId == lessonId && vw.Word == word);
    }
}
