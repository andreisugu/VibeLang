using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VibeLang.Models;

/* --- CONTENT TABLES --- */

public class Language
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string Name { get; set; } = string.Empty;
    [Required]
    public string IsoCode { get; set; } = string.Empty;
    public ICollection<Course> Courses { get; set; } = new List<Course>();
}

public class Course
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int LanguageId { get; set; }
    [ForeignKey("LanguageId")]
    public Language? Language { get; set; }
    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}

public class Lesson
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string Title { get; set; } = string.Empty;
    public int Order { get; set; }
    public int CourseId { get; set; }
    [ForeignKey("CourseId")]
    public Course? Course { get; set; }
    public ICollection<VocabularyWord> VocabularyWords { get; set; } = new List<VocabularyWord>();
}

public class VocabularyWord
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string Word { get; set; } = string.Empty;
    [Required]
    public string Translation { get; set; } = string.Empty;
    public string? ExampleSentence { get; set; }
    public int LessonId { get; set; }
    [ForeignKey("LessonId")]
    public Lesson? Lesson { get; set; }
}

/* --- PROGRESS & TRACKING TABLES (The "Real App" Logic) --- */

public class UserVocabulary
{
    [Key]
    public int Id { get; set; }
    public int UserId { get; set; } // Reference to User table (managed externally)
    public int WordId { get; set; }
    [ForeignKey("WordId")]
    public VocabularyWord? Word { get; set; }
    public string Status { get; set; } = "New"; // "Learned", "New", "Mastered"
    public DateTime LastReviewed { get; set; } = DateTime.UtcNow;
}

public class UserLessonProgress
{
    [Key]
    public int Id { get; set; }
    public int UserId { get; set; }
    public int LessonId { get; set; }
    [ForeignKey("LessonId")]
    public Lesson? Lesson { get; set; }
    public bool IsCompleted { get; set; }
    public int ScoreAchieved { get; set; }
    public DateTime CompletionDate { get; set; }
}

public class UserCourseStats
{
    [Key]
    public int Id { get; set; }
    public int UserId { get; set; }
    public int CourseId { get; set; }
    [ForeignKey("CourseId")]
    public Course? Course { get; set; }
    public int TotalXP { get; set; }
    public int CurrentStreak { get; set; }
}

public class Achievement
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? IconPath { get; set; }
}

/* --- ASSESSMENT TABLES --- */

public class Quiz
{
    [Key]
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int LessonId { get; set; }
    [ForeignKey("LessonId")]
    public Lesson? Lesson { get; set; }
    public ICollection<QuizQuestion> Questions { get; set; } = new List<QuizQuestion>();
}

public class QuizQuestion
{
    [Key]
    public int Id { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public int QuizId { get; set; }
    [ForeignKey("QuizId")]
    public Quiz? Quiz { get; set; }
    public ICollection<QuizOption> Options { get; set; } = new List<QuizOption>();
}

public class QuizOption
{
    [Key]
    public int Id { get; set; }
    public string OptionText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int QuestionId { get; set; }
    [ForeignKey("QuestionId")]
    public QuizQuestion? Question { get; set; }
}
