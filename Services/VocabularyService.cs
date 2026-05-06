using VibeLang.Models;
using VibeLang.Repositories;

namespace VibeLang.Services;

/// <summary>
/// Service for managing USER vocabulary (what they've learned).
/// Does NOT handle lesson-level syncing - use LessonVocabularyService for that.
/// Focuses on: assigning words, tracking status, statistics.
/// </summary>
public class VocabularyService : IVocabularyService
{
    private readonly IUserVocabularyRepository _userVocabularyRepository;
    private readonly IVocabularyWordRepository _vocabularyWordRepository;
    private readonly ILogger<VocabularyService> _logger;

    public VocabularyService(IUserVocabularyRepository userVocabularyRepository,
        IVocabularyWordRepository vocabularyWordRepository,
        ILogger<VocabularyService> logger)
    {
        _userVocabularyRepository = userVocabularyRepository;
        _vocabularyWordRepository = vocabularyWordRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<VocabularyWord>> GetUserVocabularyAsync(string userId)
    {
        return await _userVocabularyRepository.GetUserVocabularyWordsAsync(userId);
    }

    public async Task<Dictionary<int, string>> GetUserVocabularyProgressAsync(string userId)
    {
        var userVocabEntries = await _userVocabularyRepository.GetUserVocabulariesAsync(userId);
        
        // Group by word content and take the best status (Mastered > Learned > New)
        var progressMap = userVocabEntries
            .Where(uv => uv.Word != null)
            .GroupBy(uv => new { uv.Word!.Word, uv.Word.Translation })
            .ToDictionary(
                g => g.First().Word!.Id,  // Use first WordId as key
                g => DetermineBestStatus(g.Select(uv => uv.Status))
            );
        
        return progressMap;
    }

    private string DetermineBestStatus(IEnumerable<string> statuses)
    {
        // Priority: Mastered > Learned > New
        if (statuses.Contains("Mastered")) return "Mastered";
        if (statuses.Contains("Learned")) return "Learned";
        return "New";
    }

    public async Task<(int Total, int Learned, int Mastered)> GetUserVocabularyStatsAsync(string userId)
    {
        return await _userVocabularyRepository.GetUserVocabularyStatsAsync(userId);
    }

    public async Task UpdateWordStatusAsync(string userId, int wordId, string status)
    {
        await _userVocabularyRepository.UpdateVocabularyStatusAsync(userId, wordId, status);
    }

    public async Task RemoveWordAsync(string userId, int wordId)
    {
        await _userVocabularyRepository.RemoveFromUserVocabularyAsync(userId, wordId);
    }

    /// <summary>
    /// Assign all lesson vocabulary words to user in ONE batch operation.
    /// Efficient: single query to check existing, single bulk insert for new words.
    /// Handles race conditions gracefully.
    /// </summary>
    public async Task AssignLessonVocabularyToUserAsync(string userId, int lessonId)
    {
        try
        {
            // Get all vocabulary words for this lesson
            var lessonWords = await _vocabularyWordRepository.GetWordsByLessonAsync(lessonId);
            if (!lessonWords.Any())
            {
                _logger.LogWarning($"No vocabulary words found for lesson {lessonId}");
                return;
            }

            // Bulk assign words to user
            var wordIds = lessonWords.Select(w => w.Id);
            await _userVocabularyRepository.AssignWordsToUserAsync(userId, wordIds);

            _logger.LogInformation($"Assigned {lessonWords.Count()} vocabulary words from lesson {lessonId} to user {userId}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error assigning vocabulary for user {userId}, lesson {lessonId}: {ex.Message}");
            throw;
        }
    }
}
