using Microsoft.EntityFrameworkCore;
using Entities.Models;

namespace Entities.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // DbSets for all entities
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<ReadingExercise> ReadingExercises { get; set; } = null!;
    public DbSet<ReadingQuestion> ReadingQuestions { get; set; } = null!;
    public DbSet<ReadingExerciseResult> ReadingExerciseResults { get; set; } = null!;
    public DbSet<UserProgress> UserProgresses { get; set; } = null!;
    public DbSet<Achievement> Achievements { get; set; } = null!;
    public DbSet<StudySession> StudySessions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            
            entity.Property(e => e.Username)
                .IsRequired()
                .HasMaxLength(100);
                
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255);
                
            entity.Property(e => e.FullName)
                .IsRequired()
                .HasMaxLength(200);
                
            entity.Property(e => e.Level)
                .HasMaxLength(50)
                .HasDefaultValue("Beginner");
                
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql(GetUtcNowSql());
                
            entity.Property(e => e.LastActiveAt)
                .HasDefaultValueSql(GetUtcNowSql());
        });

        // Configure ReadingExercise entity
        modelBuilder.Entity<ReadingExercise>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);
                
            entity.Property(e => e.Level)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("Beginner");
                
            entity.Property(e => e.Type)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("Part 5");
                
            entity.Property(e => e.SourceType)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("manual");
                
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql(GetUtcNowSql());
                
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);

            // Configure relationship with questions
            entity.HasMany(e => e.Questions)
                .WithOne(q => q.ReadingExercise)
                .HasForeignKey(q => q.ReadingExerciseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure ReadingQuestion entity
        modelBuilder.Entity<ReadingQuestion>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.QuestionText)
                .IsRequired();
                
            entity.Property(e => e.OptionA)
                .IsRequired()
                .HasMaxLength(500);
                
            entity.Property(e => e.OptionB)
                .IsRequired()
                .HasMaxLength(500);
                
            entity.Property(e => e.OptionC)
                .IsRequired()
                .HasMaxLength(500);
                
            entity.Property(e => e.OptionD)
                .IsRequired()
                .HasMaxLength(500);
                
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql(GetUtcNowSql());
        });

        // Configure ReadingExerciseResult entity
        modelBuilder.Entity<ReadingExerciseResult>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.UserAnswers)
                .IsRequired();
                
            entity.HasOne(e => e.User)
                .WithMany(u => u.ExerciseResults)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.ReadingExercise)
                .WithMany(re => re.Results)
                .HasForeignKey(e => e.ReadingExerciseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure UserProgress entity (One-to-One with User)
        modelBuilder.Entity<UserProgress>(entity =>
        {
            entity.HasKey(e => e.UserId);
            
            entity.Property(e => e.AverageAccuracy)
                .HasColumnType("decimal(5,2)");
                
            entity.Property(e => e.ListeningAccuracy)
                .HasColumnType("decimal(5,2)");
                
            entity.Property(e => e.ReadingAccuracy)
                .HasColumnType("decimal(5,2)");
                
            entity.Property(e => e.LastUpdated)
                .HasDefaultValueSql(GetUtcNowSql());

            entity.HasOne(e => e.User)
                .WithOne(u => u.Progress)
                .HasForeignKey<UserProgress>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Achievement entity
        modelBuilder.Entity<Achievement>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);
                
            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(1000);
                
            entity.Property(e => e.Type)
                .IsRequired()
                .HasMaxLength(50);
                
            entity.Property(e => e.EarnedAt)
                .HasDefaultValueSql(GetUtcNowSql());
                
            entity.HasOne(e => e.User)
                .WithMany(u => u.Achievements)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure StudySession entity
        modelBuilder.Entity<StudySession>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.ActivityType)
                .IsRequired()
                .HasMaxLength(50);
                
            entity.Property(e => e.StartTime)
                .HasDefaultValueSql(GetUtcNowSql());
                
            entity.Property(e => e.AverageScore)
                .HasColumnType("decimal(5,2)");
                
            entity.HasOne(e => e.User)
                .WithMany(u => u.StudySessions)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Seed some initial data
        SeedData(modelBuilder);
    }

    private static string GetUtcNowSql()
    {
        // Return SQL Server GETUTCDATE() by default
        // MySQL UTC_TIMESTAMP() will be handled by the provider
        return "GETUTCDATE()";
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Seed some initial users for testing
        var users = new[]
        {
            new User
            {
                Id = 1,
                Username = "admin",
                Email = "admin@englishmentorbuddy.com",
                FullName = "Administrator",
                Level = "Advanced",
                StudyStreak = 30,
                TotalStudyTime = 1800, // 30 hours
                TotalXP = 5000,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new User
            {
                Id = 2,
                Username = "john_doe",
                Email = "john.doe@example.com",
                FullName = "John Doe",
                Level = "Intermediate",
                StudyStreak = 15,
                TotalStudyTime = 900, // 15 hours
                TotalXP = 2500,
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            },
            new User
            {
                Id = 3,
                Username = "jane_smith",
                Email = "jane.smith@example.com",
                FullName = "Jane Smith",
                Level = "Beginner",
                StudyStreak = 7,
                TotalStudyTime = 420, // 7 hours
                TotalXP = 1200,
                CreatedAt = DateTime.UtcNow.AddDays(-7)
            }
        };

        modelBuilder.Entity<User>().HasData(users);

        // Seed initial progress for users
        var userProgresses = new[]
        {
            new UserProgress
            {
                UserId = 1,
                ListeningScore = 420,
                SpeakingScore = 170,
                ReadingScore = 450,
                WritingScore = 180,
                TotalScore = 870,
                CompletedExercises = 25,
                TotalExercisesAvailable = 50,
                AverageAccuracy = 87.5m,
                ListeningAccuracy = 85.0m,
                ReadingAccuracy = 90.0m,
                CurrentStreak = 30,
                WeeklyGoal = 10,
                MonthlyGoal = 40
            },
            new UserProgress
            {
                UserId = 2,
                ListeningScore = 350,
                SpeakingScore = 140,
                ReadingScore = 380,
                WritingScore = 150,
                TotalScore = 730,
                CompletedExercises = 15,
                TotalExercisesAvailable = 50,
                AverageAccuracy = 75.0m,
                ListeningAccuracy = 72.0m,
                ReadingAccuracy = 78.0m,
                CurrentStreak = 15,
                WeeklyGoal = 8,
                MonthlyGoal = 30
            },
            new UserProgress
            {
                UserId = 3,
                ListeningScore = 280,
                SpeakingScore = 110,
                ReadingScore = 300,
                WritingScore = 120,
                TotalScore = 590,
                CompletedExercises = 8,
                TotalExercisesAvailable = 50,
                AverageAccuracy = 65.0m,
                ListeningAccuracy = 62.0m,
                ReadingAccuracy = 68.0m,
                CurrentStreak = 7,
                WeeklyGoal = 5,
                MonthlyGoal = 20
            }
        };

        modelBuilder.Entity<UserProgress>().HasData(userProgresses);
    }
}