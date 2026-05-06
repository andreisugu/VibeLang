using VibeLang.Models;

namespace VibeLang.Repositories;

public interface IUserVocabularyRepository : IRepository<UserVocabulary>
{
    Task<IEnumerable<UserVocabulary>> GetUserVocabulariesAsync(string userId);
    Task<bool> UserHasVocabularyAsync(string userId, int wordId);
    Task<IEnumerable<VocabularyWord>> GetUserVocabularyWordsAsync(string userId);
    
    /// <summary>Bulk assign words to user (efficient batch operation)</summary>
    Task AssignWordsToUserAsync(string userId, IEnumerable<int> wordIds);
    
    /// <summary>Update vocabulary status (Learned, Mastered, etc.)</summary>
    Task UpdateVocabularyStatusAsync(string userId, int wordId, string status);
    
    /// <summary>Remove word from user vocabulary</summary>
    Task RemoveFromUserVocabularyAsync(string userId, int wordId);
    
    /// <summary>Get user vocabulary statistics</summary>
    Task<(int Total, int Learned, int Mastered)> GetUserVocabularyStatsAsync(string userId);
}
