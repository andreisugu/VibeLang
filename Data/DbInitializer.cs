using Microsoft.EntityFrameworkCore;
using VibeLang.Models;
using System.Text.Json;

namespace VibeLang.Data;

public static class DbInitializer
{
    public static async Task Initialize(VibeLangDbContext context)
    {
        context.Database.EnsureCreated();

        // 1. Check for existing data
        if (await context.Courses.AnyAsync())
        {
            return; // DB has been seeded
        }

        // 2. Add Course
        var englishBasics = new Course 
        { 
            Title = "English for Beginners", 
            Description = "Learn the essential phrases and grammar.",
            IsoCode = "en"
        };
        context.Courses.Add(englishBasics);
        await context.SaveChangesAsync();

        // 3. Add Chapter
        var chapter1 = new Chapter 
        { 
            Title = "Chapter 1: Getting Started", 
            Order = 1, 
            CourseId = englishBasics.Id 
        };
        context.Chapters.Add(chapter1);
        await context.SaveChangesAsync();

        // 4. Add Lesson
        var lesson1 = new Lesson
        {
            Title = "Lecția 1: Fraze Comune",
            Difficulty = "Ușor",
            Order = 1,
            ChapterId = chapter1.Id
        };
        context.Lessons.Add(lesson1);
        await context.SaveChangesAsync();

        // 5. Add Vocabulary Words
        var words = new List<string> { "bună dimineața", "bună seara", "noapte bună", "cum ești?", "sunt bine", "mulțumesc", "cu plăcere", "scuze", "da", "nu" };
        var translations = new List<string> { "good morning", "good evening", "good night", "how are you?", "I am fine", "thank you", "you are welcome", "sorry", "yes", "no" };

        for (int i = 0; i < words.Count; i++)
        {
            context.VocabularyWords.Add(new VocabularyWord
            {
                Word = words[i],
                Translation = translations[i],
                LessonId = lesson1.Id
            });
        }

        // 6. Add Quiz
        var quiz1 = new Quiz { Title = "Test: Fraze Comune", LessonId = lesson1.Id };
        context.Quizzes.Add(quiz1);
        await context.SaveChangesAsync();

        // 7. Add Quiz Questions
        
        // Tip 1: RO -> EN
        context.QuizQuestions.Add(new QuizQuestion
        {
            QuizId = quiz1.Id,
            Tip = 1,
            QuestionText = "Bună dimineața, cum te simți?",
            CorrectAnswer = "Good morning, how do you feel?"
        });

        // Tip 2: EN -> RO
        context.QuizQuestions.Add(new QuizQuestion
        {
            QuizId = quiz1.Id,
            Tip = 2,
            QuestionText = "How are you today?",
            CorrectAnswer = "Cum ești azi?"
        });

        // Tip 3: Matching
        var matchingData = new
        {
            leftWords = new[] { "masă", "scaun", "carte", "apă", "pâine" },
            rightWords = new[] { "table", "chair", "book", "water", "bread" }
        };
        context.QuizQuestions.Add(new QuizQuestion
        {
            QuizId = quiz1.Id,
            Tip = 3,
            QuestionText = "Match the pairs",
            MatchingDataJson = JsonSerializer.Serialize(matchingData)
        });

        // Tip 4: Context/Tip
        context.QuizQuestions.Add(new QuizQuestion
        {
            QuizId = quiz1.Id,
            Tip = 4,
            QuestionText = "Un exemplu de folosire a cuvântului 'hello': 'Hello, how are you?' (Bună, cum ești?)"
        });

        await context.SaveChangesAsync();
    }
}
