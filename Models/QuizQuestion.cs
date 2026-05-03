using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VibeLang.Models;

public class QuizQuestion
{
    [Key]
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Question text is required.")]
    [StringLength(500, MinimumLength = 5, ErrorMessage = "Question must be between 5 and 500 characters.")]
    public string QuestionText { get; set; } = string.Empty;
    
    [Range(1, 4, ErrorMessage = "Tip must be between 1 and 4.")]
    public int Tip { get; set; } // Matches JSON "tip" (1, 2, 3, 4)
    
    [StringLength(200, ErrorMessage = "Correct answer cannot exceed 200 characters.")]
    public string? CorrectAnswer { get; set; } // For Tip 1 & 2
    
    public string? MatchingDataJson { get; set; } // For Tip 3 (Serialized left/right words)

    public int QuizId { get; set; }
    [ForeignKey("QuizId")]
    public Quiz? Quiz { get; set; }
    
    public ICollection<QuizOption> Options { get; set; } = new List<QuizOption>();
}
