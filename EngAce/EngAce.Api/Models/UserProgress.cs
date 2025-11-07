using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EngAce.Api.Models;

public partial class UserProgress
{
    [Key]
    public int UserId { get; set; }

    public int ListeningScore { get; set; }

    public int SpeakingScore { get; set; }

    public int ReadingScore { get; set; }

    public int WritingScore { get; set; }

    public int TotalScore { get; set; }

    public int CompletedExercises { get; set; }

    public int TotalExercisesAvailable { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal AverageAccuracy { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal ListeningAccuracy { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal ReadingAccuracy { get; set; }

    public TimeOnly AverageTimePerExercise { get; set; }

    public DateTime LastUpdated { get; set; }

    [Column(TypeName = "text")]
    public string? WeeklyProgressData { get; set; }

    public int CurrentStreak { get; set; }

    public int WeeklyGoal { get; set; }

    public int MonthlyGoal { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("UserProgress")]
    public virtual User User { get; set; } = null!;
}
