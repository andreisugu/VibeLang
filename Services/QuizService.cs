using VibeLang.Models;
using VibeLang.Repositories;

namespace VibeLang.Services;

public class QuizService : IQuizService
{
    private readonly IQuizRepository _quizRepository;

    public QuizService(IQuizRepository quizRepository)
    {
        _quizRepository = quizRepository;
    }

    public async Task<Quiz?> GetQuizWithQuestionsAsync(int quizId)
    {
        return await _quizRepository.GetQuizWithQuestionsAndOptionsAsync(quizId);
    }

    public async Task<Quiz?> GetQuizByLessonIdAsync(int lessonId)
    {
        return await _quizRepository.GetQuizByLessonIdAsync(lessonId);
    }

    public async Task<(int correct, int total, int score)> CalculateQuizScoreAsync(int quizId, Dictionary<string, string> answers)
    {
        var quiz = await _quizRepository.GetQuizWithQuestionsAndOptionsAsync(quizId);
        if (quiz == null) throw new ArgumentException("Quiz not found");

        int correct = 0;
        int total = quiz.Questions.Count;

        foreach (var question in quiz.Questions)
        {
            if (answers.TryGetValue(question.Id.ToString(), out var selectedOptionIdStr) &&
                int.TryParse(selectedOptionIdStr, out var selectedOptionId))
            {
                var selectedOption = question.Options.FirstOrDefault(o => o.Id == selectedOptionId);
                if (selectedOption?.IsCorrect ?? false)
                {
                    correct++;
                }
            }
        }

        int score = total > 0 ? (correct * 100) / total : 0;
        return (correct, total, score);
    }
}
