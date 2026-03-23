using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VibeLang.Models;

namespace VibeLang.Controllers;

public class ChaptersController : Controller
{
    private readonly VibeLangDbContext _context;

    public ChaptersController(VibeLangDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var chapters = await _context.Chapters.Include(c => c.Course).OrderBy(c => c.CourseId).ThenBy(c => c.Order).ToListAsync();
        return View(chapters);
    }

    public IActionResult Create()
    {
        ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Title");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Title,Order,CourseId")] Chapter chapter)
    {
        if (ModelState.IsValid)
        {
            _context.Add(chapter);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Title", chapter.CourseId);
        return View(chapter);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var chapter = await _context.Chapters.FindAsync(id);
        if (chapter == null) return NotFound();
        ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Title", chapter.CourseId);
        return View(chapter);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Order,CourseId")] Chapter chapter)
    {
        if (id != chapter.Id) return NotFound();
        if (ModelState.IsValid)
        {
            _context.Update(chapter);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Title", chapter.CourseId);
        return View(chapter);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var chapter = await _context.Chapters.Include(c => c.Course).FirstOrDefaultAsync(m => m.Id == id);
        if (chapter == null) return NotFound();
        return View(chapter);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var chapter = await _context.Chapters.FindAsync(id);
        if (chapter != null) _context.Chapters.Remove(chapter);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
