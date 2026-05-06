using VibeLang.Models;

namespace VibeLang.Services;

public interface IVocabularyService
{
    Task<IEnumerable<VocabularyWord>> GetUserVocabularyAsync(string userId);
    Task<Dictionary<int, string>> GetUserVocabularyProgressAsync(string userId);
    Task UpdateUserVocabularyAsync(string userId, int lessonId);
}
