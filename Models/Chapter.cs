using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VibeLang.Models;

public class Chapter
{
    [Key]
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Chapter title is required.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 150 characters.")]
    public string Title { get; set; } = string.Empty;
    
    [Range(1, 999, ErrorMessage = "Order must be between 1 and 999.")]
    public int Order { get; set; }
    
    public int CourseId { get; set; }
    [ForeignKey("CourseId")]
    public Course? Course { get; set; }
    
    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}
