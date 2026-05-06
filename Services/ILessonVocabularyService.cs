using VibeLang.Models;

namespace VibeLang.Services;

/// <summary>
/// Service for managing vocabulary at the LESSON level (syncing from content JSON).
/// Separate from user vocabulary tracking - handles one-time sync per lesson.
/// </summary>
public interface ILessonVocabularyService
{
    /// <summary>Sync vocabulary words from lesson JSON content (one-time operation)</summary>
    Task SyncVocabularyAsync(Lesson lesson);
    
    /// <summary>Get all vocabulary words for a lesson</summary>
    Task<IEnumerable<VocabularyWord>> GetLessonVocabularyAsync(int lessonId);
}
