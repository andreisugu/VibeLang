using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VibeLang.Models;

public class UserVocabulary
{
    [Key]
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    [ForeignKey("UserId")]
    public ApplicationUser? User { get; set; }
    public int WordId { get; set; }
    [ForeignKey("WordId")]
    public VocabularyWord? Word { get; set; }
    public string Status { get; set; } = "New";
    public DateTime LastReviewed { get; set; } = DateTime.UtcNow;
}
