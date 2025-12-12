using Microsoft.EntityFrameworkCore;
using Entities.Models;

namespace Entities.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // DbSets for simplified schema - only essential entities
    public DbSet<Models.User> Users { get; set; } = null!;
    public DbSet<Exercise> Exercises { get; set; } = null!; 
    public DbSet<Completion> Completions { get; set; } = null!;
    public DbSet<CompletionScore> CompletionScores { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity - matches schema gốc
        modelBuilder.Entity<Models.User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.GoogleId).IsUnique();
            entity.HasIndex(e => e.FacebookId).IsUnique();
            entity.HasIndex(e => e.Role);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.AccountType);
            entity.HasIndex(e => e.TotalXp);
            
            entity.Property(e => e.Username)
                .IsRequired()
                .HasMaxLength(50);
                
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(100);
                
            entity.Property(e => e.Password)
                .IsRequired()
                .HasMaxLength(255);
                
            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("active"); // ENUM('active','inactive','banned')

            entity.Property(e => e.AccountType)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("free"); // ENUM('free','premium')

            entity.Property(e => e.Role)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("user"); // ENUM('user','admin')

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql(GetUtcNowSql());
                
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql(GetUtcNowSql())
                .ValueGeneratedOnAddOrUpdate();
        });

        // Configure Exercise entity - matches schema gốc
        modelBuilder.Entity<Models.Exercise>(entity =>
        {
            entity.HasKey(e => e.ExerciseId);
            entity.HasIndex(e => e.Title);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.Level);
            entity.HasIndex(e => e.IsActive);
            
            entity.Property(e => e.Title)
                .IsRequired(); // NVARCHAR(200)
                
            entity.Property(e => e.Questions)
                .IsRequired(); // JSON NOT NULL
                
            entity.Property(e => e.CorrectAnswers)
                .IsRequired(); // JSON NOT NULL

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql(GetUtcNowSql());
                
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql(GetUtcNowSql())
                .ValueGeneratedOnAddOrUpdate();

            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);

            entity.HasOne(e => e.CreatedByUser)
                .WithMany()
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure Completion entity - matches schema gốc
        modelBuilder.Entity<Models.Completion>(entity =>
        {
            entity.HasKey(e => e.CompletionId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ExerciseId);
            entity.HasIndex(e => e.Score);
            
            // UNIQUE constraint: (user_id, exercise_id, attempts)
            entity.HasIndex(e => new { e.UserId, e.ExerciseId, e.Attempts })
                .IsUnique();
            
            entity.Property(e => e.Score)
                .HasColumnType("decimal(5,2)");
                
            entity.Property(e => e.IsCompleted)
                .HasDefaultValue(false);
                
            entity.Property(e => e.Attempts)
                .HasDefaultValue(1);

            entity.HasOne(e => e.User)
                .WithMany(u => u.Completions)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Exercise)
                .WithMany(ex => ex.Completions)
                .HasForeignKey(e => e.ExerciseId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Exercise)
                .WithMany(ex => ex.Completions)
                .HasForeignKey(e => e.ExerciseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure CompletionScore entity - matches schema gốc
        modelBuilder.Entity<Models.CompletionScore>(entity =>
        {
            entity.HasKey(e => e.CompletionScoreId);
            entity.HasIndex(e => e.CompletionId);
            
            entity.Property(e => e.Points)
                .HasColumnType("decimal(5,2)");
                
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql(GetUtcNowSql());

            entity.HasOne(e => e.Completion)
                .WithMany(c => c.CompletionScores)
                .HasForeignKey(e => e.CompletionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

    }

    private static string GetUtcNowSql()
    {
        // Use CURRENT_TIMESTAMP(6) so MySQL can generate UTC timestamps by configuration
        return "CURRENT_TIMESTAMP(6)";
    }

}