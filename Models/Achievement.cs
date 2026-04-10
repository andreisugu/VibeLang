using System.ComponentModel.DataAnnotations;

namespace VibeLang.Models;

public class Achievement
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? IconPath { get; set; }
}
