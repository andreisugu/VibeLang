using System.Collections.Generic;
using System.Threading.Tasks;
using VibeLang.Models;

namespace VibeLang.Services;

public interface IChapterService
{
    Task<IEnumerable<Chapter>> GetAllChaptersAsync();
    Task<Chapter?> GetChapterByIdAsync(int id);
    Task AddChapterAsync(Chapter chapter);
    Task UpdateChapterAsync(Chapter chapter);
    Task DeleteChapterAsync(int id);
    Task<bool> ChapterExistsAsync(int id);
}
