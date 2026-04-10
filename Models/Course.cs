using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VibeLang.Models;

public class Course
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    [Required]
    public string IsoCode { get; set; } = string.Empty;
    public ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();
}
