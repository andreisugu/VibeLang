using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VibeLang.Models;

public class UserLessonProgress
{
    [Key]
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    [ForeignKey("UserId")]
    public ApplicationUser? User { get; set; }
    public int LessonId { get; set; }
    [ForeignKey("LessonId")]
    public Lesson? Lesson { get; set; }
    public bool IsCompleted { get; set; }
    public int ScoreAchieved { get; set; }
    public DateTime CompletionDate { get; set; }
}
