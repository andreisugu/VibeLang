using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using VibeLang.Models;
using VibeLang.Repositories;

namespace VibeLang.Services;

public class VocabularyService : IVocabularyService
{
    private readonly IUserVocabularyRepository _userVocabularyRepository;
    private readonly IVocabularyWordRepository _vocabularyWordRepository;
    private readonly VibeLangDbContext _context;
    private readonly ILogger<VocabularyService> _logger;

    public VocabularyService(IUserVocabularyRepository userVocabularyRepository,
        IVocabularyWordRepository vocabularyWordRepository,
        VibeLangDbContext context,
        ILogger<VocabularyService> logger)
    {
        _userVocabularyRepository = userVocabularyRepository;
        _vocabularyWordRepository = vocabularyWordRepository;
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<VocabularyWord>> GetUserVocabularyAsync(string userId)
    {
        return await _userVocabularyRepository.GetUserVocabularyWordsAsync(userId);
    }

    public async Task<Dictionary<int, string>> GetUserVocabularyProgressAsync(string userId)
    {
        var userVocabEntries = await _userVocabularyRepository.GetUserVocabulariesAsync(userId);
        return userVocabEntries
            .Where(uv => uv.Word != null)
            .GroupBy(uv => uv.WordId)
            .ToDictionary(g => g.Key, g => g.First().Status);
    }

    public async Task UpdateUserVocabularyAsync(string userId, int lessonId)
    {
        try
        {
            var lesson = await _context.Lessons
                .Include(l => l.VocabularyWords)
                .FirstOrDefaultAsync(l => l.Id == lessonId);

            if (lesson == null) return;

            // 1. If VocabularyWords table is empty, try to sync from ContentJson
            if (!lesson.VocabularyWords.Any() && !string.IsNullOrEmpty(lesson.ContentJson))
            {
                await SyncVocabularyFromContentJsonAsync(lesson);
            }

            // 2. Fetch all words (including newly synced ones)
            var words = await _vocabularyWordRepository.GetWordsByLessonAsync(lessonId);

            foreach (var word in words)
            {
                // Extra check to prevent duplicates
                bool hasVocab = await _userVocabularyRepository.UserHasVocabularyAsync(userId, word.Id);
                if (!hasVocab)
                {
                    await _userVocabularyRepository.AddAsync(new UserVocabulary
                    {
                        UserId = userId,
                        WordId = word.Id,
                        Status = "Learned",
                        LastReviewed = DateTime.UtcNow
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating user vocabulary: {ex.Message}");
        }
    }

    private async Task SyncVocabularyFromContentJsonAsync(Lesson lesson)
    {
        try
        {
            using var doc = JsonDocument.Parse(lesson.ContentJson!);
            var root = doc.RootElement;

            // 1. Sync from cuvinteInvatate
            if (root.TryGetProperty("cuvinteInvatate", out var wordList) && wordList.ValueKind == JsonValueKind.Array)
            {
                foreach (var wordEl in wordList.EnumerateArray())
                {
                    string? word = null;
                    string? translation = null;

                    if (wordEl.ValueKind == JsonValueKind.Object)
                    {
                        word = wordEl.TryGetProperty("word", out var w) ? w.GetString() : null;
                        translation = wordEl.TryGetProperty("translation", out var t) ? t.GetString() : null;
                    }
                    else if (wordEl.ValueKind == JsonValueKind.String)
                    {
                        word = wordEl.GetString();
                        translation = word;
                    }

                    if (!string.IsNullOrWhiteSpace(word) && !string.IsNullOrWhiteSpace(translation))
                    {
                        if (word.Length > 30) continue;

                        bool exists = await _vocabularyWordRepository.WordExistsInLessonAsync(lesson.Id, word);
                        if (!exists)
                        {
                            await _vocabularyWordRepository.AddAsync(new VocabularyWord
                            {
                                Word = word,
                                Translation = translation,
                                LessonId = lesson.Id
                            });
                        }
                    }
                }
            }

            // 2. Sync from teste
            if (root.TryGetProperty("teste", out var tests) && tests.ValueKind == JsonValueKind.Array)
            {
                foreach (var test in tests.EnumerateArray())
                {
                    int tip = test.TryGetProperty("tip", out var tipProp) ? tipProp.GetInt32() : 0;

                    if (tip == 1 || tip == 2 || tip == 4)
                    {
                        string? ro = null;
                        string? en = null;

                        if (tip == 1)
                        {
                            ro = test.TryGetProperty("propozitie", out var p) ? p.GetString() : null;
                            en = test.TryGetProperty("raspunsCorrect", out var r) ? r.GetString() : (test.TryGetProperty("raspunsCorect", out var r2) ? r2.GetString() : null);
                        }
                        else if (tip == 2)
                        {
                            en = test.TryGetProperty("propozitie", out var p) ? p.GetString() : null;
                            ro = test.TryGetProperty("raspunsCorrect", out var r) ? r.GetString() : (test.TryGetProperty("raspunsCorect", out var r2) ? r2.GetString() : null);
                        }
                        else if (tip == 4)
                        {
                            ro = test.TryGetProperty("propozitie", out var p) ? p.GetString() : null;
                            en = ro;
                        }

                        if (!string.IsNullOrEmpty(ro) && !string.IsNullOrEmpty(en))
                        {
                            if (ro.Length > 30) continue;

                            bool exists = await _vocabularyWordRepository.WordExistsInLessonAsync(lesson.Id, ro);
                            if (!exists)
                            {
                                await _vocabularyWordRepository.AddAsync(new VocabularyWord
                                {
                                    Word = ro,
                                    Translation = en,
                                    LessonId = lesson.Id
                                });
                            }
                        }
                    }
                    else if (tip == 3)
                    {
                        if (test.TryGetProperty("leftWords", out var leftProp) && leftProp.ValueKind == JsonValueKind.Array &&
                            test.TryGetProperty("rightWords", out var rightProp) && rightProp.ValueKind == JsonValueKind.Array)
                        {
                            var left = leftProp.EnumerateArray().ToList();
                            var right = rightProp.EnumerateArray().ToList();

                            for (int i = 0; i < Math.Min(left.Count, right.Count); i++)
                            {
                                var lWord = left[i].GetString();
                                var rWord = right[i].GetString();

                                if (!string.IsNullOrEmpty(lWord) && !string.IsNullOrEmpty(rWord))
                                {
                                    if (lWord.Length > 30) continue;

                                    bool exists = await _vocabularyWordRepository.WordExistsInLessonAsync(lesson.Id, lWord);
                                    if (!exists)
                                    {
                                        await _vocabularyWordRepository.AddAsync(new VocabularyWord
                                        {
                                            Word = lWord,
                                            Translation = rWord,
                                            LessonId = lesson.Id
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Failed to sync vocabulary from JSON for lesson {lesson.Id}: {ex.Message}");
        }
    }
}
