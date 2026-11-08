using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VibeLang.Browser;
using VibeLang.Browser.Data;
using VibeLang.Browser.Services;
using VibeLang.Models;
using VibeLang.Repositories;
using VibeLang.Services;

SQLitePCL.Batteries_V2.Init();

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddDbContext<VibeLangDbContext>(options => options.UseSqlite("Data Source=vibelang-demo.db"));

builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<VibeLangDbContext>();

// Real repositories and services
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IChapterRepository, ChapterRepository>();
builder.Services.AddScoped<ILessonRepository, LessonRepository>();
builder.Services.AddScoped<IQuizRepository, QuizRepository>();
builder.Services.AddScoped<IStatsRepository, StatsRepository>();
builder.Services.AddScoped<IUserAchievementRepository, UserAchievementRepository>();
builder.Services.AddScoped<IUserVocabularyRepository, UserVocabularyRepository>();
builder.Services.AddScoped<IVocabularyWordRepository, VocabularyWordRepository>();
builder.Services.AddScoped<ILessonVocabularyRepository, LessonVocabularyRepository>();

builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IChapterService, ChapterService>();
builder.Services.AddScoped<ILessonService, LessonService>();
builder.Services.AddScoped<IQuizService, QuizService>();
builder.Services.AddScoped<IStatsService, StatsService>();
builder.Services.AddScoped<IAchievementService, AchievementService>();
builder.Services.AddScoped<IVocabularyService, VocabularyService>();
builder.Services.AddScoped<ILessonVocabularyService, LessonVocabularyService>();

builder.Services.AddScoped<SnapshotService>();
builder.Services.AddScoped<BrowserAuthService>();
builder.Services.AddScoped<LessonResultService>();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<VibeLangDbContext>();
    await db.Database.EnsureCreatedAsync();

    if (!await db.Courses.AnyAsync())
    {
        var http = scope.ServiceProvider.GetRequiredService<HttpClient>();
        await ContentSeeder.SeedAsync(db, http);
    }

    var snapshot = scope.ServiceProvider.GetRequiredService<SnapshotService>();
    var restored = await snapshot.RestoreAsync(db);
    if (!restored)
    {
        await DemoDataSeeder.SeedAsync(scope.ServiceProvider);
    }

    await snapshot.SaveAsync(db);
}

await host.RunAsync();
