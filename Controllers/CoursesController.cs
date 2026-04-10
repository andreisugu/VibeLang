using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using VibeLang.Models;
using VibeLang.Services;

namespace VibeLang.Controllers;

public class CoursesController : Controller
{
    private readonly ICourseService _courseService;

    public CoursesController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    // GET: Courses
    public async Task<IActionResult> Index()
    {
        return View(await _courseService.GetAllCoursesAsync());
    }

    // GET: Courses/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var course = await _courseService.GetCourseByIdAsync(id.Value);
        if (course == null) return NotFound();

        return View(course);
    }

    // GET: Courses/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Courses/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Title,Description,IsoCode")] Course course)
    {
        if (ModelState.IsValid)
        {
            await _courseService.AddCourseAsync(course);
            return RedirectToAction(nameof(Index));
        }
        return View(course);
    }

    // GET: Courses/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var course = await _courseService.GetCourseByIdAsync(id.Value);
        if (course == null) return NotFound();
        return View(course);
    }

    // POST: Courses/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,IsoCode")] Course course)
    {
        if (id != course.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                await _courseService.UpdateCourseAsync(course);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await CourseExists(course.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(course);
    }

    // GET: Courses/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var course = await _courseService.GetCourseByIdAsync(id.Value);
        if (course == null) return NotFound();

        return View(course);
    }

    // POST: Courses/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var course = await _courseService.GetCourseByIdAsync(id);
        if (course != null)
        {
            await _courseService.DeleteCourseAsync(id);
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> CourseExists(int id)
    {
        return await _courseService.CourseExistsAsync(id);
    }
}
