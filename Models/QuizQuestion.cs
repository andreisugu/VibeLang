using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VibeLang.Models;

public class QuizQuestion
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string QuestionText { get; set; } = string.Empty;
    public int Tip { get; set; } // Matches JSON "tip" (1, 2, 3, 4)
    public string? CorrectAnswer { get; set; } // For Tip 1 & 2
    public string? MatchingDataJson { get; set; } // For Tip 3 (Serialized left/right words)

    public int QuizId { get; set; }
    [ForeignKey("QuizId")]
    public Quiz? Quiz { get; set; }
    public ICollection<QuizOption> Options { get; set; } = new List<QuizOption>();
}
