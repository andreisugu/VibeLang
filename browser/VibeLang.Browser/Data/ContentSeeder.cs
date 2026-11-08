using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using VibeLang.Models;

namespace VibeLang.Browser.Data;

/// <summary>Seeds the in-browser SQLite database from wwwroot/content.json.</summary>
public static class ContentSeeder
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public static async Task SeedAsync(VibeLangDbContext db, HttpClient http)
    {
        var bundle = await http.GetFromJsonAsync<ContentBundle>("content.json", JsonOptions);
        if (bundle is null) return;

        foreach (var c in bundle.Courses ?? new List<CourseDto>())
        {
            var course = new Course
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                IsoCode = c.IsoCode,
            };

            foreach (var ch in c.Chapters ?? new List<ChapterDto>())
            {
                var chapter = new Chapter
                {
                    Id = ch.Id,
                    Title = ch.Title,
                    Order = ch.Order,
                    CourseId = c.Id,
                };

                foreach (var l in ch.Lessons ?? new List<LessonDto>())
                {
                    var lesson = new Lesson
                    {
                        Id = l.Id,
                        Title = l.Title,
                        Difficulty = string.IsNullOrWhiteSpace(l.Difficulty) ? "beginner" : l.Difficulty!.ToLowerInvariant(),
                        Order = l.Order,
                        ContentJson = l.ContentJson,
                        ChapterId = ch.Id,
                    };

                    foreach (var v in l.Vocabulary ?? new List<VocabDto>())
                    {
                        lesson.VocabularyWords.Add(new VocabularyWord
                        {
                            Id = v.Id,
                            Word = v.Word,
                            Translation = v.Translation,
                            ExampleSentence = v.Example,
                            LessonId = l.Id,
                        });
                    }

                    chapter.Lessons.Add(lesson);
                }

                course.Chapters.Add(chapter);
            }

            db.Courses.Add(course);
        }

        foreach (var a in bundle.Achievements ?? new List<AchievementDto>())
        {
            db.Achievements.Add(new Achievement
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                IconPath = a.IconPath,
            });
        }

        await db.SaveChangesAsync();
    }

    public sealed class ContentBundle
    {
        public List<CourseDto> Courses { get; set; } = new();
        public List<AchievementDto> Achievements { get; set; } = new();
    }

    public sealed class CourseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public string IsoCode { get; set; } = "";
        public List<ChapterDto> Chapters { get; set; } = new();
    }

    public sealed class ChapterDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public int Order { get; set; }
        public List<LessonDto> Lessons { get; set; } = new();
    }

    public sealed class LessonDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string? Difficulty { get; set; }
        public int Order { get; set; }
        public string? ContentJson { get; set; }
        public List<VocabDto> Vocabulary { get; set; } = new();
    }

    public sealed class VocabDto
    {
        public int Id { get; set; }
        public string Word { get; set; } = "";
        public string Translation { get; set; } = "";
        public string? Example { get; set; }
    }

    public sealed class AchievementDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string? IconPath { get; set; }
    }
}
