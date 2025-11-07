using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EngAce.Api.Models;

[Index("ReadingExerciseId", Name = "IX_ReadingQuestions_ReadingExerciseId")]
public partial class ReadingQuestion
{
    [Key]
    public int Id { get; set; }

    public int ReadingExerciseId { get; set; }

    [Column(TypeName = "text")]
    public string QuestionText { get; set; } = null!;

    [StringLength(500)]
    public string OptionA { get; set; } = null!;

    [StringLength(500)]
    public string OptionB { get; set; } = null!;

    [StringLength(500)]
    public string OptionC { get; set; } = null!;

    [StringLength(500)]
    public string OptionD { get; set; } = null!;

    public int CorrectAnswer { get; set; }

    [Column(TypeName = "text")]
    public string? Explanation { get; set; }

    public int OrderNumber { get; set; }

    public int Difficulty { get; set; }

    public DateTime CreatedAt { get; set; }

    [ForeignKey("ReadingExerciseId")]
    [InverseProperty("ReadingQuestions")]
    public virtual ReadingExercise ReadingExercise { get; set; } = null!;
}
