using Microsoft.EntityFrameworkCore;
using VibeLang.Models;
using Microsoft.AspNetCore.Identity;
using VibeLang.Repositories;
using VibeLang.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure routing to use lowercase URLs
builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

// Register DbContext with PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<VibeLangDbContext>(options =>
    options.UseNpgsql(connectionString, o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

// Configure Identity with Role support
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<VibeLangDbContext>()
.AddDefaultUI()
.AddDefaultTokenProviders();

// Wire up custom cookie paths for login and access-denied redirects
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
});

// Register Repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IChapterRepository, ChapterRepository>();
builder.Services.AddScoped<ILessonRepository, LessonRepository>();
builder.Services.AddScoped<IQuizRepository, QuizRepository>();
builder.Services.AddScoped<IUserAchievementRepository, UserAchievementRepository>();
builder.Services.AddScoped<IUserVocabularyRepository, UserVocabularyRepository>();
builder.Services.AddScoped<IVocabularyWordRepository, VocabularyWordRepository>();
builder.Services.AddScoped<IStatsRepository, StatsRepository>();
builder.Services.AddScoped<ILessonVocabularyRepository, LessonVocabularyRepository>();

// Register Services
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IChapterService, ChapterService>();
builder.Services.AddScoped<ILessonService, LessonService>();
builder.Services.AddScoped<IQuizService, QuizService>();
builder.Services.AddScoped<IAchievementService, AchievementService>();
builder.Services.AddScoped<IVocabularyService, VocabularyService>();
builder.Services.AddScoped<ILessonVocabularyService, LessonVocabularyService>();
builder.Services.AddScoped<IStatsService, StatsService>();

// ── Authentication Service Layer ─────────────────────────────────────────
//  IAuthService (interface) is implemented by AuthService (concrete class).
//  Registered as Scoped so each HTTP request gets its own instance.
//
//  Consumers (injected via constructor):
//    • LoginModel    (Areas/Identity/Pages/Account/Login.cshtml.cs)
//    • RegisterModel (Areas/Identity/Pages/Account/Register.cshtml.cs)
//    • LogoutModel   (Areas/Identity/Pages/Account/Logout.cshtml.cs)
//
//  This ensures UserManager and SignInManager are never referenced
//  directly in Razor Page code-behind files (Service Layer pattern).
// ─────────────────────────────────────────────────────────────────────────
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

// Seed Database, Roles, and backfill any role-less users
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<VibeLangDbContext>();
        await VibeLang.Data.DbInitializer.Initialize(context);

        // 1. Seed application roles (Admin and User) using RoleManager
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        string[] roles = { "Admin", "User" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // 2. Backfill: assign "User" role to any existing account that has no role.
        //    This handles accounts registered before role-assignment was implemented.
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        foreach (var user in userManager.Users.ToList())
        {
            var userRoles = await userManager.GetRolesAsync(user);
            if (userRoles.Count == 0)
            {
                await userManager.AddToRoleAsync(user, "User");
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database or roles.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication(); // Added
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages(); // Added for Identity UI

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
