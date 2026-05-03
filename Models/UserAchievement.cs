using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VibeLang.Models;

public class UserAchievement
{
    [Key]
    public int Id { get; set; }
    
    public string UserId { get; set; } = string.Empty;
    [ForeignKey("UserId")]
    public ApplicationUser? User { get; set; }
    
    public int AchievementId { get; set; }
    [ForeignKey("AchievementId")]
    public Achievement? Achievement { get; set; }
    
    [Required]
    public DateTime EarnedDate { get; set; }
}
