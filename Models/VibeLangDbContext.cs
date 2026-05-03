using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace VibeLang.Models;

public class VibeLangDbContext : IdentityDbContext<ApplicationUser>
{
    public VibeLangDbContext(DbContextOptions<VibeLangDbContext> options)
        : base(options)
    {
    }

    // Content
    public DbSet<Course> Courses { get; set; }
    public DbSet<Chapter> Chapters { get; set; }
    public DbSet<Lesson> Lessons { get; set; }
    public DbSet<VocabularyWord> VocabularyWords { get; set; }

    // Progress
    public DbSet<UserVocabulary> UserVocabularies { get; set; }
    public DbSet<UserLessonProgress> UserLessonProgresses { get; set; }
    public DbSet<UserCourseStats> UserCourseStats { get; set; }
    public DbSet<Achievement> Achievements { get; set; }
    public DbSet<UserAchievement> UserAchievements { get; set; }

    // Assessment
    public DbSet<Quiz> Quizzes { get; set; }
    public DbSet<QuizQuestion> QuizQuestions { get; set; }
    public DbSet<QuizOption> QuizOptions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Course>().ToTable("Courses");
        modelBuilder.Entity<Chapter>().ToTable("Chapters");
        modelBuilder.Entity<Lesson>().ToTable("Lessons");
        modelBuilder.Entity<VocabularyWord>().ToTable("VocabularyWords");
        modelBuilder.Entity<UserVocabulary>().ToTable("UserVocabularies");
        modelBuilder.Entity<UserLessonProgress>().ToTable("UserLessonProgresses");
        modelBuilder.Entity<UserCourseStats>().ToTable("UserCourseStats");
        modelBuilder.Entity<Achievement>().ToTable("Achievements");
        modelBuilder.Entity<UserAchievement>().ToTable("UserAchievements");
        modelBuilder.Entity<Quiz>().ToTable("Quizzes");
        modelBuilder.Entity<QuizQuestion>().ToTable("QuizQuestions");
        modelBuilder.Entity<QuizOption>().ToTable("QuizOptions");
    }
}
