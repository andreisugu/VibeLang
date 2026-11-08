using Microsoft.EntityFrameworkCore;
using VibeLang.Models;

namespace VibeLang.Browser.Data;

/// <summary>
/// The content half of the real VibeLang model, running on SQLite in the
/// browser. Identity and per-user progress are intentionally absent: the
/// demo has no server and no real accounts.
/// </summary>
public class BrowserDbContext : DbContext
{
    public BrowserDbContext(DbContextOptions<BrowserDbContext> options) : base(options) { }

    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Chapter> Chapters => Set<Chapter>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<VocabularyWord> VocabularyWords => Set<VocabularyWord>();
    public DbSet<Achievement> Achievements => Set<Achievement>();
    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<QuizQuestion> QuizQuestions => Set<QuizQuestion>();
    public DbSet<QuizOption> QuizOptions => Set<QuizOption>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Course>().ToTable("Courses");
        modelBuilder.Entity<Chapter>().ToTable("Chapters");
        modelBuilder.Entity<Lesson>().ToTable("Lessons");
        modelBuilder.Entity<Achievement>().ToTable("Achievements");
        modelBuilder.Entity<Quiz>().ToTable("Quizzes");
        modelBuilder.Entity<QuizQuestion>().ToTable("QuizQuestions");
        modelBuilder.Entity<QuizOption>().ToTable("QuizOptions");

        modelBuilder.Entity<VocabularyWord>().ToTable("VocabularyWords")
            .HasIndex(v => new { v.LessonId, v.Word })
            .IsUnique()
            .HasDatabaseName("IX_VocabularyWords_LessonId_Word");
    }
}
