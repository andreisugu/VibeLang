using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using VibeLang.Models;

namespace VibeLang.Repositories;

public class LessonVocabularyRepository : ILessonVocabularyRepository
{
    private readonly VibeLangDbContext _context;
    private readonly ILogger<LessonVocabularyRepository> _logger;

    public LessonVocabularyRepository(VibeLangDbContext context, ILogger<LessonVocabularyRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<VocabularyWord>> GetLessonVocabularyAsync(int lessonId)
    {
        return await _context.VocabularyWords
            .Where(vw => vw.LessonId == lessonId)
            .ToListAsync();
    }

    public async Task<VocabularyWord?> GetOrCreateWordInLessonAsync(int lessonId, string word, string translation)
    {
        if (string.IsNullOrWhiteSpace(word) || string.IsNullOrWhiteSpace(translation))
            return null;

        // Normalize the word to prevent case-sensitivity duplicates
        var normalizedWord = word.Trim().ToLower();
        
        // Try to find existing word (unique constraint: lessonId + word)
        var existing = await _context.VocabularyWords
            .FirstOrDefaultAsync(vw => vw.LessonId == lessonId && vw.Word.ToLower() == normalizedWord);

        if (existing != null)
        {
            // Update translation if different
            if (existing.Translation != translation.Trim())
            {
                existing.Translation = translation.Trim();
                // Don't save here - caller will save in batch
            }
            return existing;
        }

        // Create new word (preserve original casing)
        var newWord = new VocabularyWord
        {
            LessonId = lessonId,
            Word = word.Trim(),
            Translation = translation.Trim()
        };

        _context.VocabularyWords.Add(newWord);
        // Don't save here - caller will save in batch
        return newWord;
    }

    public async Task SyncVocabularyFromLessonContentAsync(Lesson lesson)
    {
        if (string.IsNullOrEmpty(lesson.ContentJson)) return;

        try
        {
            using var doc = JsonDocument.Parse(lesson.ContentJson);
            var root = doc.RootElement;

            // 1. Sync from cuvinteInvatate (vocabulary list)
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

                    if (!string.IsNullOrWhiteSpace(word) && !string.IsNullOrWhiteSpace(translation) && word.Length <= 100)
                    {
                        await GetOrCreateWordInLessonAsync(lesson.Id, word, translation);
                    }
                }
            }

            // 2. Sync from teste (quiz questions - extract vocabulary)
            if (root.TryGetProperty("teste", out var tests) && tests.ValueKind == JsonValueKind.Array)
            {
                foreach (var test in tests.EnumerateArray())
                {
                    int tip = test.TryGetProperty("tip", out var tipProp) ? tipProp.GetInt32() : 0;

                    // Only extract vocabulary from translation tests (tip 1, 2) and matching pairs (tip 3)
                    // Skip tip 4 (context tests) - entire sentences are not useful vocabulary words
                    if (tip == 1 || tip == 2)
                    {
                        string? ro = null;
                        string? en = null;

                        if (tip == 1) // RO -> EN
                        {
                            ro = test.TryGetProperty("propozitie", out var p) ? p.GetString()?.Trim() : null;
                            en = test.TryGetProperty("raspunsCorrect", out var r) ? r.GetString()?.Trim() :
                                (test.TryGetProperty("raspunsCorect", out var r2) ? r2.GetString()?.Trim() : null);
                        }
                        else if (tip == 2) // EN -> RO
                        {
                            en = test.TryGetProperty("propozitie", out var p) ? p.GetString()?.Trim() : null;
                            ro = test.TryGetProperty("raspunsCorrect", out var r) ? r.GetString()?.Trim() :
                                (test.TryGetProperty("raspunsCorect", out var r2) ? r2.GetString()?.Trim() : null);
                        }

                        // Only add if both words exist, are reasonably short (not sentences), and not duplicates
                        if (!string.IsNullOrEmpty(ro) && !string.IsNullOrEmpty(en) && ro.Length <= 50 && en.Length <= 50)
                        {
                            await GetOrCreateWordInLessonAsync(lesson.Id, ro, en);
                        }
                    }
                    else if (tip == 3) // Matching pairs
                    {
                        if (test.TryGetProperty("leftWords", out var leftProp) && leftProp.ValueKind == JsonValueKind.Array &&
                            test.TryGetProperty("rightWords", out var rightProp) && rightProp.ValueKind == JsonValueKind.Array)
                        {
                            var left = leftProp.EnumerateArray().ToList();
                            var right = rightProp.EnumerateArray().ToList();

                            for (int i = 0; i < Math.Min(left.Count, right.Count); i++)
                            {
                                var lWord = left[i].GetString()?.Trim();
                                var rWord = right[i].GetString()?.Trim();

                                if (!string.IsNullOrEmpty(lWord) && !string.IsNullOrEmpty(rWord) && lWord.Length <= 50 && rWord.Length <= 50)
                                {
                                    await GetOrCreateWordInLessonAsync(lesson.Id, lWord, rWord);
                                }
                            }
                        }
                    }
                    // tip == 4 (context tests) is intentionally skipped - full sentences are not vocabulary
                }
            }

            // Save all changes once at the end (batch operation)
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Successfully synced vocabulary for lesson {lesson.Id}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error syncing vocabulary for lesson {lesson.Id}: {ex.Message}");
            throw;
        }
    }
}
