using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VibeLang.Models;

namespace VibeLang.Repositories;

public class LessonRepository : Repository<Lesson>, ILessonRepository
{
    public LessonRepository(VibeLangDbContext context) : base(context)
    {
    }

    public override async Task<IEnumerable<Lesson>> GetAllAsync()
    {
        return await _context.Lessons
            .Include(l => l.Chapter)
                .ThenInclude(c => c.Course)
            .OrderBy(l => l.Chapter.CourseId)
            .ThenBy(l => l.Chapter.Order)
            .ThenBy(l => l.Order)
            .ToListAsync();
    }

    public override async Task<Lesson?> GetByIdAsync(int id)
    {
        return await _context.Lessons
            .Include(l => l.Chapter)
            .FirstOrDefaultAsync(m => m.Id == id);
    }
}
