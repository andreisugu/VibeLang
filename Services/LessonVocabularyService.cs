using VibeLang.Models;
using VibeLang.Repositories;

namespace VibeLang.Services;

public class LessonVocabularyService : ILessonVocabularyService
{
    private readonly ILessonVocabularyRepository _lessonVocabularyRepository;
    private readonly ILogger<LessonVocabularyService> _logger;

    public LessonVocabularyService(ILessonVocabularyRepository lessonVocabularyRepository, ILogger<LessonVocabularyService> logger)
    {
        _lessonVocabularyRepository = lessonVocabularyRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<VocabularyWord>> GetLessonVocabularyAsync(int lessonId)
    {
        return await _lessonVocabularyRepository.GetLessonVocabularyAsync(lessonId);
    }

    public async Task SyncVocabularyAsync(Lesson lesson)
    {
        if (string.IsNullOrEmpty(lesson.ContentJson))
        {
            _logger.LogWarning($"Lesson {lesson.Id} has no content JSON to sync vocabulary from");
            return;
        }

        try
        {
            await _lessonVocabularyRepository.SyncVocabularyFromLessonContentAsync(lesson);
            _logger.LogInformation($"Successfully synced vocabulary for lesson {lesson.Id}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to sync vocabulary for lesson {lesson.Id}: {ex.Message}");
            throw;
        }
    }
}
