using VibeLang.Models;

namespace VibeLang.Services;

public interface IVocabularyService
{
    /// <summary>Get all vocabulary words a user has learned</summary>
    Task<IEnumerable<VocabularyWord>> GetUserVocabularyAsync(string userId);
    
    /// <summary>Get progress status (Learned, Mastered) for each word</summary>
    Task<Dictionary<int, string>> GetUserVocabularyProgressAsync(string userId);
    
    /// <summary>Assign lesson vocabulary words to user in bulk (after lesson completion)</summary>
    Task AssignLessonVocabularyToUserAsync(string userId, int lessonId);
    
    /// <summary>Get user vocabulary statistics</summary>
    Task<(int Total, int Learned, int Mastered)> GetUserVocabularyStatsAsync(string userId);
    
    /// <summary>Mark a word as learned, mastered, etc.</summary>
    Task UpdateWordStatusAsync(string userId, int wordId, string status);
    
    /// <summary>Remove a word from user vocabulary</summary>
    Task RemoveWordAsync(string userId, int wordId);
}
