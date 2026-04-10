using System.Collections.Generic;
using System.Threading.Tasks;
using VibeLang.Models;
using VibeLang.Repositories;

namespace VibeLang.Services;

public class LessonService : ILessonService
{
    private readonly ILessonRepository _repository;

    public LessonService(ILessonRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Lesson>> GetAllLessonsAsync() => await _repository.GetAllAsync();

    public async Task<Lesson?> GetLessonByIdAsync(int id) => await _repository.GetByIdAsync(id);

    public async Task AddLessonAsync(Lesson lesson) => await _repository.AddAsync(lesson);

    public async Task UpdateLessonAsync(Lesson lesson) => await _repository.UpdateAsync(lesson);

    public async Task DeleteLessonAsync(int id) => await _repository.DeleteAsync(id);

    public async Task<bool> LessonExistsAsync(int id) => await _repository.ExistsAsync(id);
}
