using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VibeLang.Models;

public class VocabularyWord
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string Word { get; set; } = string.Empty;
    [Required]
    public string Translation { get; set; } = string.Empty;
    public string? ExampleSentence { get; set; }
    public int LessonId { get; set; }
    [ForeignKey("LessonId")]
    public Lesson? Lesson { get; set; }
}
