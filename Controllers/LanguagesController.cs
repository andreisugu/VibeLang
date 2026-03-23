using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VibeLang.Models;

namespace VibeLang.Controllers;

public class LanguagesController : Controller
{
    private readonly VibeLangDbContext _context;

    public LanguagesController(VibeLangDbContext context)
    {
        _context = context;
    }

    // GET: Languages
    public async Task<IActionResult> Index()
    {
        return View(await _context.Languages.ToListAsync());
    }

    // GET: Languages/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var language = await _context.Languages
            .FirstOrDefaultAsync(m => m.Id == id);
        if (language == null) return NotFound();

        return View(language);
    }

    // GET: Languages/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Languages/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Name,IsoCode")] Language language)
    {
        if (ModelState.IsValid)
        {
            _context.Add(language);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(language);
    }

    // GET: Languages/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var language = await _context.Languages.FindAsync(id);
        if (language == null) return NotFound();
        return View(language);
    }

    // POST: Languages/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,IsoCode")] Language language)
    {
        if (id != language.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(language);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LanguageExists(language.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(language);
    }

    // GET: Languages/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var language = await _context.Languages
            .FirstOrDefaultAsync(m => m.Id == id);
        if (language == null) return NotFound();

        return View(language);
    }

    // POST: Languages/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var language = await _context.Languages.FindAsync(id);
        if (language != null)
        {
            _context.Languages.Remove(language);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool LanguageExists(int id)
    {
        return _context.Languages.Any(e => e.Id == id);
    }
}
