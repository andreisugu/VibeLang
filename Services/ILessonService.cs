using System.Collections.Generic;
using System.Threading.Tasks;
using VibeLang.Models;

namespace VibeLang.Services;

public interface ILessonService
{
    Task<IEnumerable<Lesson>> GetAllLessonsAsync();
    Task<Lesson?> GetLessonByIdAsync(int id);
    Task AddLessonAsync(Lesson lesson);
    Task UpdateLessonAsync(Lesson lesson);
    Task DeleteLessonAsync(int id);
    Task<bool> LessonExistsAsync(int id);
}
