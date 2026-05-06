using VibeLang.Models;

namespace VibeLang.Repositories;

public interface IVocabularyWordRepository : IRepository<VocabularyWord>
{
    Task<IEnumerable<VocabularyWord>> GetWordsByLessonAsync(int lessonId);
    Task<bool> WordExistsInLessonAsync(int lessonId, string word);
}
