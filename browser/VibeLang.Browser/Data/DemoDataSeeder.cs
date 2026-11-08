using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VibeLang.Models;

namespace VibeLang.Browser.Data;

/// <summary>Seeds the two roles and a few demo accounts on a fresh browser database.</summary>
public static class DemoDataSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<VibeLangDbContext>();
        var users = services.GetRequiredService<UserManager<ApplicationUser>>();

        if (!await db.Roles.AnyAsync())
        {
            db.Roles.AddRange(
                new IdentityRole { Id = Guid.NewGuid().ToString(), Name = "Admin", NormalizedName = "ADMIN", ConcurrencyStamp = Guid.NewGuid().ToString() },
                new IdentityRole { Id = Guid.NewGuid().ToString(), Name = "User", NormalizedName = "USER", ConcurrencyStamp = Guid.NewGuid().ToString() });
            await db.SaveChangesAsync();
        }

        if (await db.Users.AnyAsync()) return;

        var adminRole = await db.Roles.FirstAsync(r => r.Name == "Admin");
        var userRole = await db.Roles.FirstAsync(r => r.Name == "User");

        async Task Make(string email, string password, string firstName, string lastName, string roleId, int xp)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = firstName,
                LastName = lastName,
            };

            var result = await users.CreateAsync(user, password);
            if (!result.Succeeded) return;

            db.UserRoles.Add(new IdentityUserRole<string> { UserId = user.Id, RoleId = roleId });

            if (xp > 0)
            {
                db.UserCourseStats.Add(new UserCourseStats
                {
                    UserId = user.Id,
                    CourseId = 1,
                    TotalXP = xp,
                    CurrentStreak = 1,
                    MaxStreakEver = 1,
                    LastActivityDate = DateTime.UtcNow,
                });
            }
        }

        await Make("admin@vibelang.com", "Admin123", "Admin", "VibeLang", adminRole.Id, 0);
        await Make("student@vibelang.com", "Student123", "Demo", "Student", userRole.Id, 0);
        await Make("ada@vibelang.com", "Student123", "Ada", "Lovelace", userRole.Id, 180);
        await Make("grace@vibelang.com", "Student123", "Grace", "Hopper", userRole.Id, 60);

        await db.SaveChangesAsync();
    }
}
