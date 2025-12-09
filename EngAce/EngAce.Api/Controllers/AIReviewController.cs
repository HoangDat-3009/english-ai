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
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            var whereConditions = new List<string>
            {
                "ec.is_completed = 1",
                "e.ai_generated = 1"  // Chỉ lấy bài do AI tạo
            };

            // Status filter
            if (!string.IsNullOrEmpty(status) && status != "all")
            {
                whereConditions.Add($"ec.review_status = '{status}'");
            }

            // Confidence filter
            if (!string.IsNullOrEmpty(confidenceFilter) && confidenceFilter != "all")
            {
                if (confidenceFilter == "low")
                    whereConditions.Add("ec.confidence_score < 0.75");
                else if (confidenceFilter == "medium")
                    whereConditions.Add("ec.confidence_score >= 0.75 AND ec.confidence_score < 0.9");
                else if (confidenceFilter == "high")
                    whereConditions.Add("ec.confidence_score >= 0.9");
            }

            // Search filter
            if (!string.IsNullOrEmpty(search))
            {
                whereConditions.Add($"(u.username LIKE '%{search}%' OR e.title LIKE '%{search}%' OR e.category LIKE '%{search}%')");
            }

            var whereClause = string.Join(" AND ", whereConditions);

            var query = $@"
                SELECT 
                    ec.id,
                    ec.user_id as userId,
                    u.username as userName,
                    u.email as userEmail,
                    ec.exercise_id as exerciseId,
                    e.title as exerciseTitle,
                    e.category as exerciseCode,
                    e.level as exerciseLevel,
                    e.type as exerciseType,
                    e.ai_generated as aiGenerated,
                    ec.score as originalScore,
                    ec.final_score as finalScore,
                    ec.score as currentScore,
                    COALESCE(ec.confidence_score, 0.85) as confidenceScore,
                    COALESCE(ec.review_status, 'pending') as reviewStatus,
                    ec.completed_at as completedAt,
                    ec.reviewed_by as reviewedBy,
                    ec.reviewed_at as reviewedAt,
                    ec.review_notes as reviewNotes,
                    COALESCE(ec.total_questions, 10) as totalQuestions,
                    COALESCE(ec.user_answers_json, '{{}}') as userAnswers
                FROM exercise_completions ec
                JOIN users u ON ec.user_id = u.id
                JOIN exercises e ON ec.exercise_id = e.id
                WHERE {whereClause}
                ORDER BY ec.completed_at DESC
                LIMIT 100";

            using var cmd = new MySqlCommand(query, connection);
            using var reader = await cmd.ExecuteReaderAsync();

            var submissions = new List<object>();
            while (await reader.ReadAsync())
            {
                // Calculate confidence score based on actual score if not provided
                var currentScore = reader.GetDecimal("currentScore");
                decimal confidenceScore;
                
                if (!reader.IsDBNull(reader.GetOrdinal("confidenceScore")))
                {
                    confidenceScore = reader.GetDecimal("confidenceScore");
                }
                else
                {
                    // Generate confidence based on score: high score = high confidence
                    if (currentScore >= 90) confidenceScore = 0.92m;
                    else if (currentScore >= 80) confidenceScore = 0.85m;
                    else if (currentScore >= 70) confidenceScore = 0.78m;
                    else if (currentScore >= 60) confidenceScore = 0.68m;
                    else confidenceScore = 0.45m;
                }
                
                submissions.Add(new
                {
                    id = reader.GetInt32("id"),
                    userId = reader.GetInt32("userId"),
                    userName = reader.GetString("userName"),
                    userEmail = reader.GetString("userEmail"),
                    exerciseId = reader.GetInt32("exerciseId"),
                    exerciseTitle = reader.GetString("exerciseTitle"),
                    exerciseCode = reader.IsDBNull(reader.GetOrdinal("exerciseCode")) ? null : reader.GetString("exerciseCode"),
                    exerciseLevel = reader.IsDBNull(reader.GetOrdinal("exerciseLevel")) ? "A1" : reader.GetString("exerciseLevel"),
                    exerciseType = reader.IsDBNull(reader.GetOrdinal("exerciseType")) ? "quiz" : reader.GetString("exerciseType"),
                    aiGenerated = true,
                    originalScore = reader.IsDBNull(reader.GetOrdinal("originalScore")) ? currentScore : reader.GetDecimal("originalScore"),
                    finalScore = reader.IsDBNull(reader.GetOrdinal("finalScore")) ? (decimal?)null : reader.GetDecimal("finalScore"),
                    confidenceScore = confidenceScore,
                    reviewStatus = reader.GetString("reviewStatus"),
                    completedAt = reader.IsDBNull(reader.GetOrdinal("completedAt")) ? DateTime.Now : reader.GetDateTime("completedAt"),
                    reviewedBy = reader.IsDBNull(reader.GetOrdinal("reviewedBy")) ? (int?)null : reader.GetInt32("reviewedBy"),
                    reviewedAt = reader.IsDBNull(reader.GetOrdinal("reviewedAt")) ? (DateTime?)null : reader.GetDateTime("reviewedAt"),
                    reviewNotes = reader.IsDBNull(reader.GetOrdinal("reviewNotes")) ? null : reader.GetString("reviewNotes"),
                    totalQuestions = reader.GetInt32("totalQuestions"),
                    userAnswers = reader.IsDBNull(reader.GetOrdinal("userAnswers")) ? "{}" : reader.GetString("userAnswers")
                });
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
                    ec.id,
                    ec.exercise_id as exerciseId,
                    ec.user_answers_json as userAnswers,
                    ec.score as score,
                    ec.total_questions as totalQuestions,
                    e.questions_json as questionsJson,
                    e.correct_answers_json as correctAnswersJson,
                    e.title as exerciseTitle
                FROM exercise_completions ec
                JOIN exercises e ON ec.exercise_id = e.id
                WHERE ec.id = @id";

            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var details = new
                {
                    id = reader.GetInt32("id"),
                    exerciseId = reader.GetInt32("exerciseId"),
                    exerciseTitle = reader.GetString("exerciseTitle"),
                    userAnswers = reader.IsDBNull(reader.GetOrdinal("userAnswers")) ? "[]" : reader.GetString("userAnswers"),
                    questionsJson = reader.IsDBNull(reader.GetOrdinal("questionsJson")) ? "[]" : reader.GetString("questionsJson"),
                    correctAnswersJson = reader.IsDBNull(reader.GetOrdinal("correctAnswersJson")) ? "[]" : reader.GetString("correctAnswersJson"),
                    score = reader.IsDBNull(reader.GetOrdinal("score")) ? 0 : reader.GetDecimal("score"),
                    totalQuestions = reader.GetInt32("totalQuestions")
                };

                return Ok(details);
            }

            return NotFound(new { message = "Submission not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting submission details");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }
}
