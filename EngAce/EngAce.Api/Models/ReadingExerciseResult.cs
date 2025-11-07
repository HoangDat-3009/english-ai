using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EngAce.Api.Models;

[Index("ReadingExerciseId", Name = "IX_ReadingExerciseResults_ReadingExerciseId")]
[Index("UserId", Name = "IX_ReadingExerciseResults_UserId")]
public partial class ReadingExerciseResult
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    public int ReadingExerciseId { get; set; }

    public int Score { get; set; }

    public int TotalQuestions { get; set; }

    public int CorrectAnswers { get; set; }

    [Column(TypeName = "text")]
    public string UserAnswers { get; set; } = null!;

    public TimeOnly TimeSpent { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime CompletedAt { get; set; }

    public int? DifficultyRating { get; set; }

    [StringLength(1000)]
    public string? UserFeedback { get; set; }

    public bool IsCompleted { get; set; }

    [ForeignKey("ReadingExerciseId")]
    [InverseProperty("ReadingExerciseResults")]
    public virtual ReadingExercise ReadingExercise { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("ReadingExerciseResults")]
    public virtual User User { get; set; } = null!;
}
