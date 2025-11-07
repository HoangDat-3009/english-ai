using System;
using System.Collections.Generic;
using EngAce.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EngAce.Api.Data;

public partial class EnglishMentorBuddyDbContext : DbContext
{
    public EnglishMentorBuddyDbContext()
    {
    }

    public EnglishMentorBuddyDbContext(DbContextOptions<EnglishMentorBuddyDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Achievement> Achievements { get; set; }

    public virtual DbSet<ReadingExercise> ReadingExercises { get; set; }

    public virtual DbSet<ReadingExerciseResult> ReadingExerciseResults { get; set; }

    public virtual DbSet<ReadingQuestion> ReadingQuestions { get; set; }

    public virtual DbSet<StudySession> StudySessions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserProgress> UserProgresses { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=EnglishMentorBuddyDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Achievement>(entity =>
        {
            entity.Property(e => e.EarnedAt).HasDefaultValueSql("(getutcdate())");
        });

        modelBuilder.Entity<ReadingExercise>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Level).HasDefaultValue("Beginner");
            entity.Property(e => e.SourceType).HasDefaultValue("manual");
            entity.Property(e => e.Type).HasDefaultValue("Part 5");
        });

        modelBuilder.Entity<ReadingQuestion>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
        });

        modelBuilder.Entity<StudySession>(entity =>
        {
            entity.Property(e => e.StartTime).HasDefaultValueSql("(getutcdate())");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.LastActiveAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Level).HasDefaultValue("Beginner");
        });

        modelBuilder.Entity<UserProgress>(entity =>
        {
            entity.Property(e => e.UserId).ValueGeneratedNever();
            entity.Property(e => e.LastUpdated).HasDefaultValueSql("(getutcdate())");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
