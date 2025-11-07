using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EngAce.Api.Models;

public partial class ReadingExercise
{
    [Key]
    public int Id { get; set; }

    [StringLength(200)]
    public string Name { get; set; } = null!;

    [Column(TypeName = "text")]
    public string Content { get; set; } = null!;

    [StringLength(50)]
    public string Level { get; set; } = null!;

    [StringLength(50)]
    public string Type { get; set; } = null!;

    [StringLength(50)]
    public string SourceType { get; set; } = null!;

    [StringLength(100)]
    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [StringLength(500)]
    public string? OriginalFileName { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    public int EstimatedMinutes { get; set; }

    public bool IsActive { get; set; }

    [InverseProperty("ReadingExercise")]
    public virtual ICollection<ReadingExerciseResult> ReadingExerciseResults { get; set; } = new List<ReadingExerciseResult>();

    [InverseProperty("ReadingExercise")]
    public virtual ICollection<ReadingQuestion> ReadingQuestions { get; set; } = new List<ReadingQuestion>();
}
