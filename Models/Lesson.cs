using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VibeLang.Models;

public class Lesson
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string Title { get; set; } = string.Empty;
    public string? Difficulty { get; set; } // Matches JSON "dificultate"
    public int Order { get; set; }
    
    public string? ContentJson { get; set; } // Stores vocabulary and quiz data

    public int ChapterId { get; set; }
    [ForeignKey("ChapterId")]
    public Chapter? Chapter { get; set; }

    public ICollection<VocabularyWord> VocabularyWords { get; set; } = new List<VocabularyWord>();
    public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
}
