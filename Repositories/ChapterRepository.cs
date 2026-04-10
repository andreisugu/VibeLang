using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VibeLang.Models;

namespace VibeLang.Repositories;

public class ChapterRepository : Repository<Chapter>, IChapterRepository
{
    public ChapterRepository(VibeLangDbContext context) : base(context)
    {
    }

    public override async Task<IEnumerable<Chapter>> GetAllAsync()
    {
        return await _context.Chapters
            .Include(c => c.Course)
            .OrderBy(c => c.CourseId)
            .ThenBy(c => c.Order)
            .ToListAsync();
    }

    public override async Task<Chapter?> GetByIdAsync(int id)
    {
        return await _context.Chapters
            .Include(c => c.Course)
            .FirstOrDefaultAsync(m => m.Id == id);
    }
}
