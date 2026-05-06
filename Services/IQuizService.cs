using VibeLang.Models;

namespace VibeLang.Services;

public interface IQuizService
{
    Task<Quiz?> GetQuizWithQuestionsAsync(int quizId);
    Task<Quiz?> GetQuizByLessonIdAsync(int lessonId);
    Task<(int correct, int total, int score)> CalculateQuizScoreAsync(int quizId, Dictionary<string, string> answers);
}
