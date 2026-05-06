using Microsoft.EntityFrameworkCore;
using VibeLang.Models;

namespace VibeLang.Repositories;

public class QuizRepository : Repository<Quiz>, IQuizRepository
{
    public QuizRepository(VibeLangDbContext context) : base(context)
    {
    }

    public async Task<Quiz?> GetQuizWithQuestionsAndOptionsAsync(int quizId)
    {
        return await _context.Quizzes
            .Include(q => q.Questions)
            .ThenInclude(q => q.Options)
            .Include(q => q.Lesson)
            .ThenInclude(l => l.Chapter)
            .FirstOrDefaultAsync(q => q.Id == quizId);
    }

    public async Task<Quiz?> GetQuizByLessonIdAsync(int lessonId)
    {
        return await _context.Quizzes
            .Include(q => q.Questions)
            .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(q => q.LessonId == lessonId);
    }
}
