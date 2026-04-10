using System.Collections.Generic;
using System.Threading.Tasks;
using VibeLang.Models;
using VibeLang.Repositories;

namespace VibeLang.Services;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _repository;

    public CourseService(ICourseRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Course>> GetAllCoursesAsync() => await _repository.GetAllAsync();

    public async Task<Course?> GetCourseByIdAsync(int id) => await _repository.GetByIdAsync(id);

    public async Task AddCourseAsync(Course course) => await _repository.AddAsync(course);

    public async Task UpdateCourseAsync(Course course) => await _repository.UpdateAsync(course);

    public async Task DeleteCourseAsync(int id) => await _repository.DeleteAsync(id);

    public async Task<bool> CourseExistsAsync(int id) => await _repository.ExistsAsync(id);
}
