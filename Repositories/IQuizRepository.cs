using VibeLang.Models;

namespace VibeLang.Repositories;

public interface IQuizRepository : IRepository<Quiz>
{
    Task<Quiz?> GetQuizWithQuestionsAndOptionsAsync(int quizId);
    Task<Quiz?> GetQuizByLessonIdAsync(int lessonId);
}
