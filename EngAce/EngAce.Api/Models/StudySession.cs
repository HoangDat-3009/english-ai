using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EngAce.Api.Models;

[Index("UserId", Name = "IX_StudySessions_UserId")]
public partial class StudySession
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public int DurationMinutes { get; set; }

    [StringLength(50)]
    public string ActivityType { get; set; } = null!;

    [StringLength(100)]
    public string? ActivityName { get; set; }

    public int ExercisesCompleted { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? AverageScore { get; set; }

    [Column("XPEarned")]
    public int Xpearned { get; set; }

    [Column(TypeName = "text")]
    public string? SessionData { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public bool IsCompleted { get; set; }

    [StringLength(100)]
    public string? DeviceType { get; set; }

    [StringLength(100)]
    public string? Platform { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("StudySessions")]
    public virtual User User { get; set; } = null!;
}
