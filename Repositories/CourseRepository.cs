using VibeLang.Models;

namespace VibeLang.Repositories;

public class CourseRepository : Repository<Course>, ICourseRepository
{
    public CourseRepository(VibeLangDbContext context) : base(context)
    {
    }
}
