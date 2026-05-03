# VibeLang - Code Examples for Professor

**Demonstrating Grading Criteria**

---

## 1️⃣ REPOSITORY PATTERN (3-Layer Architecture)

### Layer 1: Generic Data Access - `Repositories/Repository.cs`

**What to show:**
- Generic `IRepository<T>` interface (reusable for all entities)
- Base `Repository<T>` class (CRUD operations)
- Applied to all models: Course, Chapter, Lesson, Achievement, etc.

```csharp
// Generic interface - reusable for ALL entities
public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task SaveChangesAsync();
}

// Generic base class - implements interface
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly VibeLangDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(VibeLangDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<IEnumerable<T>> GetAllAsync() 
        => await _dbSet.ToListAsync();

    public async Task<T?> GetByIdAsync(int id) 
        => await _dbSet.FindAsync(id);

    public async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await SaveChangesAsync();
    }

    public async Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        await SaveChangesAsync();
    }

    public async Task SaveChangesAsync() 
        => await _context.SaveChangesAsync();
}
```

**Talking Point:**
"Instead of writing CRUD logic 10+ times for each entity, we write it once in Repository<T>. This follows DRY (Don't Repeat Yourself) principle."

---

### Layer 2: Specialized Data Access - `Repositories/CourseRepository.cs`

**What to show:**
- Extends `Repository<T>` with specialized queries
- Uses `.Include()` for eager loading
- Custom queries specific to business logic

```csharp
public class CourseRepository : Repository<Course>, ICourseRepository
{
    public CourseRepository(VibeLangDbContext context) : base(context) { }

    // Specialized query with related data
    public async Task<Course?> GetCourseWithChaptersAsync(int courseId)
    {
        return await _context.Courses
            .Include(c => c.Chapters)
                .ThenInclude(ch => ch.Lessons)
            .FirstOrDefaultAsync(c => c.Id == courseId);
    }

    // Find by property instead of ID (better UX)
    public async Task<Course?> GetCourseByIsoCodeAsync(string isoCode)
    {
        return await _context.Courses
            .FirstOrDefaultAsync(c => c.IsoCode == isoCode);
    }

    // Get statistics for dashboard
    public async Task<(int totalLessons, int completedLessons)> GetCourseProgressAsync(int courseId, string userId)
    {
        var totalLessons = await _context.Lessons
            .Where(l => l.Chapter!.CourseId == courseId)
            .CountAsync();

        var completedLessons = await _context.UserLessonProgresses
            .Where(ulp => ulp.UserId == userId && ulp.Lesson.Chapter!.CourseId == courseId && ulp.IsCompleted)
            .CountAsync();

        return (totalLessons, completedLessons);
    }
}

public interface ICourseRepository : IRepository<Course>
{
    Task<Course?> GetCourseWithChaptersAsync(int courseId);
    Task<Course?> GetCourseByIsoCodeAsync(string isoCode);
    Task<(int, int)> GetCourseProgressAsync(int courseId, string userId);
}
```

**Talking Point:**
"Specialized repositories add complex queries without cluttering the base class. This keeps the generic Repository<T> clean and reusable."

---

### Layer 3: Business Logic - `Services/CourseService.cs`

**What to show:**
- Service layer orchestrates repository operations
- Dependencies injected through constructor
- Business logic separated from data access

```csharp
public class CourseService : ICourseService
{
    private readonly ICourseRepository _courseRepository;
    private readonly IChapterRepository _chapterRepository;
    private readonly ILogger<CourseService> _logger;

    public CourseService(
        ICourseRepository courseRepository,
        IChapterRepository chapterRepository,
        ILogger<CourseService> logger)
    {
        _courseRepository = courseRepository;
        _chapterRepository = chapterRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<Course>> GetAllCoursesAsync()
    {
        try
        {
            return await _courseRepository.GetAllAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching courses");
            throw;
        }
    }

    public async Task<Course?> GetCourseDetailsAsync(int courseId)
    {
        return await _courseRepository.GetCourseWithChaptersAsync(courseId);
    }

    public async Task<Course> CreateCourseAsync(Course course)
    {
        // Business logic can be added here
        // E.g., validate course doesn't already exist
        // E.g., assign default values
        // E.g., send notifications

        _logger.LogInformation($"Creating course: {course.Name}");
        return await _courseRepository.AddAsync(course);
    }

    public async Task UpdateCourseAsync(Course course)
    {
        await _courseRepository.UpdateAsync(course);
    }

    public async Task DeleteCourseAsync(int courseId)
    {
        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course != null)
        {
            await _courseRepository.DeleteAsync(course);
        }
    }
}

public interface ICourseService
{
    Task<IEnumerable<Course>> GetAllCoursesAsync();
    Task<Course?> GetCourseDetailsAsync(int courseId);
    Task<Course> CreateCourseAsync(Course course);
    Task UpdateCourseAsync(Course course);
    Task DeleteCourseAsync(int courseId);
}
```

**Talking Point:**
"Services contain business logic. Controllers never talk to repositories directly - only to services. This makes testing easier and business logic reusable."

---

### Layer 4: Presentation - `Controllers/CoursesController.cs` + `Program.cs` DI

**What to show:**
- Controller depends on service (not repository)
- Dependency injection in `Program.cs`
- Clean separation of concerns

```csharp
// Program.cs - Dependency Injection Setup
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IChapterRepository, ChapterRepository>();
builder.Services.AddScoped<ILessonRepository, LessonRepository>();

builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IChapterService, ChapterService>();
builder.Services.AddScoped<ILessonService, LessonService>();

// Controllers/CoursesController.cs
public class CoursesController : Controller
{
    private readonly ICourseService _courseService;

    // Dependency injection - service provided by ASP.NET Core
    public CoursesController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    public async Task<IActionResult> Index()
    {
        var courses = await _courseService.GetAllCoursesAsync();
        return View(courses);
    }

    public async Task<IActionResult> Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Course course)
    {
        if (!ModelState.IsValid)
            return View(course);

        await _courseService.CreateCourseAsync(course);
        return RedirectToAction(nameof(Index));
    }
}
```

**Talking Point:**
"The flow is: Controller → Service → Repository → Database. Each layer has one responsibility, making the code testable and maintainable."

---

## 2️⃣ INPUT VALIDATIONS

### Data Annotations in Models - `Models/Course.cs`

**What to show:**
- `[Required]` - Mandatory fields
- `[StringLength]` - Text length limits
- `[Range]` - Numeric ranges
- `[RegularExpression]` - Pattern matching

```csharp
public class Course
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Course name is required")]
    [StringLength(100, MinimumLength = 3,
        ErrorMessage = "Course name must be between 3 and 100 characters")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500,
        ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    // Validates 2-letter ISO 639-1 language codes
    [Required(ErrorMessage = "Language code is required")]
    [RegularExpression(@"^[a-z]{2}$",
        ErrorMessage = "ISO 639-1 code must be 2 lowercase letters (e.g., 'en', 'fr', 'de', 'es')")]
    public string IsoCode { get; set; } = string.Empty;

    [Range(1, 100,
        ErrorMessage = "Difficulty must be between 1 and 100")]
    public int DifficultyLevel { get; set; }

    public ICollection<Chapter> Chapters { get; set; } = [];
}

public class Lesson
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Lesson title is required")]
    [StringLength(150, MinimumLength = 5,
        ErrorMessage = "Title must be between 5 and 150 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Lesson content is required")]
    [StringLength(5000, MinimumLength = 50,
        ErrorMessage = "Content must be between 50 and 5000 characters")]
    public string Content { get; set; } = string.Empty;

    [Range(1, 1000,
        ErrorMessage = "Duration must be between 1 and 1000 minutes")]
    public int EstimatedDurationMinutes { get; set; }
}
```

**Talking Point:**
"These data annotations prevent invalid data from entering the database. Regex validates language codes without writing custom code."

---

### Server-Side Validation in Controller

**What to show:**
- `ModelState.IsValid` check
- Custom validation logic
- Error messages returned to view

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(Course course)
{
    // Step 1: Check data annotations validations
    if (!ModelState.IsValid)
    {
        return View(course); // Re-display form with errors
    }

    try
    {
        // Step 2: Custom business logic validation
        var existingCourse = await _context.Courses
            .FirstOrDefaultAsync(c => c.IsoCode == course.IsoCode);

        if (existingCourse != null)
        {
            ModelState.AddModelError("IsoCode",
                $"A course for language '{course.IsoCode}' already exists");
            return View(course);
        }

        // Step 3: Save to database
        await _courseService.CreateCourseAsync(course);

        return RedirectToAction(nameof(Index));
    }
    catch (Exception ex)
    {
        ModelState.AddModelError("", "Error creating course: " + ex.Message);
        return View(course);
    }
}
```

**Talking Point:**
"Server-side validation ensures malicious users can't bypass client-side validation. This is a security requirement."

---

### Client-Side Validation in View

**What to show:**
- HTML5 attributes generated from model annotations
- User-friendly error messages
- Form won't submit if invalid

```html
<!-- Views/Courses/Create.cshtml -->
<form asp-action="Create" method="post">
    <div class="form-group mb-3">
        <label asp-for="Name" class="form-label">Course Name *</label>
        <input asp-for="Name" class="form-control" 
               placeholder="e.g., Spanish Basics" 
               maxlength="100" />
        <span asp-validation-for="Name" class="text-danger small"></span>
    </div>

    <div class="form-group mb-3">
        <label asp-for="IsoCode" class="form-label">Language Code *</label>
        <input asp-for="IsoCode" class="form-control" 
               placeholder="e.g., es" 
               maxlength="2" 
               pattern="[a-z]{2}"
               title="Enter 2 lowercase letters (e.g., en, fr, de)" />
        <span asp-validation-for="IsoCode" class="text-danger small"></span>
        <small class="text-muted">ISO 639-1 two-letter code</small>
    </div>

    <div class="form-group mb-3">
        <label asp-for="DifficultyLevel" class="form-label">Difficulty Level (1-100) *</label>
        <input asp-for="DifficultyLevel" type="number" 
               min="1" max="100" 
               class="form-control" />
        <span asp-validation-for="DifficultyLevel" class="text-danger small"></span>
    </div>

    <button type="submit" class="btn btn-primary">Create Course</button>
</form>

@section Scripts {
    @{await Html.RenderPartialAsync("_ValidationScriptsPartial");}
}
```

**Generated HTML:**
```html
<!-- ASP.NET Core generates these from model attributes -->
<input data-val="true" 
       data-val-required="Course name is required"
       data-val-length-max="100"
       data-val-length-min="3"
       data-val-length="Course name must be between 3 and 100 characters" />
```

**Talking Point:**
"ASP.NET Core automatically generates HTML5 validation attributes from model data annotations. No manual attribute writing needed."

---

## 3️⃣ ID REPLACEMENT IN CRUD FORMS

### Problem & Solution

**❌ BAD - Exposing IDs:**
```csharp
// URL: /courses/edit/1234
// Exposes internal database ID, security risk, not user-friendly
public IActionResult Edit(int id) { }
```

**✅ GOOD - Using Meaningful Properties:**
```csharp
// URL: /courses/spanish-basics/edit
// User-friendly, no ID exposure, SEO-friendly
public IActionResult Edit(string courseName) { }
```

---

### Example 1: Course - Finding by Name Instead of ID

**What to show:**
- Controller action uses name, not ID
- Database query by property
- URL is readable

```csharp
// CoursesController.cs
public async Task<IActionResult> Details(string courseName)
{
    var course = await _context.Courses
        .Include(c => c.Chapters)
            .ThenInclude(ch => ch.Lessons)
        .FirstOrDefaultAsync(c => c.Name == courseName);

    if (course == null)
        return NotFound();

    return View(course);
}

public async Task<IActionResult> Edit(string courseName)
{
    var course = await _context.Courses
        .FirstOrDefaultAsync(c => c.Name == courseName);

    if (course == null)
        return NotFound();

    return View(course);
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(string originalCourseName, Course course)
{
    var existingCourse = await _context.Courses
        .FirstOrDefaultAsync(c => c.Name == originalCourseName);

    if (existingCourse == null)
        return NotFound();

    existingCourse.Name = course.Name;
    existingCourse.Description = course.Description;
    existingCourse.DifficultyLevel = course.DifficultyLevel;

    await _courseService.UpdateCourseAsync(existingCourse);

    return RedirectToAction(nameof(Index));
}
```

**URL Examples:**
```
❌ /courses/edit/5
✓  /courses/spanish-basics/edit
```

---

### Example 2: Chapter - Composite Key (Course + Chapter Title)

**What to show:**
- Multiple parameters identify unique resource
- No numeric IDs exposed
- Clearer for users

```csharp
// ChaptersController.cs
public async Task<IActionResult> Edit(int courseId, string chapterTitle)
{
    var chapter = await _context.Chapters
        .FirstOrDefaultAsync(c => 
            c.CourseId == courseId && 
            c.Title == chapterTitle);

    if (chapter == null)
        return NotFound();

    return View(chapter);
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(int courseId, string originalTitle, Chapter chapter)
{
    var existingChapter = await _context.Chapters
        .FirstOrDefaultAsync(c => 
            c.CourseId == courseId && 
            c.Title == originalTitle);

    if (existingChapter == null)
        return NotFound();

    existingChapter.Title = chapter.Title;
    existingChapter.Description = chapter.Description;

    await _chapterService.UpdateChapterAsync(existingChapter);

    return RedirectToAction("Edit", new { courseId, chapterTitle = chapter.Title });
}
```

**URLs:**
```
❌ /chapters/edit/5/7
✓  /courses/spanish/chapters/basic-greetings/edit
```

---

### Example 3: Dropdowns - Show Names, Not IDs

**What to show:**
- Dropdown displays course names
- User never sees IDs
- Better UX

```csharp
// In Create view for Chapter
@model Chapter

<form asp-action="Create" method="post">
    <div class="form-group mb-3">
        <label class="form-label">Select Course *</label>
        <select asp-for="CourseId" asp-items="ViewBag.Courses" 
                class="form-control">
            <option value="">-- Select a course --</option>
        </select>
    </div>
</form>

// In ChaptersController
public async Task<IActionResult> Create()
{
    // ❌ BAD: Shows "1", "2", "3"
    // ViewBag.Courses = await _context.Courses
    //     .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Id.ToString() })
    //     .ToListAsync();

    // ✓ GOOD: Shows "Spanish Basics", "French Basics", "German Basics"
    ViewBag.Courses = await _context.Courses
        .Select(c => new SelectListItem 
        { 
            Value = c.Id.ToString(), 
            Text = c.Name  // Display course name, not ID
        })
        .ToListAsync();

    return View();
}
```

---

### Example 4: Achievement - Award by Title, Not ID

**What to show:**
- Achievement identified by title
- No ID exposure in business logic
- More maintainable

```csharp
// HomeController.cs - Achievement System
private async Task AwardAchievementIfNotEarned(string userId, string achievementTitle)
{
    // Find achievement by TITLE, not ID
    var achievement = await _context.Achievements
        .FirstOrDefaultAsync(a => a.Title == achievementTitle);

    if (achievement == null)
        return;

    // Check if user already earned this achievement
    var alreadyEarned = await _context.UserAchievements
        .AnyAsync(ua => ua.UserId == userId && ua.AchievementId == achievement.Id);

    if (!alreadyEarned)
    {
        await _context.UserAchievements.AddAsync(new UserAchievement
        {
            UserId = userId,
            AchievementId = achievement.Id,  // Use ID here for FK, but lookup was by name
            EarnedDate = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
    }
}

// Call it like this:
await AwardAchievementIfNotEarned(userId, "Beginner's Start");  // String title, not ID
await AwardAchievementIfNotEarned(userId, "Perfect Score");
await AwardAchievementIfNotEarned(userId, "Expert Learner");
```

**Talking Point:**
"When you award an achievement, you call it by name, not by ID. This is more readable and maintainable. If the database ID changes, the code still works."

---

## 4️⃣ CONSISTENT APPLICATION STRUCTURE

### Folder Organization

**What to show:**
- Every entity has: Model, Repository, Service
- Views organized by Controller
- Migrations tracked separately

```
Controllers/
  ├── HomeController.cs (User pages: Dashboard, Lesson, Quiz, Achievements, etc.)
  ├── CoursesController.cs (Admin: Courses CRUD)
  ├── ChaptersController.cs (Admin: Chapters CRUD)
  ├── LessonsController.cs (Admin: Lessons CRUD)
  └── AdminController.cs (Admin Dashboard)

Models/
  ├── Course.cs
  ├── Chapter.cs
  ├── Lesson.cs
  ├── Achievement.cs
  ├── UserAchievement.cs (NEW - for storing earned achievements)
  ├── ApplicationUser.cs
  └── VibeLangDbContext.cs

Repositories/
  ├── IRepository.cs (Generic interface)
  ├── Repository.cs (Generic implementation)
  ├── ICourseRepository.cs
  ├── CourseRepository.cs
  ├── IChapterRepository.cs
  ├── ChapterRepository.cs
  ├── ILessonRepository.cs
  └── LessonRepository.cs

Services/
  ├── ICourseService.cs
  ├── CourseService.cs
  ├── IChapterService.cs
  ├── ChapterService.cs
  ├── ILessonService.cs
  └── LessonService.cs

Views/
  ├── Home/ (User views)
  │   ├── Index.cshtml (Dashboard)
  │   ├── Courses.cshtml (Course catalog)
  │   ├── Lesson.cshtml (Lesson content)
  │   ├── Quiz.cshtml (Interactive quiz)
  │   ├── Achievements.cshtml (Progress tracking)
  │   └── Vocabulary.cshtml (Vocabulary list)
  ├── Courses/ (Admin views)
  │   ├── Index.cshtml (List all)
  │   ├── Create.cshtml (Add new)
  │   ├── Edit.cshtml (Update)
  │   └── Delete.cshtml (Confirm delete)
  ├── Chapters/
  │   ├── Index.cshtml
  │   ├── Create.cshtml
  │   ├── Edit.cshtml
  │   └── Delete.cshtml
  ├── Lessons/
  │   ├── Index.cshtml
  │   ├── Create.cshtml
  │   ├── Edit.cshtml
  │   └── Delete.cshtml
  └── Shared/
      ├── _Layout.cshtml (Master layout)
      ├── _AdminLayout.cshtml (Admin layout)
      └── _LoginPartial.cshtml

Migrations/
  ├── 20260323180938_InitialCreate.cs
  ├── 20260323193440_RefactorLanguageToCourse.cs
  ├── 20260503155240_AddUserAchievements.cs (NEW)
  └── VibeLangDbContextModelSnapshot.cs

Data/
  └── DbInitializer.cs (Seed 6 achievements)
```

---

### CRUD Controller Pattern (Consistent Across All)

**What to show:**
- Every controller follows same structure
- Makes code predictable and maintainable

```csharp
public class CoursesController : Controller
{
    private readonly ICourseService _courseService;
    private readonly ILogger<CoursesController> _logger;

    // Constructor: Dependency Injection
    public CoursesController(ICourseService courseService, ILogger<CoursesController> logger)
    {
        _courseService = courseService;
        _logger = logger;
    }

    // READ: List all
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var courses = await _courseService.GetAllCoursesAsync();
        return View(courses);
    }

    // READ: View single item
    [HttpGet("{courseName}")]
    public async Task<IActionResult> Details(string courseName)
    {
        var course = await _courseService.GetCourseDetailsAsync(courseName);
        if (course == null)
            return NotFound();
        return View(course);
    }

    // CREATE: Show form
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    // CREATE: Process form
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Course course)
    {
        if (!ModelState.IsValid)
            return View(course);

        await _courseService.CreateCourseAsync(course);
        return RedirectToAction(nameof(Index));
    }

    // UPDATE: Show form with existing data
    [HttpGet("{courseName}")]
    public async Task<IActionResult> Edit(string courseName)
    {
        var course = await _courseService.GetCourseDetailsAsync(courseName);
        if (course == null)
            return NotFound();
        return View(course);
    }

    // UPDATE: Process form
    [HttpPost("{originalName}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string originalName, Course course)
    {
        var existing = await _courseService.GetCourseDetailsAsync(originalName);
        if (existing == null)
            return NotFound();

        existing.Name = course.Name;
        existing.Description = course.Description;
        existing.DifficultyLevel = course.DifficultyLevel;

        await _courseService.UpdateCourseAsync(existing);
        return RedirectToAction(nameof(Index));
    }

    // DELETE: Show confirmation
    [HttpGet("{courseName}")]
    public async Task<IActionResult> Delete(string courseName)
    {
        var course = await _courseService.GetCourseDetailsAsync(courseName);
        if (course == null)
            return NotFound();
        return View(course);
    }

    // DELETE: Process deletion
    [HttpPost("{courseName}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string courseName)
    {
        await _courseService.DeleteCourseAsync(courseName);
        return RedirectToAction(nameof(Index));
    }
}
```

**Talking Point:**
"Every controller from Courses to Chapters to Lessons follows this exact same pattern. Consistency makes the code predictable and easier to maintain."

---

## 5️⃣ BONUS: ACHIEVEMENT SYSTEM (NEW - Demonstrates Integration)

### Model - `Models/UserAchievement.cs`

**What to show:**
- Junction table linking users to achievements
- Timestamp of when earned

```csharp
public class UserAchievement
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;
    [ForeignKey("UserId")]
    public ApplicationUser? User { get; set; }

    public int AchievementId { get; set; }
    [ForeignKey("AchievementId")]
    public Achievement? Achievement { get; set; }

    public DateTime EarnedDate { get; set; }
}
```

### Business Logic - `Controllers/HomeController.cs`

**What to show:**
- Retroactively checks completed lessons
- Awards achievements in real-time after quiz

```csharp
private async Task CheckAndAwardAchievements(string userId, int lessonId, int courseId, int score)
{
    // Award Beginner's Start on first lesson
    var completedCount = await _context.UserLessonProgresses
        .Where(ulp => ulp.UserId == userId && ulp.IsCompleted)
        .CountAsync();

    if (completedCount == 1)
        await AwardAchievementIfNotEarned(userId, "Beginner's Start");

    // Award Perfect Score on 100%
    if (score == 100)
        await AwardAchievementIfNotEarned(userId, "Perfect Score");

    // Award Expert Learner on completing all lessons in course
    var totalLessonsInCourse = await _context.Lessons
        .Where(l => l.Chapter!.CourseId == courseId)
        .CountAsync();

    var completedLessonsInCourse = await _context.UserLessonProgresses
        .Where(ulp => ulp.UserId == userId && ulp.IsCompleted && ulp.Lesson.Chapter!.CourseId == courseId)
        .CountAsync();

    if (completedLessonsInCourse == totalLessonsInCourse)
        await AwardAchievementIfNotEarned(userId, "Expert Learner");
}

private async Task AwardAchievementIfNotEarned(string userId, string achievementTitle)
{
    var achievement = await _context.Achievements
        .FirstOrDefaultAsync(a => a.Title == achievementTitle);

    if (achievement == null)
        return;

    var alreadyEarned = await _context.UserAchievements
        .AnyAsync(ua => ua.UserId == userId && ua.AchievementId == achievement.Id);

    if (!alreadyEarned)
    {
        await _context.UserAchievements.AddAsync(new UserAchievement
        {
            UserId = userId,
            AchievementId = achievement.Id,
            EarnedDate = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
    }
}
```

### View - `Views/Home/Achievements.cshtml`

**What to show:**
- Progress tracking with visual indicators
- All 6 achievements displayed
- "Beginner's Start" marked as earned

```html
<h1>Achievements</h1>

<div class="row mb-4">
    <div class="col-md-3">
        <div class="card">
            <h5>@ViewBag.EarnedCount</h5>
            <p>Achievements Earned</p>
        </div>
    </div>
    <div class="col-md-3">
        <div class="card">
            <h5>6</h5>
            <p>Total Available</p>
        </div>
    </div>
    <div class="col-md-3">
        <div class="card">
            <h5>@((ViewBag.EarnedCount / 6 * 100).ToString("F0"))%</h5>
            <p>Completion</p>
        </div>
    </div>
</div>

<h3>All Achievements</h3>
<div class="row">
    @foreach (var achievement in ViewBag.AllAchievements)
    {
        var isEarned = ViewBag.EarnedAchievements.ContainsKey(achievement.Id);
        
        <div class="col-md-4 mb-3">
            <div class="card @(isEarned ? "bg-success-subtle" : "")">
                <div class="card-body">
                    <h6>@achievement.IconPath @achievement.Title</h6>
                    <p class="small">@achievement.Description</p>
                    
                    @if (isEarned)
                    {
                        <span class="badge bg-success">✓ Earned</span>
                        <small>@ViewBag.EarnedAchievements[achievement.Id]</small>
                    }
                    else
                    {
                        <span class="badge bg-secondary">🔒 Locked</span>
                    }
                </div>
            </div>
        </div>
    }
</div>
```

---

## 📋 PRESENTATION CHECKLIST

### Files to Open in Code During Presentation

1. **Repository Pattern** (show layering)
   - [ ] Repositories/Repository.cs
   - [ ] Repositories/CourseRepository.cs
   - [ ] Services/CourseService.cs
   - [ ] Controllers/CoursesController.cs
   - [ ] Program.cs (DI section)

2. **Validations**
   - [ ] Models/Course.cs
   - [ ] Controllers/CoursesController.cs (ModelState check)
   - [ ] Views/Courses/Create.cshtml (HTML5 attributes)

3. **ID Replacement**
   - [ ] Controllers/CoursesController.cs (Edit with string parameter)
   - [ ] Controllers/ChaptersController.cs (Composite key)
   - [ ] Controllers/HomeController.cs (Achievement by title)

4. **Consistent Structure**
   - [ ] Show folder structure (Controllers, Services, Repositories)
   - [ ] Show CRUD pattern in CoursesController vs ChaptersController

5. **New Achievement Feature**
   - [ ] Models/UserAchievement.cs
   - [ ] Controllers/HomeController.cs (CheckAndAwardAchievements)
   - [ ] Views/Home/Achievements.cshtml
   - [ ] Run app and navigate to Achievements page

---

## 🗣️ KEY TALKING POINTS

> "Repository Pattern keeps data access separate from business logic, making code testable."

> "These validations prevent invalid data at three levels: data annotations, client-side HTML5, and server-side ModelState checks."

> "We never expose database IDs in URLs or forms. Instead, we use meaningful properties like course names and lesson titles. This is better for security and UX."

> "Every controller follows the same CRUD pattern. This consistency makes the codebase predictable and maintainable."

> "The achievement system demonstrates integration: it uses the repository pattern to query data, validates conditions in the service layer, and displays results in the view."

---

**Ready to present!** ✓
