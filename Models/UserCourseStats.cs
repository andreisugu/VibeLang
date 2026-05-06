using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VibeLang.Models;

public class UserCourseStats
{
    [Key]
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    [ForeignKey("UserId")]
    public ApplicationUser? User { get; set; }
    public int CourseId { get; set; }
    [ForeignKey("CourseId")]
    public Course? Course { get; set; }
    public int TotalXP { get; set; }
    public int CurrentStreak { get; set; }
    public int MaxStreakEver { get; set; } = 0;  // Historical best streak
    public DateTime? LastActivityDate { get; set; }
    public bool StreakBrokenToday { get; set; } = false;  // Indicates if streak broke today
}
