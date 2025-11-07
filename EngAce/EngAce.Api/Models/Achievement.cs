using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EngAce.Api.Models;

[Index("UserId", Name = "IX_Achievements_UserId")]
public partial class Achievement
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    [StringLength(200)]
    public string Title { get; set; } = null!;

    [StringLength(1000)]
    public string Description { get; set; } = null!;

    [StringLength(50)]
    public string Type { get; set; } = null!;

    [StringLength(100)]
    public string Icon { get; set; } = null!;

    [StringLength(50)]
    public string Rarity { get; set; } = null!;

    public int Points { get; set; }

    public DateTime EarnedAt { get; set; }

    [Column(TypeName = "text")]
    public string? Criteria { get; set; }

    [Column(TypeName = "text")]
    public string? Metadata { get; set; }

    public bool IsVisible { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("Achievements")]
    public virtual User User { get; set; } = null!;
}
