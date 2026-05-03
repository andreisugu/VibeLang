using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using VibeLang.Models;
using Microsoft.AspNetCore.Authorization;

namespace VibeLang.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly VibeLangDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(ILogger<HomeController> logger, VibeLangDbContext context, UserManager<ApplicationUser> userManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Leaderboard()
    {
        // Fetch top stats, including the Course and User name for the leaderboard
        var stats = await _context.UserCourseStats
            .Include(s => s.Course)
            .Include(s => s.User)
            .OrderByDescending(s => s.TotalXP)
            .Take(10)
            .ToListAsync();
        return View(stats);
    }

    public async Task<IActionResult> Lesson(int? id)
    {
        Lesson? lesson;
        if (id.HasValue)
        {
            lesson = await _context.Lessons
                .Include(l => l.Chapter)
                .FirstOrDefaultAsync(l => l.Id == id.Value);
        }
        else
        {
            lesson = await _context.Lessons
                .Include(l => l.Chapter)
                .OrderBy(l => l.Id)
                .FirstOrDefaultAsync();
        }
        
        return View(lesson);
    }

    [HttpGet]
    public async Task<IActionResult> GetLessonData(int id)
    {
        var lesson = await _context.Lessons.FindAsync(id);
        if (lesson == null || string.IsNullOrEmpty(lesson.ContentJson))
        {
            return NotFound();
        }
        return Content(lesson.ContentJson, "application/json");
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitResult([FromBody] LessonResult result)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var lesson = await _context.Lessons
            .Include(l => l.Chapter)
            .FirstOrDefaultAsync(l => l.Id == result.LessonId);
        
        if (lesson == null) return NotFound();

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

        // 2. Update Course Stats (XP and Streak)
        var stats = await _context.UserCourseStats
            .FirstOrDefaultAsync(s => s.UserId == user.Id && s.CourseId == lesson.Chapter!.CourseId);
        
        if (stats == null)
        {
            stats = new UserCourseStats
            {
                UserId = user.Id,
                CourseId = lesson.Chapter!.CourseId,
                TotalXP = result.Score,
                CurrentStreak = 1,
                LastActivityDate = DateTime.UtcNow
            };
            _context.UserCourseStats.Add(stats);
        }
        else
        {
            stats.TotalXP += result.Score;
            
            // Streak logic
            var today = DateTime.UtcNow.Date;
            var lastDate = stats.LastActivityDate?.Date;

            if (lastDate != today)
            {
                if (lastDate == today.AddDays(-1))
                {
                    stats.CurrentStreak++; // consecutive day
                }
                else
                {
                    stats.CurrentStreak = 1; // broke the streak
                }
                stats.LastActivityDate = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();
        
        // 3. Add words to User Vocabulary
        await UpdateUserVocabulary(user.Id, lesson.Id);

        // 4. Check and award achievements
        await CheckAndAwardAchievements(user.Id, lesson.Id, lesson.Chapter!.CourseId, result.Score);

        return Json(new { success = true, xpAdded = result.Score, totalXP = stats.TotalXP });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitQuiz([FromBody] QuizSubmission submission)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var quiz = await _context.Quizzes
            .Include(q => q.Questions)
            .ThenInclude(q => q.Options)
            .Include(q => q.Lesson)
            .ThenInclude(l => l.Chapter)
            .FirstOrDefaultAsync(q => q.Id == submission.QuizId);
        
        if (quiz == null) return NotFound();

        // Calculate score
        int correct = 0;
        int total = quiz.Questions.Count;

        foreach (var question in quiz.Questions)
        {
            if (submission.Answers.TryGetValue(question.Id.ToString(), out var selectedOptionIdStr) &&
                int.TryParse(selectedOptionIdStr, out var selectedOptionId))
            {
                var selectedOption = question.Options.FirstOrDefault(o => o.Id == selectedOptionId);
                if (selectedOption?.IsCorrect ?? false)
                {
                    correct++;
                }
            }
        }

        // Calculate percentage score (0-100)
        int score = total > 0 ? (correct * 100) / total : 0;

        // Update User Lesson Progress
        var lesson = quiz.Lesson;
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

        // Update Course Stats
        var stats = await _context.UserCourseStats
            .FirstOrDefaultAsync(s => s.UserId == user.Id && s.CourseId == lesson.Chapter!.CourseId);
        
        int xpGained = score; // Award XP equal to score percentage
        
        if (stats == null)
        {
            stats = new UserCourseStats
            {
                UserId = user.Id,
                CourseId = lesson.Chapter!.CourseId,
                TotalXP = xpGained,
                CurrentStreak = 1,
                LastActivityDate = DateTime.UtcNow
            };
            _context.UserCourseStats.Add(stats);
        }
        else
        {
            stats.TotalXP += xpGained;
            
            // Streak logic
            var today = DateTime.UtcNow.Date;
            var lastDate = stats.LastActivityDate?.Date;

            if (lastDate != today)
            {
                if (lastDate == today.AddDays(-1))
                {
                    stats.CurrentStreak++; // consecutive day
                }
                else
                {
                    stats.CurrentStreak = 1; // broke the streak
                }
                stats.LastActivityDate = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();

        // Add words to User Vocabulary
        await UpdateUserVocabulary(user.Id, lesson.Id);

        // Check and award achievements
        await CheckAndAwardAchievements(user.Id, lesson.Id, lesson.Chapter!.CourseId, score);

        return Json(new { 
            success = true, 
            score = correct, 
            total = total, 
            correct = correct,
            percentage = score,
            xpAdded = xpGained,
            totalXP = stats.TotalXP
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

    public async Task<IActionResult> Vocabulary()
    {
        var user = await _userManager.GetUserAsync(User);

        var words = new List<VocabularyWord>();
        var userProgress = new Dictionary<int, string>();

        if (user != null)
        {
            // Source of truth: what THIS user has in their vocabulary
            var userVocabEntries = await _context.UserVocabularies
                .Where(uv => uv.UserId == user.Id)
                .Include(uv => uv.Word)
                    .ThenInclude(w => w!.Lesson)
                        .ThenInclude(l => l!.Chapter)
                .ToListAsync();

            words = userVocabEntries
                .Where(uv => uv.Word != null)
                .Select(uv => uv.Word!)
                .ToList();

            userProgress = userVocabEntries
                .Where(uv => uv.Word != null)
                .GroupBy(uv => uv.WordId)
                .ToDictionary(g => g.Key, g => g.First().Status);
        }

        var courses = await _context.Courses.ToListAsync();
        ViewBag.Courses = courses;

        var lessonCourseMap = new Dictionary<int, int>();
        foreach (var word in words)
        {
            if (word.Lesson?.Chapter != null && !lessonCourseMap.ContainsKey(word.LessonId))
                lessonCourseMap[word.LessonId] = word.Lesson.Chapter.CourseId;
        }

        ViewBag.LessonCourseMap = lessonCourseMap;
        ViewBag.UserProgress = userProgress;

        return View(words);
    }


    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        ViewBag.FirstName = user?.FirstName ?? user?.UserName ?? "Student";

        int? lastLessonId = null;

        if (user != null)
        {
            // 1. Get user's latest lesson activity
            var lastProgress = await _context.UserLessonProgresses
                .Include(p => p.Lesson)
                    .ThenInclude(l => l!.Chapter)
                .Where(p => p.UserId == user.Id)
                .OrderByDescending(p => p.CompletionDate)
                .FirstOrDefaultAsync();

            if (lastProgress != null && lastProgress.Lesson != null)
            {
                // If the last lesson was passed (score >= 50), try to find the NEXT one
                if (lastProgress.ScoreAchieved >= 50)
                {
                    // Find next lesson in same chapter
                    var nextLesson = await _context.Lessons
                        .Where(l => l.ChapterId == lastProgress.Lesson.ChapterId && l.Order > lastProgress.Lesson.Order)
                        .OrderBy(l => l.Order)
                        .FirstOrDefaultAsync();

                    if (nextLesson != null)
                    {
                        lastLessonId = nextLesson.Id;
                    }
                    else
                    {
                        // Try next chapter in same course
                        var nextChapter = await _context.Chapters
                            .Where(c => c.CourseId == lastProgress.Lesson.Chapter!.CourseId && c.Order > lastProgress.Lesson.Chapter!.Order)
                            .OrderBy(c => c.Order)
                            .FirstOrDefaultAsync();

                        if (nextChapter != null)
                        {
                            var firstLessonInNextChapter = await _context.Lessons
                                .Where(l => l.ChapterId == nextChapter.Id)
                                .OrderBy(l => l.Order)
                                .FirstOrDefaultAsync();
                            lastLessonId = firstLessonInNextChapter?.Id;
                        }
                    }
                }
                
                // If we didn't find a next lesson, or the last one was failed, just use the last one
                if (lastLessonId == null)
                {
                    lastLessonId = lastProgress.LessonId;
                }
            }

            // Fallback for new users: get first lesson ever
            if (lastLessonId == null)
            {
                var firstLesson = await _context.Lessons.OrderBy(l => l.Id).FirstOrDefaultAsync();
                lastLessonId = firstLesson?.Id;
            }

            if (lastLessonId != null)
            {
                ViewBag.ContinueLesson = await _context.Lessons
                    .Include(l => l.Chapter)
                        .ThenInclude(c => c!.Course)
                    .FirstOrDefaultAsync(l => l.Id == lastLessonId);
            }

            // 2. Show the user's overall stats on the dashboard
            var stats = await _context.UserCourseStats
                .Where(s => s.UserId == user.Id)
                .OrderByDescending(s => s.TotalXP)
                .FirstOrDefaultAsync();
            
            // 3. Count lessons completed
            var lessonsCompleted = await _context.UserLessonProgresses
                .Where(ulp => ulp.UserId == user.Id && ulp.IsCompleted)
                .CountAsync();
            ViewBag.LessonsCompleted = lessonsCompleted;

            // 4. Calculate user level (1 level per 500 XP)
            ViewBag.CurrentLevel = Math.Max(1, ((stats?.TotalXP ?? 0) / 500) + 1);

            // 5. Get achievements count
            var achievementCount = await _context.UserAchievements
                .Where(ua => ua.UserId == user.Id)
                .CountAsync();
            ViewBag.CurrentAchievementCount = achievementCount;

            // 6. Get course progress for each course
            var courses = await _context.Courses
                .Include(c => c.Chapters)
                    .ThenInclude(ch => ch.Lessons)
                .ToListAsync();

            var courseProgress = new List<dynamic>();
            foreach (var course in courses)
            {
                var totalLessons = course.Chapters.SelectMany(ch => ch.Lessons).Count();
                if (totalLessons == 0) continue;

                var completedLessons = await _context.UserLessonProgresses
                    .Where(ulp => ulp.UserId == user.Id && ulp.IsCompleted && 
                           ulp.Lesson.Chapter!.CourseId == course.Id)
                    .CountAsync();

                courseProgress.Add(new
                {
                    CourseName = course.Title,
                    TotalLessons = totalLessons,
                    CompletedLessons = completedLessons
                });
            }
            ViewBag.CourseProgress = courseProgress;

            // 7. Get recent activity (last 5 completed lessons)
            var recentActivity = await _context.UserLessonProgresses
                .Where(ulp => ulp.UserId == user.Id && ulp.IsCompleted)
                .Include(ulp => ulp.Lesson)
                    .ThenInclude(l => l.Chapter)
                        .ThenInclude(ch => ch!.Course)
                .OrderByDescending(ulp => ulp.CompletionDate)
                .Take(5)
                .Select(ulp => new
                {
                    LessonTitle = ulp.Lesson!.Title,
                    ChapterTitle = ulp.Lesson!.Chapter!.Title,
                    CourseName = ulp.Lesson!.Chapter!.Course!.Title,
                    Score = ulp.ScoreAchieved,
                    CompletionDate = ulp.CompletionDate
                })
                .ToListAsync();
            ViewBag.RecentActivity = recentActivity;

            ViewBag.LastLessonId = lastLessonId;
            return View(stats);
        }
        
        return View();
    }

    public async Task<IActionResult> Courses()
    {
        var user = await _userManager.GetUserAsync(User);

        // Fetch all courses with their chapters and lessons
        var courses = await _context.Courses
            .Include(c => c.Chapters.OrderBy(ch => ch.Order))
                .ThenInclude(ch => ch.Lessons.OrderBy(l => l.Order))
            .ToListAsync();

        if (user != null)
        {
            // Get completed lesson IDs for the current user
            var completedLessonIds = await _context.UserLessonProgresses
                .Where(ulp => ulp.UserId == user.Id && ulp.IsCompleted)
                .Select(ulp => ulp.LessonId)
                .ToListAsync();
            
            ViewBag.CompletedLessonIds = completedLessonIds;
        }
        else
        {
            ViewBag.CompletedLessonIds = new List<int>();
        }

        return View(courses);
    }

    public IActionResult Profile()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(string firstName, string lastName)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        user.FirstName = firstName;
        user.LastName = lastName;

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            TempData["StatusMessage"] = "Profile updated successfully!";
        }
        else
        {
            TempData["StatusMessage"] = "Error updating profile.";
        }

        return RedirectToAction(nameof(Profile));
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public async Task<IActionResult> Achievements()
    {
        var user = await _userManager.GetUserAsync(User);
        
        if (user != null)
        {
            await CheckAchievementsForAllCompletedLessons(user.Id);
        }
        
        var allAchievements = await _context.Achievements.ToListAsync();
        var earnedAchievements = new Dictionary<int, DateTime>();
        
        if (user != null)
        {
            earnedAchievements = await _context.UserAchievements
                .Where(ua => ua.UserId == user.Id)
                .ToDictionaryAsync(ua => ua.AchievementId, ua => ua.EarnedDate);
        }

        ViewBag.AllAchievements = allAchievements;
        ViewBag.EarnedAchievementIds = earnedAchievements.Keys.ToHashSet();
        ViewBag.EarnedAchievements = earnedAchievements;
        ViewBag.TotalEarned = earnedAchievements.Count;
        ViewBag.TotalAvailable = allAchievements.Count;

        // Calculate progress for each achievement type
        var progressData = new Dictionary<string, dynamic>();
        if (user != null)
        {
            var completedLessons = await _context.UserLessonProgresses
                .Where(ulp => ulp.UserId == user.Id && ulp.IsCompleted)
                .Include(ulp => ulp.Lesson).ThenInclude(l => l!.Chapter)
                .ToListAsync();

            // 1. Beginner's Start
            progressData["Beginner's Start"] = new { Current = Math.Min(completedLessons.Count, 1), Target = 1, Text = $"{Math.Min(completedLessons.Count, 1)}/1 lesson completed" };

            // 2. Perfect Score
            var bestScore = completedLessons.Any() ? completedLessons.Max(ul => ul.ScoreAchieved) : 0;
            progressData["Perfect Score"] = new { Current = bestScore, Target = 100, Text = $"Best score: {bestScore}%" };

            // 3. Expert Learner
            int bestCoursePercent = 0;
            string bestCourseText = "0 lessons";
            var courseIds = completedLessons.Where(ul => ul.Lesson?.Chapter != null).Select(ul => ul.Lesson!.Chapter!.CourseId).Distinct();
            foreach (var courseId in courseIds)
            {
                var total = await _context.Lessons.CountAsync(l => l.Chapter!.CourseId == courseId);
                var completed = completedLessons.Count(ul => ul.Lesson?.Chapter?.CourseId == courseId);
                int percent = total > 0 ? (int)((double)completed / total * 100) : 0;
                if (percent >= bestCoursePercent)
                {
                    bestCoursePercent = percent;
                    bestCourseText = $"{completed}/{total} lessons in best course";
                }
            }
            progressData["Expert Learner"] = new { Current = bestCoursePercent, Target = 100, Text = bestCourseText };

            // 4. Speed Learner
            var bestDayCount = completedLessons.GroupBy(ul => ul.CompletionDate.Date).Select(g => g.Count()).DefaultIfEmpty(0).Max();
            progressData["Speed Learner"] = new { Current = Math.Min(bestDayCount, 5), Target = 5, Text = $"{bestDayCount}/5 lessons in one day (best day)" };

            // 5. Marathon Runner
            var maxStreak = await _context.UserCourseStats.Where(s => s.UserId == user.Id).MaxAsync(s => (int?)s.CurrentStreak) ?? 0;
            progressData["Marathon Runner"] = new { Current = Math.Min(maxStreak, 7), Target = 7, Text = $"{maxStreak}/7 day streak" };

            // 6. Social Butterfly
            int bestRank = 100; // Default high rank
            var userStats = await _context.UserCourseStats.Where(s => s.UserId == user.Id).ToListAsync();
            foreach (var stat in userStats)
            {
                var usersAbove = await _context.UserCourseStats.CountAsync(s => s.CourseId == stat.CourseId && s.TotalXP > stat.TotalXP);
                int rank = usersAbove + 1;
                if (rank < bestRank) bestRank = rank;
            }
            int rankProgress = bestRank <= 10 ? 100 : (bestRank > 100 ? 0 : 100 - (bestRank - 10));
            progressData["Social Butterfly"] = new { Current = Math.Max(0, rankProgress), Target = 100, Text = $"Current best rank: #{bestRank}" };
        }
        
        ViewBag.AchievementProgress = progressData;
        return View(allAchievements);
    }

    private async Task UpdateUserVocabulary(string userId, int lessonId)
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
                await SyncVocabularyFromContentJson(lesson);
            }

            // 2. Fetch all words (including newly synced ones)
            var words = await _context.VocabularyWords
                .Where(w => w.LessonId == lessonId)
                .ToListAsync();

            foreach (var word in words)
            {
                // Extra check to prevent duplicates even if the table has dirty data
                var existing = await _context.UserVocabularies
                    .AnyAsync(uv => uv.UserId == userId && uv.WordId == word.Id);

                if (!existing)
                {
                    _context.UserVocabularies.Add(new UserVocabulary
                    {
                        UserId = userId,
                        WordId = word.Id,
                        Status = "Learned",
                        LastReviewed = DateTime.UtcNow
                    });
                }
            }

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating user vocabulary: {ex.Message}");
        }
    }

    private async Task SyncVocabularyFromContentJson(Lesson lesson)
    {
        try
        {
            using var doc = JsonDocument.Parse(lesson.ContentJson!);
            var root = doc.RootElement;

            // 1. Sync from cuvinteInvatate (Option A: Support for {word, translation} objects)
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
                        translation = word; // Fallback
                    }

                    if (!string.IsNullOrWhiteSpace(word) && !string.IsNullOrWhiteSpace(translation))
                    {
                        // Character length guard: Skip sentences, only keep words/short phrases
                        if (word.Length > 30) continue;

                        var exists = await _context.VocabularyWords.AnyAsync(vw => vw.LessonId == lesson.Id && vw.Word == word);
                        if (!exists)
                        {
                            _context.VocabularyWords.Add(new VocabularyWord
                            {
                                Word = word,
                                Translation = translation,
                                LessonId = lesson.Id
                            });
                        }
                    }
                }
            }

            // 2. Sync from teste (interactive content)
            if (root.TryGetProperty("teste", out var tests) && tests.ValueKind == JsonValueKind.Array)
            {
                foreach (var test in tests.EnumerateArray())
                {
                    int tip = test.TryGetProperty("tip", out var tipProp) ? tipProp.GetInt32() : 0;
                    
                    if (tip == 1 || tip == 2 || tip == 4)
                    {
                        string? ro = null;
                        string? en = null;

                        if (tip == 1) // RO -> EN
                        {
                            ro = test.TryGetProperty("propozitie", out var p) ? p.GetString() : null;
                            en = test.TryGetProperty("raspunsCorrect", out var r) ? r.GetString() : (test.TryGetProperty("raspunsCorect", out var r2) ? r2.GetString() : null);
                        }
                        else if (tip == 2) // EN -> RO
                        {
                            en = test.TryGetProperty("propozitie", out var p) ? p.GetString() : null;
                            ro = test.TryGetProperty("raspunsCorrect", out var r) ? r.GetString() : (test.TryGetProperty("raspunsCorect", out var r2) ? r2.GetString() : null);
                        }
                        else if (tip == 4) // Context Info
                        {
                            ro = test.TryGetProperty("propozitie", out var p) ? p.GetString() : null;
                            en = ro;
                        }

                        if (!string.IsNullOrEmpty(ro) && !string.IsNullOrEmpty(en))
                        {
                            // Character length guard: Skip sentences
                            if (ro.Length > 30) continue;

                            var exists = await _context.VocabularyWords.AnyAsync(vw => vw.LessonId == lesson.Id && vw.Word == ro);
                            if (!exists)
                            {
                                _context.VocabularyWords.Add(new VocabularyWord
                                {
                                    Word = ro,
                                    Translation = en,
                                    LessonId = lesson.Id
                                });
                            }
                        }
                    }
                    else if (tip == 3) // Matching Pairs
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
                                    // Character length guard
                                    if (lWord.Length > 30) continue;

                                    var exists = await _context.VocabularyWords.AnyAsync(vw => vw.LessonId == lesson.Id && vw.Word == lWord);
                                    if (!exists)
                                    {
                                        _context.VocabularyWords.Add(new VocabularyWord
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
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Failed to sync vocabulary from JSON for lesson {lesson.Id}: {ex.Message}");
        }
    }

    private async Task CheckAndAwardAchievements(string userId, int lessonId, int courseId, int score)
    {
        try
        {
            // 1. Beginner's Start - First lesson completed
            var totalLessonsCompleted = await _context.UserLessonProgresses
                .Where(ulp => ulp.UserId == userId && ulp.IsCompleted)
                .CountAsync();

            if (totalLessonsCompleted >= 1)
            {
                await AwardAchievementIfNotEarned(userId, "Beginner's Start");
            }

            // 2. Perfect Score - 100% on a quiz
            if (score == 100)
            {
                await AwardAchievementIfNotEarned(userId, "Perfect Score");
            }

            // 3. Expert Learner - Completed all lessons in a course
            var totalLessonsInCourse = await _context.Lessons
                .Where(l => l.Chapter!.CourseId == courseId)
                .CountAsync();

            var completedLessonsInCourse = await _context.UserLessonProgresses
                .Where(ulp => ulp.UserId == userId && ulp.IsCompleted && 
                       ulp.Lesson!.Chapter!.CourseId == courseId)
                .CountAsync();

            if (totalLessonsInCourse > 0 && completedLessonsInCourse == totalLessonsInCourse)
            {
                await AwardAchievementIfNotEarned(userId, "Expert Learner");
            }

            // 4. Speed Learner - Complete 5 lessons in one day
            var today = DateTime.UtcNow.Date;
            var lessonsToday = await _context.UserLessonProgresses
                .Where(ulp => ulp.UserId == userId && ulp.IsCompleted && ulp.CompletionDate >= today)
                .CountAsync();

            if (lessonsToday >= 5)
            {
                await AwardAchievementIfNotEarned(userId, "Speed Learner");
            }

            // 5. Marathon Runner - Maintain a 7-day streak on ANY course
            var maxStreak = await _context.UserCourseStats
                .Where(s => s.UserId == userId)
                .MaxAsync(s => (int?)s.CurrentStreak) ?? 0;
            
            if (maxStreak >= 7)
            {
                await AwardAchievementIfNotEarned(userId, "Marathon Runner");
            }

            // 6. Social Butterfly - Reach the top 10 on the leaderboard
            var userXP = await _context.UserCourseStats
                .Where(s => s.UserId == userId && s.CourseId == courseId)
                .Select(s => s.TotalXP)
                .FirstOrDefaultAsync();

            var usersAbove = await _context.UserCourseStats
                .Where(s => s.CourseId == courseId && s.TotalXP > userXP)
                .CountAsync();

            if (usersAbove < 10) // rank = usersAbove + 1, so rank <= 10 means usersAbove <= 9
            {
                await AwardAchievementIfNotEarned(userId, "Social Butterfly");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error checking achievements: {ex.Message}");
        }
    }

    private async Task AwardAchievementIfNotEarned(string userId, string achievementTitle)
    {
        // Find the achievement by title
        var achievement = await _context.Achievements
            .FirstOrDefaultAsync(a => a.Title == achievementTitle);

        if (achievement == null) return;

        // Check if user already earned this achievement
        var alreadyEarned = await _context.UserAchievements
            .AnyAsync(ua => ua.UserId == userId && ua.AchievementId == achievement.Id);

        if (!alreadyEarned)
        {
            var userAchievement = new UserAchievement
            {
                UserId = userId,
                AchievementId = achievement.Id,
                EarnedDate = DateTime.UtcNow
            };
            _context.UserAchievements.Add(userAchievement);
            await _context.SaveChangesAsync();
        }
    }

    private async Task CheckAchievementsForAllCompletedLessons(string userId)
    {
        try
        {
            var completedLessons = await _context.UserLessonProgresses
                .Where(ulp => ulp.UserId == userId && ulp.IsCompleted)
                .Include(ulp => ulp.Lesson).ThenInclude(l => l!.Chapter)
                .ToListAsync();

            if (!completedLessons.Any()) return;

            // 1. Beginner's Start
            await AwardAchievementIfNotEarned(userId, "Beginner's Start");

            // 2. Perfect Score
            if (completedLessons.Any(ul => ul.ScoreAchieved == 100))
            {
                await AwardAchievementIfNotEarned(userId, "Perfect Score");
            }

            // 3. Expert Learner
            var courseIds = completedLessons
                .Where(ul => ul.Lesson?.Chapter != null)
                .Select(ul => ul.Lesson!.Chapter!.CourseId)
                .Distinct();

            foreach (var courseId in courseIds)
            {
                var totalInCourse = await _context.Lessons
                    .Where(l => l.Chapter!.CourseId == courseId).CountAsync();
                
                var completedInCourse = completedLessons
                    .Count(ul => ul.Lesson?.Chapter?.CourseId == courseId);

                if (totalInCourse > 0 && completedInCourse == totalInCourse)
                {
                    await AwardAchievementIfNotEarned(userId, "Expert Learner");
                    break;
                }
            }

            // 4. Speed Learner - check if any single day has 5+ completions
            var lessonsByDay = completedLessons
                .GroupBy(ul => ul.CompletionDate.Date)
                .Any(g => g.Count() >= 5);

            if (lessonsByDay)
            {
                await AwardAchievementIfNotEarned(userId, "Speed Learner");
            }

            // 5. Marathon Runner
            var maxStreak = await _context.UserCourseStats
                .Where(s => s.UserId == userId)
                .MaxAsync(s => (int?)s.CurrentStreak) ?? 0;

            if (maxStreak >= 7)
            {
                await AwardAchievementIfNotEarned(userId, "Marathon Runner");
            }

            // 6. Social Butterfly - check best rank across all courses
            var allUserStats = await _context.UserCourseStats
                .Where(s => s.UserId == userId)
                .ToListAsync();

            foreach (var stat in allUserStats)
            {
                var usersAbove = await _context.UserCourseStats
                    .Where(s => s.CourseId == stat.CourseId && s.TotalXP > stat.TotalXP)
                    .CountAsync();

                if (usersAbove < 10)
                {
                    await AwardAchievementIfNotEarned(userId, "Social Butterfly");
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error checking achievements for all lessons: {ex.Message}");
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
