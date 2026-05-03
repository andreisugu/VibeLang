using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VibeLang.Models;

public class Course
{
    [Key]
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Course title is required.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters.")]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
    public string? Description { get; set; }
    
    [Required(ErrorMessage = "Language code (ISO 639-1) is required.")]
    [RegularExpression(@"^[a-z]{2}$", ErrorMessage = "ISO Code must be a 2-letter language code (e.g., 'en', 'ro', 'fr').")]
    public string IsoCode { get; set; } = string.Empty;
    
    public ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();
}
