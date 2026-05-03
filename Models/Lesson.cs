using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VibeLang.Models;

public class Lesson
{
    [Key]
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Lesson title is required.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters.")]
    public string Title { get; set; } = string.Empty;
    
    [RegularExpression(@"^(beginner|intermediate|advanced)$", ErrorMessage = "Difficulty must be 'beginner', 'intermediate', or 'advanced'.")]
    public string? Difficulty { get; set; } // Matches JSON "dificultate"
    
    [Range(1, 999, ErrorMessage = "Order must be between 1 and 999.")]
    public int Order { get; set; }
    
    public string? ContentJson { get; set; } // Stores vocabulary and quiz data

    public int ChapterId { get; set; }
    [ForeignKey("ChapterId")]
    public Chapter? Chapter { get; set; }

    public ICollection<VocabularyWord> VocabularyWords { get; set; } = new List<VocabularyWord>();
    public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
}
