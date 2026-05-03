using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VibeLang.Models;

public class Quiz
{
    [Key]
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Quiz title is required.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 150 characters.")]
    public string Title { get; set; } = string.Empty;
    
    public int LessonId { get; set; }
    [ForeignKey("LessonId")]
    public Lesson? Lesson { get; set; }
    
    public ICollection<QuizQuestion> Questions { get; set; } = new List<QuizQuestion>();
}
