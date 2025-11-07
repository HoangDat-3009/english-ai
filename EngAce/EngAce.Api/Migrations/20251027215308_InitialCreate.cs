using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EngAce.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReadingExercises",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    Level = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Beginner"),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Part 5"),
                    SourceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "manual"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OriginalFileName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EstimatedMinutes = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReadingExercises", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Level = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Beginner"),
                    StudyStreak = table.Column<int>(type: "int", nullable: false),
                    TotalStudyTime = table.Column<int>(type: "int", nullable: false),
                    TotalXP = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastActiveAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Avatar = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReadingQuestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReadingExerciseId = table.Column<int>(type: "int", nullable: false),
                    QuestionText = table.Column<string>(type: "TEXT", nullable: false),
                    OptionA = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    OptionB = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    OptionC = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    OptionD = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CorrectAnswer = table.Column<int>(type: "int", nullable: false),
                    Explanation = table.Column<string>(type: "TEXT", nullable: true),
                    OrderNumber = table.Column<int>(type: "int", nullable: false),
                    Difficulty = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReadingQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReadingQuestions_ReadingExercises_ReadingExerciseId",
                        column: x => x.ReadingExerciseId,
                        principalTable: "ReadingExercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Achievements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Rarity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Points = table.Column<int>(type: "int", nullable: false),
                    EarnedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Criteria = table.Column<string>(type: "TEXT", nullable: true),
                    Metadata = table.Column<string>(type: "TEXT", nullable: true),
                    IsVisible = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Achievements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Achievements_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReadingExerciseResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ReadingExerciseId = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    TotalQuestions = table.Column<int>(type: "int", nullable: false),
                    CorrectAnswers = table.Column<int>(type: "int", nullable: false),
                    UserAnswers = table.Column<string>(type: "TEXT", nullable: false),
                    TimeSpent = table.Column<TimeSpan>(type: "time", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DifficultyRating = table.Column<int>(type: "int", nullable: true),
                    UserFeedback = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReadingExerciseResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReadingExerciseResults_ReadingExercises_ReadingExerciseId",
                        column: x => x.ReadingExerciseId,
                        principalTable: "ReadingExercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReadingExerciseResults_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudySessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    ActivityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ActivityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ExercisesCompleted = table.Column<int>(type: "int", nullable: false),
                    AverageScore = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    XPEarned = table.Column<int>(type: "int", nullable: false),
                    SessionData = table.Column<string>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    DeviceType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Platform = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudySessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudySessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserProgresses",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ListeningScore = table.Column<int>(type: "int", nullable: false),
                    SpeakingScore = table.Column<int>(type: "int", nullable: false),
                    ReadingScore = table.Column<int>(type: "int", nullable: false),
                    WritingScore = table.Column<int>(type: "int", nullable: false),
                    TotalScore = table.Column<int>(type: "int", nullable: false),
                    CompletedExercises = table.Column<int>(type: "int", nullable: false),
                    TotalExercisesAvailable = table.Column<int>(type: "int", nullable: false),
                    AverageAccuracy = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    ListeningAccuracy = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    ReadingAccuracy = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    AverageTimePerExercise = table.Column<TimeSpan>(type: "time", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    WeeklyProgressData = table.Column<string>(type: "TEXT", nullable: true),
                    CurrentStreak = table.Column<int>(type: "int", nullable: false),
                    WeeklyGoal = table.Column<int>(type: "int", nullable: false),
                    MonthlyGoal = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProgresses", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserProgresses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Avatar", "CreatedAt", "Email", "FullName", "IsActive", "LastActiveAt", "Level", "StudyStreak", "TotalStudyTime", "TotalXP", "Username" },
                values: new object[,]
                {
                    { 1, null, new DateTime(2025, 9, 27, 21, 53, 7, 374, DateTimeKind.Utc).AddTicks(8831), "admin@englishmentorbuddy.com", "Administrator", true, new DateTime(2025, 10, 27, 21, 53, 7, 374, DateTimeKind.Utc).AddTicks(8817), "Advanced", 30, 1800, 5000, "admin" },
                    { 2, null, new DateTime(2025, 10, 12, 21, 53, 7, 374, DateTimeKind.Utc).AddTicks(8844), "john.doe@example.com", "John Doe", true, new DateTime(2025, 10, 27, 21, 53, 7, 374, DateTimeKind.Utc).AddTicks(8839), "Intermediate", 15, 900, 2500, "john_doe" },
                    { 3, null, new DateTime(2025, 10, 20, 21, 53, 7, 374, DateTimeKind.Utc).AddTicks(8850), "jane.smith@example.com", "Jane Smith", true, new DateTime(2025, 10, 27, 21, 53, 7, 374, DateTimeKind.Utc).AddTicks(8846), "Beginner", 7, 420, 1200, "jane_smith" }
                });

            migrationBuilder.InsertData(
                table: "UserProgresses",
                columns: new[] { "UserId", "AverageAccuracy", "AverageTimePerExercise", "CompletedExercises", "CurrentStreak", "LastUpdated", "ListeningAccuracy", "ListeningScore", "MonthlyGoal", "ReadingAccuracy", "ReadingScore", "SpeakingScore", "TotalExercisesAvailable", "TotalScore", "WeeklyGoal", "WeeklyProgressData", "WritingScore" },
                values: new object[,]
                {
                    { 1, 87.5m, new TimeSpan(0, 0, 0, 0, 0), 25, 30, new DateTime(2025, 10, 27, 21, 53, 7, 374, DateTimeKind.Utc).AddTicks(9147), 85.0m, 420, 40, 90.0m, 450, 170, 50, 870, 10, null, 180 },
                    { 2, 75.0m, new TimeSpan(0, 0, 0, 0, 0), 15, 15, new DateTime(2025, 10, 27, 21, 53, 7, 374, DateTimeKind.Utc).AddTicks(9161), 72.0m, 350, 30, 78.0m, 380, 140, 50, 730, 8, null, 150 },
                    { 3, 65.0m, new TimeSpan(0, 0, 0, 0, 0), 8, 7, new DateTime(2025, 10, 27, 21, 53, 7, 374, DateTimeKind.Utc).AddTicks(9167), 62.0m, 280, 20, 68.0m, 300, 110, 50, 590, 5, null, 120 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Achievements_UserId",
                table: "Achievements",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReadingExerciseResults_ReadingExerciseId",
                table: "ReadingExerciseResults",
                column: "ReadingExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_ReadingExerciseResults_UserId",
                table: "ReadingExerciseResults",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReadingQuestions_ReadingExerciseId",
                table: "ReadingQuestions",
                column: "ReadingExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_StudySessions_UserId",
                table: "StudySessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Achievements");

            migrationBuilder.DropTable(
                name: "ReadingExerciseResults");

            migrationBuilder.DropTable(
                name: "ReadingQuestions");

            migrationBuilder.DropTable(
                name: "StudySessions");

            migrationBuilder.DropTable(
                name: "UserProgresses");

            migrationBuilder.DropTable(
                name: "ReadingExercises");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
