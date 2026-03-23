using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VibeLang.Models;

namespace VibeLang.Controllers;

public class LessonsController : Controller
{
    private readonly VibeLangDbContext _context;

    public LessonsController(VibeLangDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var lessons = await _context.Lessons
            .Include(l => l.Chapter)
                .ThenInclude(c => c.Course)
            .OrderBy(l => l.Chapter.CourseId)
            .ThenBy(l => l.Chapter.Order)
            .ThenBy(l => l.Order)
            .ToListAsync();
        return View(lessons);
    }

    public IActionResult Create()
    {
        var chapters = _context.Chapters
            .Include(c => c.Course)
            .Select(c => new { 
                Id = c.Id, 
                Display = $"{c.Course.Title} - {c.Title}" 
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
            _context.Add(lesson);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        var chapters = _context.Chapters
            .Include(c => c.Course)
            .Select(c => new { 
                Id = c.Id, 
                Display = $"{c.Course.Title} - {c.Title}" 
            })
            .ToList();
        ViewData["ChapterId"] = new SelectList(chapters, "Id", "Display", lesson.ChapterId);
        return View(lesson);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var lesson = await _context.Lessons.FindAsync(id);
        if (lesson == null) return NotFound();
        
        var chapters = _context.Chapters
            .Include(c => c.Course)
            .Select(c => new { 
                Id = c.Id, 
                Display = $"{c.Course.Title} - {c.Title}" 
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
            _context.Update(lesson);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        var chapters = _context.Chapters
            .Include(c => c.Course)
            .Select(c => new { 
                Id = c.Id, 
                Display = $"{c.Course.Title} - {c.Title}" 
            })
            .ToList();
        ViewData["ChapterId"] = new SelectList(chapters, "Id", "Display", lesson.ChapterId);
        return View(lesson);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var lesson = await _context.Lessons
            .Include(l => l.Chapter)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (lesson == null) return NotFound();
        return View(lesson);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var lesson = await _context.Lessons.FindAsync(id);
        if (lesson != null) _context.Lessons.Remove(lesson);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
