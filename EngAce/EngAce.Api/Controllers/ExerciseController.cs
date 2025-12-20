using EngAce.Api.DTO.Writing;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using System.Text.Json;

namespace EngAce.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExerciseController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly ILogger<ExerciseController> _logger;

        public ExerciseController(IConfiguration configuration, ILogger<ExerciseController> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new ArgumentNullException("Connection string not found");
            _logger = logger;
        }

        /// <summary>
        /// Save AI-generated exercise to database
        /// </summary>
        [HttpPost("save")]
        public async Task<IActionResult> SaveExercise([FromBody] SaveExerciseRequest request)
        {
            if (request == null || request.Questions == null || request.Questions.Count == 0)
            {
                return BadRequest(new { success = false, message = "Invalid exercise data" });
            }

            try
            {
                _logger.LogInformation("💾 Saving AI-generated exercise: {Title}", request.Title);

                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                // Prepare questions_json - format: [{"q":"...", "options":[...]}]
                var questionsJson = JsonSerializer.Serialize(request.Questions.Select(q => new
                {
                    q = q.Question,
                    options = q.Options
                }).ToList());

                // Prepare correct_answers_json - format: ["A", "B", "C", ...]
                // Convert option index to letter (0->A, 1->B, 2->C, 3->D)
                var correctAnswersJson = JsonSerializer.Serialize(
                    request.Questions.Select(q => 
                    {
                        int index = q.RightOptionIndex;
                        if (index >= 0 && index < 26)
                        {
                            return ((char)('A' + index)).ToString();
                        }
                        return "A"; // Default to A if invalid
                    }).ToList()
                );

                _logger.LogInformation("📝 Questions JSON: {QuestionsJson}", questionsJson);
                _logger.LogInformation("✅ Correct Answers JSON: {CorrectAnswersJson}", correctAnswersJson);

                var sql = @"
                    INSERT INTO exercises (
                        title, 
                        content, 
                        questions_json, 
                        correct_answers_json, 
                        level, 
                        type, 
                        category, 
                        estimated_minutes, 
                        time_limit, 
                        description, 
                        source_type, 
                        created_by, 
                        is_active, 
                        ai_generated,
                        created_at,
                        updated_at
                    ) VALUES (
                        @Title, 
                        @Content, 
                        @QuestionsJson, 
                        @CorrectAnswersJson, 
                        @Level, 
                        @Type, 
                        @Category, 
                        @EstimatedMinutes, 
                        @TimeLimit, 
                        @Description, 
                        @SourceType, 
                        @CreatedBy, 
                        @IsActive, 
                        @AiGenerated,
                        @CreatedAt,
                        @UpdatedAt
                    );
                    SELECT LAST_INSERT_ID();";

                var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@Title", request.Title);
                command.Parameters.AddWithValue("@Content", request.Content ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@QuestionsJson", questionsJson);
                command.Parameters.AddWithValue("@CorrectAnswersJson", correctAnswersJson);
                command.Parameters.AddWithValue("@Level", request.Level ?? "A1");
                command.Parameters.AddWithValue("@Type", request.Type ?? "mixed");
                command.Parameters.AddWithValue("@Category", request.Category ?? request.Topic);
                command.Parameters.AddWithValue("@EstimatedMinutes", request.EstimatedMinutes ?? 10);
                command.Parameters.AddWithValue("@TimeLimit", request.TimeLimit ?? 600);
                command.Parameters.AddWithValue("@Description", request.Description ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@SourceType", "ai_generated");
                command.Parameters.AddWithValue("@CreatedBy", request.CreatedBy ?? 1);
                command.Parameters.AddWithValue("@IsActive", true);
                command.Parameters.AddWithValue("@AiGenerated", true);
                command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);
                command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);

                var exerciseId = Convert.ToInt32(await command.ExecuteScalarAsync());

                _logger.LogInformation("✅ Exercise saved successfully with ID: {ExerciseId}", exerciseId);

                return Ok(new 
                { 
                    success = true, 
                    message = "Exercise saved successfully",
                    exerciseId = exerciseId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error saving exercise");
                return StatusCode(500, new 
                { 
                    success = false, 
                    message = $"Failed to save exercise: {ex.Message}" 
                });
            }
        }

        /// <summary>
        /// Get all exercises with pagination
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetExercises(
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? level = null,
            [FromQuery] string? type = null,
            [FromQuery] bool? aiGenerated = null)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                var whereConditions = new List<string>();
                if (!string.IsNullOrEmpty(level))
                    whereConditions.Add("level = @Level");
                if (!string.IsNullOrEmpty(type))
                    whereConditions.Add("type = @Type");
                if (aiGenerated.HasValue)
                    whereConditions.Add("ai_generated = @AiGenerated");

                var whereClause = whereConditions.Count > 0 
                    ? "WHERE " + string.Join(" AND ", whereConditions) 
                    : "";

                var sql = $@"
                    SELECT 
                        id, title, level, type, category, 
                        estimated_minutes, ai_generated, created_at
                    FROM exercises
                    {whereClause}
                    ORDER BY created_at DESC
                    LIMIT @PageSize OFFSET @Offset";

                var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@PageSize", pageSize);
                command.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);
                
                if (!string.IsNullOrEmpty(level))
                    command.Parameters.AddWithValue("@Level", level);
                if (!string.IsNullOrEmpty(type))
                    command.Parameters.AddWithValue("@Type", type);
                if (aiGenerated.HasValue)
                    command.Parameters.AddWithValue("@AiGenerated", aiGenerated.Value);

                var exercises = new List<object>();
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    exercises.Add(new
                    {
                        id = reader.GetInt32("id"),
                        title = reader.GetString("title"),
                        level = reader.IsDBNull(reader.GetOrdinal("level")) ? null : reader.GetString("level"),
                        type = reader.IsDBNull(reader.GetOrdinal("type")) ? null : reader.GetString("type"),
                        category = reader.IsDBNull(reader.GetOrdinal("category")) ? null : reader.GetString("category"),
                        estimatedMinutes = reader.IsDBNull(reader.GetOrdinal("estimated_minutes")) ? 0 : reader.GetInt32("estimated_minutes"),
                        aiGenerated = reader.GetBoolean("ai_generated"),
                        createdAt = reader.GetDateTime("created_at")
                    });
                }

                return Ok(new { success = true, exercises = exercises });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting exercises");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Save AI-generated sentence writing exercise to database
        /// </summary>
        [HttpPost("save-sentence-writing")]
        public async Task<IActionResult> SaveSentenceWriting([FromBody] SaveSentenceWritingRequest request)
        {
            if (request == null || request.Sentences == null || request.Sentences.Count == 0)
            {
                return BadRequest(new { success = false, message = "Invalid sentence writing data" });
            }

            try
            {
                _logger.LogInformation("💾 Saving AI-generated sentence writing: {Title}", request.Title);

                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                // Prepare questions_json - format for sentence writing: [{"q":"Vietnamese sentence", "options":[]}]
                var questionsJson = JsonSerializer.Serialize(request.Sentences.Select(s => new
                {
                    q = s.Vietnamese,
                    options = new string[] { } // Empty for writing exercises
                }).ToList());

                // Prepare correct_answers_json - format: ["Correct English sentence", ...]
                var correctAnswersJson = JsonSerializer.Serialize(
                    request.Sentences.Select(s => s.CorrectAnswer).ToList()
                );

                _logger.LogInformation("📝 Questions JSON: {QuestionsJson}", questionsJson);
                _logger.LogInformation("✅ Correct Answers JSON: {CorrectAnswersJson}", correctAnswersJson);

                var sql = @"
                    INSERT INTO exercises (
                        title, 
                        content, 
                        questions_json, 
                        correct_answers_json, 
                        level, 
                        type, 
                        category, 
                        estimated_minutes, 
                        time_limit, 
                        description, 
                        source_type, 
                        created_by, 
                        is_active, 
                        ai_generated,
                        created_at,
                        updated_at
                    ) VALUES (
                        @Title, 
                        @Content, 
                        @QuestionsJson, 
                        @CorrectAnswersJson, 
                        @Level, 
                        @Type, 
                        @Category, 
                        @EstimatedMinutes, 
                        @TimeLimit, 
                        @Description, 
                        @SourceType, 
                        @CreatedBy, 
                        @IsActive, 
                        @AiGenerated,
                        @CreatedAt,
                        @UpdatedAt
                    );
                    SELECT LAST_INSERT_ID();";

                var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@Title", request.Title);
                command.Parameters.AddWithValue("@Content", request.Content ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@QuestionsJson", questionsJson);
                command.Parameters.AddWithValue("@CorrectAnswersJson", correctAnswersJson);
                command.Parameters.AddWithValue("@Level", request.Level ?? "Intermediate");
                command.Parameters.AddWithValue("@Type", "sentence_writing");
                command.Parameters.AddWithValue("@Category", request.Category ?? request.Topic);
                command.Parameters.AddWithValue("@EstimatedMinutes", request.EstimatedMinutes ?? 15);
                command.Parameters.AddWithValue("@TimeLimit", request.TimeLimit ?? 900);
                command.Parameters.AddWithValue("@Description", request.Description ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@SourceType", "ai_generated_writing");
                command.Parameters.AddWithValue("@CreatedBy", request.CreatedBy ?? 1);
                command.Parameters.AddWithValue("@IsActive", true);
                command.Parameters.AddWithValue("@AiGenerated", true);
                command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);
                command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);

                var exerciseId = Convert.ToInt32(await command.ExecuteScalarAsync());

                _logger.LogInformation("✅ Sentence writing saved successfully with ID: {ExerciseId}", exerciseId);

                return Ok(new 
                { 
                    success = true, 
                    message = "Sentence writing exercise saved successfully",
                    exerciseId = exerciseId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error saving sentence writing exercise");
                return StatusCode(500, new 
                { 
                    success = false, 
                    message = $"Failed to save exercise: {ex.Message}" 
                });
            }
        }
    }
}
