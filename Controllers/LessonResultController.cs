using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VibeLang.Models;
using VibeLang.Services;

namespace VibeLang.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class LessonResultController : ControllerBase
{
    private readonly VibeLangDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IStatsService _statsService;
    private readonly IQuizService _quizService;
    private readonly IAchievementService _achievementService;
    private readonly IVocabularyService _vocabularyService;
    private readonly ILessonVocabularyService _lessonVocabularyService;
    private readonly ILogger<LessonResultController> _logger;

    public LessonResultController(VibeLangDbContext context, UserManager<ApplicationUser> userManager,
        IStatsService statsService, IQuizService quizService, IAchievementService achievementService, 
        IVocabularyService vocabularyService, ILessonVocabularyService lessonVocabularyService,
        ILogger<LessonResultController> logger)
    {
        _context = context;
        _userManager = userManager;
        _statsService = statsService;
        _quizService = quizService;
        _achievementService = achievementService;
        _vocabularyService = vocabularyService;
        _lessonVocabularyService = lessonVocabularyService;
        _logger = logger;
    }

    [HttpPost("submit-lesson")]
    public async Task<IActionResult> SubmitLesson([FromBody] LessonResult result)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var lesson = await _context.Lessons
            .Include(l => l.Chapter)
            .FirstOrDefaultAsync(l => l.Id == result.LessonId);
        
        if (lesson == null) return NotFound();
        if (lesson.Chapter == null) return BadRequest("Lesson has no associated chapter");


        // 1. Update Lesson Progress
        var progress = await _context.UserLessonProgresses
            .FirstOrDefaultAsync(p => p.UserId == user.Id && p.LessonId == lesson.Id);
        
        if (progress == null)
        {
            progress = new UserLessonProgress
            {
                UserId = user.Id,
                LessonId = lesson.Id,
                IsCompleted = true,
                ScoreAchieved = result.Score,
                CompletionDate = DateTime.UtcNow
            };
            _context.UserLessonProgresses.Add(progress);
        }
        else
        {
            progress.IsCompleted = true;
            progress.ScoreAchieved = Math.Max(progress.ScoreAchieved, result.Score);
            progress.CompletionDate = DateTime.UtcNow;
        }

        // 2. Update Course Stats using stats service
        int xpGained = result.Score;
        await _statsService.UpdateUserStatsWithScoreAsync(user.Id, lesson.Chapter.CourseId, xpGained);

        await _context.SaveChangesAsync();
        
        try
        {
            // 3. Sync vocabulary from lesson JSON (if not already synced)
            await _lessonVocabularyService.SyncVocabularyAsync(lesson);
            
            // 4. Add words to User Vocabulary
            await _vocabularyService.AssignLessonVocabularyToUserAsync(user.Id, lesson.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to sync vocabulary for user {user.Id}, lesson {lesson.Id}: {ex.Message}");
            // Don't fail the entire operation - vocabulary is secondary to lesson completion
        }

        try
        {
            // 5. Check and award achievements
            await _achievementService.CheckAndAwardAchievementsAsync(user.Id, lesson.Id, lesson.Chapter.CourseId, result.Score);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to check achievements for user {user.Id}: {ex.Message}");
            // Don't fail the entire operation
        }

        var updatedStats = await _statsService.GetOrCreateUserCourseStatsAsync(user.Id, lesson.Chapter.CourseId);
        if (updatedStats == null) return BadRequest("Failed to update user stats");

        return Ok(new { success = true, xpAdded = xpGained, totalXP = updatedStats.TotalXP });
    }

    [HttpPost("submit-quiz")]
    public async Task<IActionResult> SubmitQuiz([FromBody] QuizSubmission submission)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var quiz = await _quizService.GetQuizWithQuestionsAsync(submission.QuizId);
        if (quiz == null) return NotFound();

        // Calculate score using quiz service
        var (correct, total, score) = await _quizService.CalculateQuizScoreAsync(submission.QuizId, submission.Answers);

        // Update User Lesson Progress
        var lesson = quiz.Lesson;
        if (lesson == null) return BadRequest("Quiz has no associated lesson");
        
        var progress = await _context.UserLessonProgresses
            .FirstOrDefaultAsync(p => p.UserId == user.Id && p.LessonId == lesson.Id);
        
        if (progress == null)
        {
            progress = new UserLessonProgress
            {
                UserId = user.Id,
                LessonId = lesson.Id,
                IsCompleted = true,
                ScoreAchieved = score,
                CompletionDate = DateTime.UtcNow
            };
            _context.UserLessonProgresses.Add(progress);
        }
        else
        {
            progress.IsCompleted = true;
            progress.ScoreAchieved = Math.Max(progress.ScoreAchieved, score);
            progress.CompletionDate = DateTime.UtcNow;
        }

        // Update Course Stats using stats service
        int xpGained = score;
        if (lesson.Chapter == null) return BadRequest("Lesson has no associated chapter");
        
        await _statsService.UpdateUserStatsWithScoreAsync(user.Id, lesson.Chapter.CourseId, xpGained);

        await _context.SaveChangesAsync();

        try
        {
            // Sync vocabulary from lesson JSON (if not already synced)
            await _lessonVocabularyService.SyncVocabularyAsync(lesson);

            // Add words to User Vocabulary
            await _vocabularyService.AssignLessonVocabularyToUserAsync(user.Id, lesson.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to sync vocabulary for user {user.Id}, lesson {lesson.Id}: {ex.Message}");
            // Don't fail the entire operation - vocabulary is secondary to quiz completion
        }

        try
        {
            // Check and award achievements
            await _achievementService.CheckAndAwardAchievementsAsync(user.Id, lesson.Id, lesson.Chapter.CourseId, score);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to check achievements for user {user.Id}: {ex.Message}");
            // Don't fail the entire operation
        }

        var updatedStats = await _statsService.GetOrCreateUserCourseStatsAsync(user.Id, lesson.Chapter.CourseId);
        if (updatedStats == null) return BadRequest("Failed to update user stats");

        return Ok(new { 
            success = true, 
            score = correct, 
            total = total, 
            correct = correct,
            percentage = score,
            xpAdded = xpGained,
            totalXP = updatedStats.TotalXP
        });
    }

    public class LessonResult
    {
        public int LessonId { get; set; }
        public int Score { get; set; }
    }

    public class QuizSubmission
    {
        public int QuizId { get; set; }
        public Dictionary<string, string> Answers { get; set; } = new();
    }
}
