using VibeLang.Models;

namespace VibeLang.Repositories;

public interface IUserVocabularyRepository : IRepository<UserVocabulary>
{
    Task<IEnumerable<UserVocabulary>> GetUserVocabulariesAsync(string userId);
    Task<bool> UserHasVocabularyAsync(string userId, int wordId);
    Task<IEnumerable<VocabularyWord>> GetUserVocabularyWordsAsync(string userId);
}
