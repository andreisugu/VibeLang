using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VibeLang.Models;

public class VocabularyWord
{
    [Key]
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Word is required.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Word must be between 1 and 100 characters.")]
    public string Word { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Translation is required.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Translation must be between 1 and 100 characters.")]
    public string Translation { get; set; } = string.Empty;
    
    [StringLength(300, ErrorMessage = "Example sentence cannot exceed 300 characters.")]
    public string? ExampleSentence { get; set; }
    
    public int LessonId { get; set; }
    [ForeignKey("LessonId")]
    public Lesson? Lesson { get; set; }
}
