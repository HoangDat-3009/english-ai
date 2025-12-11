using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Data;

namespace EngAce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AIReviewController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AIReviewController> _logger;

    public AIReviewController(IConfiguration configuration, ILogger<AIReviewController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    // GET: api/AIReview/test
    [HttpGet("test")]
    public IActionResult Test()
    {
        return Ok(new { message = "API is working!" });
    }

    // GET: api/AIReview/stats
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        try
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            var query = @"
                SELECT 
                    COUNT(CASE WHEN ec.review_status = 'pending' THEN 1 END) as totalPending,
                    COUNT(CASE WHEN ec.review_status = 'approved' THEN 1 END) as totalApproved,
                    COUNT(CASE WHEN ec.review_status = 'rejected' THEN 1 END) as totalRejected,
                    COUNT(CASE WHEN COALESCE(ec.confidence_score, 0) < 0.70 THEN 1 END) as lowConfidence,
                    AVG(COALESCE(ec.confidence_score, 0.75)) as avgConfidence,
                    COUNT(CASE WHEN ec.review_status = 'pending' AND COALESCE(ec.confidence_score, 0) < 0.70 THEN 1 END) as needsAttention
                FROM exercise_completions ec
                JOIN exercises e ON ec.exercise_id = e.id
                WHERE ec.is_completed = 1
                  AND e.type IN ('vocabulary', 'grammar', 'reading', 'listening', 'multiple_choice')";

            using var cmd = new MySqlCommand(query, connection);
            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var stats = new
                {
                    totalPending = reader.GetInt32("totalPending"),
                    totalApproved = reader.GetInt32("totalApproved"),
                    totalRejected = reader.GetInt32("totalRejected"),
                    lowConfidence = reader.GetInt32("lowConfidence"),
                    avgConfidence = reader.IsDBNull("avgConfidence") ? 0.0 : reader.GetDouble("avgConfidence"),
                    needsAttention = reader.GetInt32("needsAttention")
                };

                return Ok(stats);
            }

            return Ok(new
            {
                totalPending = 0,
                totalApproved = 0,
                totalRejected = 0,
                lowConfidence = 0,
                avgConfidence = 0.0,
                needsAttention = 0
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting AI review stats");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    // GET: api/AIReview/submissions
    [HttpGet("submissions")]
    public async Task<IActionResult> GetSubmissions(
        [FromQuery] string? status = null,
        [FromQuery] string? confidenceFilter = null,
        [FromQuery] string? search = null)
    {
        try
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                _logger.LogError("❌ Connection string is null or empty");
                return StatusCode(500, new { message = "Database connection not configured" });
            }

            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            var whereConditions = new List<string>
            {
                "e.ai_generated = 1"  // Only show AI-generated exercises
            };

            // Status filter - removed (no longer using exercise_completions)

            // Search filter
            if (!string.IsNullOrEmpty(search))
            {
                var safeSearch = search.Replace("'", "''"); // Prevent SQL injection
                whereConditions.Add($"(e.title LIKE '%{safeSearch}%' OR e.category LIKE '%{safeSearch}%' OR e.level LIKE '%{safeSearch}%')");
            }

            var whereClause = string.Join(" AND ", whereConditions);

            var query = $@"
                SELECT 
                    e.id as exerciseId,
                    e.title as exerciseTitle,
                    COALESCE(e.category, 'general') as exerciseCode,
                    COALESCE(e.level, 'A1') as exerciseLevel,
                    COALESCE(e.type, 'quiz') as exerciseType,
                    COALESCE(e.ai_generated, 0) as aiGenerated,
                    e.created_by as createdBy,
                    e.created_at as createdAt,
                    e.source_type as sourceType,
                    e.questions_json as questionsJson
                FROM exercises e
                WHERE {whereClause}
                ORDER BY e.created_at DESC
                LIMIT 100";

            using var cmd = new MySqlCommand(query, connection);
            using var reader = await cmd.ExecuteReaderAsync();

            var submissions = new List<object>();
            while (await reader.ReadAsync())
            {
                try
                {
                    var questionsJson = reader.IsDBNull(reader.GetOrdinal("questionsJson")) 
                        ? "[]" 
                        : reader.GetString("questionsJson");
                    
                    // Count questions from JSON
                    int totalQuestions = 0;
                    try
                    {
                        var questions = System.Text.Json.JsonSerializer.Deserialize<List<object>>(questionsJson);
                        totalQuestions = questions?.Count ?? 0;
                    }
                    catch
                    {
                        totalQuestions = 0;
                    }
                    
                    submissions.Add(new
                    {
                        id = reader.GetInt32("exerciseId"),
                        exerciseId = reader.GetInt32("exerciseId"),
                        exerciseTitle = reader.GetString("exerciseTitle"),
                        exerciseCode = reader.GetString("exerciseCode"),
                        exerciseLevel = reader.GetString("exerciseLevel"),
                        exerciseType = reader.GetString("exerciseType"),
                        aiGenerated = reader.GetBoolean("aiGenerated"),
                        createdBy = reader.IsDBNull(reader.GetOrdinal("createdBy")) ? (int?)null : reader.GetInt32("createdBy"),
                        createdAt = reader.GetDateTime("createdAt"),
                        sourceType = reader.IsDBNull(reader.GetOrdinal("sourceType")) ? null : reader.GetString("sourceType"),
                        totalQuestions = totalQuestions,
                        reviewStatus = "pending" // Default status for AI-generated exercises
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "⚠️ Error reading submission row, skipping...");
                    continue;
                }
            }

            return Ok(submissions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting AI review submissions");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    // GET: api/AIReview/submissions/{id}/details
    [HttpGet("submissions/{id}/details")]
    public async Task<IActionResult> GetSubmissionDetails(int id)
    {
        try
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            var query = @"
                SELECT 
                    e.id as exerciseId,
                    e.title as exerciseTitle,
                    e.questions_json as questionsJson,
                    e.correct_answers_json as correctAnswersJson,
                    e.level,
                    e.type,
                    e.category
                FROM exercises e
                WHERE e.id = @id AND e.ai_generated = 1";

            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var details = new
                {
                    id = id,
                    exerciseId = reader.GetInt32("exerciseId"),
                    exerciseTitle = reader.GetString("exerciseTitle"),
                    questionsJson = reader.IsDBNull(reader.GetOrdinal("questionsJson")) ? "[]" : reader.GetString("questionsJson"),
                    correctAnswersJson = reader.IsDBNull(reader.GetOrdinal("correctAnswersJson")) ? "[]" : reader.GetString("correctAnswersJson"),
                    level = reader.IsDBNull(reader.GetOrdinal("level")) ? "A1" : reader.GetString("level"),
                    type = reader.IsDBNull(reader.GetOrdinal("type")) ? "quiz" : reader.GetString("type"),
                    category = reader.IsDBNull(reader.GetOrdinal("category")) ? "general" : reader.GetString("category")
                };

                return Ok(details);
            }

            return NotFound(new { message = "Exercise not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting exercise details");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    // PUT: api/AIReview/submissions/{id}/review
    [HttpPut("submissions/{id}/review")]
    public async Task<IActionResult> UpdateReview(int id, [FromBody] ReviewUpdateRequest request)
    {
        if (request == null)
            return BadRequest(new { message = "Invalid request data" });

        try
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            // Cập nhật exercise_completions với điểm mới và trạng thái
            var updateCompletionQuery = @"
                UPDATE exercise_completions 
                SET 
                    final_score = @finalScore,
                    review_status = @reviewStatus,
                    review_notes = @reviewNotes,
                    reviewed_by = @reviewedBy,
                    reviewed_at = NOW()
                WHERE id = @id";

            using var cmd = new MySqlCommand(updateCompletionQuery, connection);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@finalScore", request.FinalScore);
            cmd.Parameters.AddWithValue("@reviewStatus", request.ReviewStatus);
            cmd.Parameters.AddWithValue("@reviewNotes", request.ReviewNotes ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@reviewedBy", request.ReviewedBy.HasValue ? request.ReviewedBy.Value : (object)DBNull.Value);
            
            var rowsAffected = await cmd.ExecuteNonQueryAsync();
            if (rowsAffected == 0)
            {
                return NotFound(new { message = "Submission not found" });
            }

            return Ok(new
            {
                message = "Review updated successfully",
                submissionId = id,
                finalScore = request.FinalScore,
                reviewStatus = request.ReviewStatus
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating review for submission {SubmissionId}", id);
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    // DTO classes
    public class ReviewUpdateRequest
    {
        public int SubmissionId { get; set; }
        public decimal FinalScore { get; set; }
        public string ReviewStatus { get; set; } = "pending";
        public string? ReviewNotes { get; set; }
        public int? ReviewedBy { get; set; }
        public List<QuestionAdjustment>? QuestionAdjustments { get; set; }
    }

    public class QuestionAdjustment
    {
        public int QuestionNumber { get; set; }
        public string? NewCorrectAnswer { get; set; }
        public string? TeacherExplanation { get; set; }
        public decimal NewPoints { get; set; }
        public bool IsCorrect { get; set; }
    }
}