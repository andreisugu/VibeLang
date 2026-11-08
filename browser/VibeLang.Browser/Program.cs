using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.EntityFrameworkCore;
using VibeLang.Browser;
using VibeLang.Browser.Data;
using VibeLang.Browser.Services;

SQLitePCL.Batteries_V2.Init();

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddDbContext<BrowserDbContext>(options => options.UseSqlite("Data Source=vibelang-demo.db"));
builder.Services.AddScoped<DemoSession>();
builder.Services.AddScoped<ProgressService>();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BrowserDbContext>();
    await db.Database.EnsureCreatedAsync();
    if (!await db.Courses.AnyAsync())
    {
        var http = scope.ServiceProvider.GetRequiredService<HttpClient>();
        await ContentSeeder.SeedAsync(db, http);
    }
}

await host.RunAsync();
