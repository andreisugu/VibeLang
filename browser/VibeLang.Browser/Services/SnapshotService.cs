using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using VibeLang.Models;

namespace VibeLang.Browser.Services;

/// <summary>
/// Persists the mutable part of the SQLite database to localStorage. Course
/// content is static and reseeded from content.json; everything that can change
/// (users, roles, progress, stats, vocabulary words, achievements) is saved.
/// </summary>
public class SnapshotService
{
    private const string Key = "vibelang-state-v1";
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly IJSRuntime _js;

    public SnapshotService(IJSRuntime js) => _js = js;

    public async Task<bool> RestoreAsync(VibeLangDbContext db)
    {
        var raw = await _js.InvokeAsync<string?>("localStorage.getItem", Key);
        if (string.IsNullOrWhiteSpace(raw)) return false;

        Snapshot? snap;
        try
        {
            snap = JsonSerializer.Deserialize<Snapshot>(raw, JsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine("[snapshot] deserialize failed: " + ex.Message);
            return false;
        }

        if (snap is null || snap.Users.Count == 0) return false;

        try
        {
            await using var tx = await db.Database.BeginTransactionAsync();

            db.UserLessonProgresses.RemoveRange(db.UserLessonProgresses);
            db.UserCourseStats.RemoveRange(db.UserCourseStats);
            db.UserVocabularies.RemoveRange(db.UserVocabularies);
            db.UserAchievements.RemoveRange(db.UserAchievements);
            db.UserRoles.RemoveRange(db.UserRoles);
            db.Users.RemoveRange(db.Users);
            db.Roles.RemoveRange(db.Roles);
            db.VocabularyWords.RemoveRange(db.VocabularyWords);
            await db.SaveChangesAsync();

            foreach (var u in snap.Users)
            {
                db.Users.Add(new ApplicationUser
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    NormalizedUserName = u.NormalizedUserName,
                    Email = u.Email,
                    NormalizedEmail = u.NormalizedEmail,
                    EmailConfirmed = u.EmailConfirmed,
                    PasswordHash = u.PasswordHash,
                    SecurityStamp = u.SecurityStamp,
                    ConcurrencyStamp = u.ConcurrencyStamp,
                    PhoneNumber = u.PhoneNumber,
                    PhoneNumberConfirmed = u.PhoneNumberConfirmed,
                    TwoFactorEnabled = u.TwoFactorEnabled,
                    LockoutEnd = u.LockoutEnd,
                    LockoutEnabled = u.LockoutEnabled,
                    AccessFailedCount = u.AccessFailedCount,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    ProfilePicturePath = u.ProfilePicturePath,
                });
            }

            foreach (var r in snap.Roles)
            {
                db.Roles.Add(new IdentityRole
                {
                    Id = r.Id,
                    Name = r.Name,
                    NormalizedName = r.NormalizedName,
                    ConcurrencyStamp = r.ConcurrencyStamp,
                });
            }

            foreach (var ur in snap.UserRoles)
                db.UserRoles.Add(new IdentityUserRole<string> { UserId = ur.UserId, RoleId = ur.RoleId });

            foreach (var p in snap.Progress)
            {
                db.UserLessonProgresses.Add(new UserLessonProgress
                {
                    Id = p.Id, UserId = p.UserId, LessonId = p.LessonId,
                    IsCompleted = p.IsCompleted, ScoreAchieved = p.ScoreAchieved,
                    CompletionDate = p.CompletionDate,
                });
            }

            foreach (var s in snap.Stats)
            {
                db.UserCourseStats.Add(new UserCourseStats
                {
                    Id = s.Id, UserId = s.UserId, CourseId = s.CourseId,
                    TotalXP = s.TotalXP, CurrentStreak = s.CurrentStreak,
                    MaxStreakEver = s.MaxStreakEver, LastActivityDate = s.LastActivityDate,
                    StreakBrokenToday = s.StreakBrokenToday,
                });
            }

            foreach (var w in snap.Words)
            {
                db.VocabularyWords.Add(new VocabularyWord
                {
                    Id = w.Id, Word = w.Word, Translation = w.Translation,
                    ExampleSentence = w.Example, LessonId = w.LessonId,
                });
            }

            foreach (var v in snap.Vocabulary)
            {
                db.UserVocabularies.Add(new UserVocabulary
                {
                    Id = v.Id, UserId = v.UserId, WordId = v.WordId,
                    Status = v.Status, LastReviewed = v.LastReviewed,
                });
            }

            foreach (var a in snap.Achievements)
            {
                db.UserAchievements.Add(new UserAchievement
                {
                    Id = a.Id, UserId = a.UserId, AchievementId = a.AchievementId,
                    EarnedDate = a.EarnedDate,
                });
            }

            await db.SaveChangesAsync();
            await tx.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("[snapshot] restore failed: " + ex.Message);
            return false;
        }
    }

    public async Task SaveAsync(VibeLangDbContext db)
    {
        var snapshot = new Snapshot
        {
            Users = await db.Users.AsNoTracking().Select(u => new UserDto
            {
                Id = u.Id, UserName = u.UserName, NormalizedUserName = u.NormalizedUserName,
                Email = u.Email, NormalizedEmail = u.NormalizedEmail,
                EmailConfirmed = u.EmailConfirmed, PasswordHash = u.PasswordHash,
                SecurityStamp = u.SecurityStamp, ConcurrencyStamp = u.ConcurrencyStamp,
                PhoneNumber = u.PhoneNumber, PhoneNumberConfirmed = u.PhoneNumberConfirmed,
                TwoFactorEnabled = u.TwoFactorEnabled, LockoutEnd = u.LockoutEnd,
                LockoutEnabled = u.LockoutEnabled, AccessFailedCount = u.AccessFailedCount,
                FirstName = u.FirstName, LastName = u.LastName,
                ProfilePicturePath = u.ProfilePicturePath,
            }).ToListAsync(),
            Roles = await db.Roles.AsNoTracking().Select(r => new RoleDto
            {
                Id = r.Id, Name = r.Name, NormalizedName = r.NormalizedName,
                ConcurrencyStamp = r.ConcurrencyStamp,
            }).ToListAsync(),
            UserRoles = await db.UserRoles.AsNoTracking().Select(ur => new UserRoleDto
            {
                UserId = ur.UserId, RoleId = ur.RoleId,
            }).ToListAsync(),
            Progress = await db.UserLessonProgresses.AsNoTracking().Select(p => new ProgressDto
            {
                Id = p.Id, UserId = p.UserId, LessonId = p.LessonId,
                IsCompleted = p.IsCompleted, ScoreAchieved = p.ScoreAchieved,
                CompletionDate = p.CompletionDate,
            }).ToListAsync(),
            Stats = await db.UserCourseStats.AsNoTracking().Select(s => new StatsDto
            {
                Id = s.Id, UserId = s.UserId, CourseId = s.CourseId,
                TotalXP = s.TotalXP, CurrentStreak = s.CurrentStreak,
                MaxStreakEver = s.MaxStreakEver, LastActivityDate = s.LastActivityDate,
                StreakBrokenToday = s.StreakBrokenToday,
            }).ToListAsync(),
            Words = await db.VocabularyWords.AsNoTracking().Select(w => new WordDto
            {
                Id = w.Id, Word = w.Word, Translation = w.Translation,
                Example = w.ExampleSentence, LessonId = w.LessonId,
            }).ToListAsync(),
            Vocabulary = await db.UserVocabularies.AsNoTracking().Select(v => new VocabularyDto
            {
                Id = v.Id, UserId = v.UserId, WordId = v.WordId,
                Status = v.Status, LastReviewed = v.LastReviewed,
            }).ToListAsync(),
            Achievements = await db.UserAchievements.AsNoTracking().Select(a => new AchievementDto
            {
                Id = a.Id, UserId = a.UserId, AchievementId = a.AchievementId,
                EarnedDate = a.EarnedDate,
            }).ToListAsync(),
        };

        var json = JsonSerializer.Serialize(snapshot, JsonOptions);
        await _js.InvokeVoidAsync("localStorage.setItem", Key, json);
    }

    public class Snapshot
    {
        public List<UserDto> Users { get; set; } = new();
        public List<RoleDto> Roles { get; set; } = new();
        public List<UserRoleDto> UserRoles { get; set; } = new();
        public List<ProgressDto> Progress { get; set; } = new();
        public List<StatsDto> Stats { get; set; } = new();
        public List<WordDto> Words { get; set; } = new();
        public List<VocabularyDto> Vocabulary { get; set; } = new();
        public List<AchievementDto> Achievements { get; set; } = new();
    }

    public class UserDto
    {
        public string Id { get; set; } = "";
        public string? UserName { get; set; }
        public string? NormalizedUserName { get; set; }
        public string? Email { get; set; }
        public string? NormalizedEmail { get; set; }
        public bool EmailConfirmed { get; set; }
        public string? PasswordHash { get; set; }
        public string? SecurityStamp { get; set; }
        public string? ConcurrencyStamp { get; set; }
        public string? PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ProfilePicturePath { get; set; }
    }

    public class RoleDto
    {
        public string Id { get; set; } = "";
        public string? Name { get; set; }
        public string? NormalizedName { get; set; }
        public string? ConcurrencyStamp { get; set; }
    }

    public class UserRoleDto { public string UserId { get; set; } = ""; public string RoleId { get; set; } = ""; }
    public class ProgressDto
    {
        public int Id { get; set; } public string UserId { get; set; } = ""; public int LessonId { get; set; }
        public bool IsCompleted { get; set; } public int ScoreAchieved { get; set; } public DateTime CompletionDate { get; set; }
    }
    public class StatsDto
    {
        public int Id { get; set; } public string UserId { get; set; } = ""; public int CourseId { get; set; }
        public int TotalXP { get; set; } public int CurrentStreak { get; set; } public int MaxStreakEver { get; set; }
        public DateTime? LastActivityDate { get; set; } public bool StreakBrokenToday { get; set; }
    }
    public class WordDto
    {
        public int Id { get; set; } public string Word { get; set; } = ""; public string Translation { get; set; } = "";
        public string? Example { get; set; } public int LessonId { get; set; }
    }
    public class VocabularyDto
    {
        public int Id { get; set; } public string UserId { get; set; } = ""; public int WordId { get; set; }
        public string Status { get; set; } = "New"; public DateTime LastReviewed { get; set; }
    }
    public class AchievementDto
    {
        public int Id { get; set; } public string UserId { get; set; } = ""; public int AchievementId { get; set; }
        public DateTime EarnedDate { get; set; }
    }
}
