using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EngAce.Api.Models;

[Index("Email", Name = "IX_Users_Email", IsUnique = true)]
[Index("Username", Name = "IX_Users_Username", IsUnique = true)]
public partial class User
{
    [Key]
    public int Id { get; set; }

    [StringLength(100)]
    public string Username { get; set; } = null!;

    [StringLength(255)]
    public string Email { get; set; } = null!;

    [StringLength(200)]
    public string FullName { get; set; } = null!;

    [StringLength(50)]
    public string Level { get; set; } = null!;

    public int StudyStreak { get; set; }

    public int TotalStudyTime { get; set; }

    [Column("TotalXP")]
    public int TotalXp { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime LastActiveAt { get; set; }

    [StringLength(500)]
    public string? Avatar { get; set; }

    public bool IsActive { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<Achievement> Achievements { get; set; } = new List<Achievement>();

    [InverseProperty("User")]
    public virtual ICollection<ReadingExerciseResult> ReadingExerciseResults { get; set; } = new List<ReadingExerciseResult>();

    [InverseProperty("User")]
    public virtual ICollection<StudySession> StudySessions { get; set; } = new List<StudySession>();

    [InverseProperty("User")]
    public virtual UserProgress? UserProgress { get; set; }
}
