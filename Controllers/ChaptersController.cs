using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;
using VibeLang.Models;
using VibeLang.Services;

namespace VibeLang.Controllers;

public class ChaptersController : Controller
{
    private readonly IChapterService _chapterService;
    private readonly ICourseService _courseService;

    public ChaptersController(IChapterService chapterService, ICourseService courseService)
    {
        _chapterService = chapterService;
        _courseService = courseService;
    }

    public async Task<IActionResult> Index()
    {
        var chapters = await _chapterService.GetAllChaptersAsync();
        return View(chapters);
    }

    public async Task<IActionResult> Create()
    {
        var courses = await _courseService.GetAllCoursesAsync();
        ViewData["CourseId"] = new SelectList(courses, "Id", "Title");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Title,Order,CourseId")] Chapter chapter)
    {
        if (ModelState.IsValid)
        {
            await _chapterService.AddChapterAsync(chapter);
            return RedirectToAction(nameof(Index));
        }
        var courses = await _courseService.GetAllCoursesAsync();
        ViewData["CourseId"] = new SelectList(courses, "Id", "Title", chapter.CourseId);
        return View(chapter);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var chapter = await _chapterService.GetChapterByIdAsync(id.Value);
        if (chapter == null) return NotFound();
        var courses = await _courseService.GetAllCoursesAsync();
        ViewData["CourseId"] = new SelectList(courses, "Id", "Title", chapter.CourseId);
        return View(chapter);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Order,CourseId")] Chapter chapter)
    {
        if (id != chapter.Id) return NotFound();
        if (ModelState.IsValid)
        {
            await _chapterService.UpdateChapterAsync(chapter);
            return RedirectToAction(nameof(Index));
        }
        var courses = await _courseService.GetAllCoursesAsync();
        ViewData["CourseId"] = new SelectList(courses, "Id", "Title", chapter.CourseId);
        return View(chapter);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var chapter = await _chapterService.GetChapterByIdAsync(id.Value);
        if (chapter == null) return NotFound();
        return View(chapter);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _chapterService.DeleteChapterAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
