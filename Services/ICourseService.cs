using System.Collections.Generic;
using System.Threading.Tasks;
using VibeLang.Models;

namespace VibeLang.Services;

public interface ICourseService
{
    Task<IEnumerable<Course>> GetAllCoursesAsync();
    Task<Course?> GetCourseByIdAsync(int id);
    Task AddCourseAsync(Course course);
    Task UpdateCourseAsync(Course course);
    Task DeleteCourseAsync(int id);
    Task<bool> CourseExistsAsync(int id);
}
