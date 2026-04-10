using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using System.Threading.Tasks;
using VibeLang.Models;
using VibeLang.Services;

namespace VibeLang.Controllers;

public class LessonsController : Controller
{
    private readonly ILessonService _lessonService;
    private readonly IChapterService _chapterService;

    public LessonsController(ILessonService lessonService, IChapterService chapterService)
    {
        _lessonService = lessonService;
        _chapterService = chapterService;
    }

    public async Task<IActionResult> Index()
    {
        var lessons = await _lessonService.GetAllLessonsAsync();
        return View(lessons);
    }

    public async Task<IActionResult> Create()
    {
        var allChapters = await _chapterService.GetAllChaptersAsync();
        var chapters = allChapters
            .Select(c => new { 
                Id = c.Id, 
                Display = $"{c?.Course?.Title} - {c?.Title}" 
            })
            .ToList();
        ViewData["ChapterId"] = new SelectList(chapters, "Id", "Display");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Title,Difficulty,Order,ChapterId,ContentJson")] Lesson lesson)
    {
        if (ModelState.IsValid)
        {
            await _lessonService.AddLessonAsync(lesson);
            return RedirectToAction(nameof(Index));
        }
        
        var allChapters = await _chapterService.GetAllChaptersAsync();
        var chapters = allChapters
            .Select(c => new { 
                Id = c.Id, 
                Display = $"{c?.Course?.Title} - {c?.Title}" 
            })
            .ToList();
        ViewData["ChapterId"] = new SelectList(chapters, "Id", "Display", lesson.ChapterId);
        return View(lesson);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var lesson = await _lessonService.GetLessonByIdAsync(id.Value);
        if (lesson == null) return NotFound();
        
        var allChapters = await _chapterService.GetAllChaptersAsync();
        var chapters = allChapters
            .Select(c => new { 
                Id = c.Id, 
                Display = $"{c?.Course?.Title} - {c?.Title}" 
            })
            .ToList();
        ViewData["ChapterId"] = new SelectList(chapters, "Id", "Display", lesson.ChapterId);
        return View(lesson);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Difficulty,Order,ChapterId,ContentJson")] Lesson lesson)
    {
        if (id != lesson.Id) return NotFound();
        if (ModelState.IsValid)
        {
            await _lessonService.UpdateLessonAsync(lesson);
            return RedirectToAction(nameof(Index));
        }
        
        var allChapters = await _chapterService.GetAllChaptersAsync();
        var chapters = allChapters
            .Select(c => new { 
                Id = c.Id, 
                Display = $"{c?.Course?.Title} - {c?.Title}" 
            })
            .ToList();
        ViewData["ChapterId"] = new SelectList(chapters, "Id", "Display", lesson.ChapterId);
        return View(lesson);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var lesson = await _lessonService.GetLessonByIdAsync(id.Value);
        if (lesson == null) return NotFound();
        return View(lesson);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _lessonService.DeleteLessonAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
