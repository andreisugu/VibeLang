using Microsoft.EntityFrameworkCore;
using VibeLang.Models;
using VibeLang.Services;

namespace VibeLang.Browser.Services;

/// <summary>Port of LessonResultController.SubmitLesson for the browser.</summary>
public class LessonResultService
{
    private readonly VibeLangDbContext _db;
    private readonly BrowserAuthService _auth;
    private readonly IStatsService _stats;
    private readonly IAchievementService _achievements;
    private readonly IVocabularyService _vocabulary;
    private readonly ILessonVocabularyService _lessonVocabulary;
    private readonly SnapshotService _snapshot;

    public LessonResultService(
        VibeLangDbContext db,
        BrowserAuthService auth,
        IStatsService stats,
        IAchievementService achievements,
        IVocabularyService vocabulary,
        ILessonVocabularyService lessonVocabulary,
        SnapshotService snapshot)
    {
        _db = db;
        _auth = auth;
        _stats = stats;
        _achievements = achievements;
        _vocabulary = vocabulary;
        _lessonVocabulary = lessonVocabulary;
        _snapshot = snapshot;
    }

    public async Task<(bool Ok, int XpAdded, int TotalXp)> SubmitAsync(int lessonId, int score)
    {
        var user = await _auth.GetUserAsync();
        if (user is null) return (false, 0, 0);

        var lesson = await _db.Lessons.Include(l => l.Chapter).FirstOrDefaultAsync(l => l.Id == lessonId);
        if (lesson?.Chapter is null) return (false, 0, 0);

        var progress = await _db.UserLessonProgresses
            .FirstOrDefaultAsync(p => p.UserId == user.Id && p.LessonId == lessonId);

        if (progress is null)
        {
            _db.UserLessonProgresses.Add(new UserLessonProgress
            {
                UserId = user.Id,
                LessonId = lesson.Id,
                IsCompleted = true,
                ScoreAchieved = score,
                CompletionDate = DateTime.UtcNow,
            });
        }
        else
        {
            progress.IsCompleted = true;
            progress.ScoreAchieved = Math.Max(progress.ScoreAchieved, score);
            progress.CompletionDate = DateTime.UtcNow;
        }

        await _stats.UpdateUserStatsWithScoreAsync(user.Id, lesson.Chapter.CourseId, score);
        await _db.SaveChangesAsync();

        try
        {
            await _lessonVocabulary.SyncVocabularyAsync(lesson);
            await _vocabulary.AssignLessonVocabularyToUserAsync(user.Id, lesson.Id);
        }
        catch
        {
            // Vocabulary is secondary to lesson completion.
        }

        try
        {
            await _achievements.CheckAndAwardAchievementsAsync(user.Id, lesson.Id, lesson.Chapter.CourseId, score);
        }
        catch
        {
            // Achievements are secondary to lesson completion.
        }

        var updated = await _stats.GetOrCreateUserCourseStatsAsync(user.Id, lesson.Chapter.CourseId);
        await _snapshot.SaveAsync(_db);

        return (true, score, updated?.TotalXP ?? 0);
    }
}
