using VibeLang.Models;

namespace VibeLang.Repositories;

/// <summary>
/// Repository for managing vocabulary words at the LESSON level.
/// Separate from user vocabulary tracking - handles syncing words from lesson content.
/// </summary>
public interface ILessonVocabularyRepository
{
    /// <summary>Sync vocabulary words from lesson JSON content (one-time per lesson)</summary>
    Task SyncVocabularyFromLessonContentAsync(Lesson lesson);
    
    /// <summary>Get all words for a lesson</summary>
    Task<IEnumerable<VocabularyWord>> GetLessonVocabularyAsync(int lessonId);
    
    /// <summary>Check if word already exists in lesson (with upsert capability)</summary>
    Task<VocabularyWord?> GetOrCreateWordInLessonAsync(int lessonId, string word, string translation);
}
