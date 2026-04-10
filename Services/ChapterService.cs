using System.Collections.Generic;
using System.Threading.Tasks;
using VibeLang.Models;
using VibeLang.Repositories;

namespace VibeLang.Services;

public class ChapterService : IChapterService
{
    private readonly IChapterRepository _repository;

    public ChapterService(IChapterRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Chapter>> GetAllChaptersAsync() => await _repository.GetAllAsync();

    public async Task<Chapter?> GetChapterByIdAsync(int id) => await _repository.GetByIdAsync(id);

    public async Task AddChapterAsync(Chapter chapter) => await _repository.AddAsync(chapter);

    public async Task UpdateChapterAsync(Chapter chapter) => await _repository.UpdateAsync(chapter);

    public async Task DeleteChapterAsync(int id) => await _repository.DeleteAsync(id);

    public async Task<bool> ChapterExistsAsync(int id) => await _repository.ExistsAsync(id);
}
