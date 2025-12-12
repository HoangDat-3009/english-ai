using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using System.Text.Json;

namespace EngAce.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WritingExerciseController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<WritingExerciseController> _logger;

        public WritingExerciseController(IConfiguration configuration, ILogger<WritingExerciseController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Get all writing exercises by type (writing_essay or writing_sentence)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetWritingExercises([FromQuery] string? type = null)
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("LearningSystemDb");
                var exercises = new List<object>();

                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    var query = @"
                        SELECT id, title, content, questions_json, correct_answers_json, 
                               level, type, category, estimated_minutes, time_limit, 
                               description, is_active, created_at
                        FROM exercises
                        WHERE (type = 'writing_essay' OR type = 'writing_sentence')";

                    if (!string.IsNullOrEmpty(type))
                    {
                        query += " AND type = @type";
                    }

                    query += " ORDER BY created_at DESC";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        if (!string.IsNullOrEmpty(type))
                        {
                            command.Parameters.AddWithValue("@type", type);
                        }

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                exercises.Add(new
                                {
                                    id = reader.GetInt32("id"),
                                    title = reader.GetString("title"),
                                    content = reader.IsDBNull(reader.GetOrdinal("content")) ? null : reader.GetString("content"),
                                    questionsJson = reader.GetString("questions_json"),
                                    correctAnswersJson = reader.GetString("correct_answers_json"),
                                    level = reader.IsDBNull(reader.GetOrdinal("level")) ? null : reader.GetString("level"),
                                    type = reader.GetString("type"),
                                    category = reader.IsDBNull(reader.GetOrdinal("category")) ? null : reader.GetString("category"),
                                    estimatedMinutes = reader.IsDBNull(reader.GetOrdinal("estimated_minutes")) ? (int?)null : reader.GetInt32("estimated_minutes"),
                                    timeLimit = reader.IsDBNull(reader.GetOrdinal("time_limit")) ? (int?)null : reader.GetInt32("time_limit"),
                                    description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString("description"),
                                    isActive = reader.GetBoolean("is_active"),
                                    createdAt = reader.GetDateTime("created_at")
                                });
                            }
                        }
                    }
                }

                return Ok(new { success = true, exercises });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting writing exercises");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get a single writing exercise by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetWritingExerciseById(int id)
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("LearningSystemDb");

                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    var query = @"
                        SELECT id, title, content, questions_json, correct_answers_json, 
                               level, type, category, estimated_minutes, time_limit, 
                               description, is_active, created_at
                        FROM exercises
                        WHERE id = @id AND (type = 'writing_essay' OR type = 'writing_sentence')";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var exercise = new
                                {
                                    id = reader.GetInt32("id"),
                                    title = reader.GetString("title"),
                                    content = reader.IsDBNull(reader.GetOrdinal("content")) ? null : reader.GetString("content"),
                                    questionsJson = reader.GetString("questions_json"),
                                    correctAnswersJson = reader.GetString("correct_answers_json"),
                                    level = reader.IsDBNull(reader.GetOrdinal("level")) ? null : reader.GetString("level"),
                                    type = reader.GetString("type"),
                                    category = reader.IsDBNull(reader.GetOrdinal("category")) ? null : reader.GetString("category"),
                                    estimatedMinutes = reader.IsDBNull(reader.GetOrdinal("estimated_minutes")) ? (int?)null : reader.GetInt32("estimated_minutes"),
                                    timeLimit = reader.IsDBNull(reader.GetOrdinal("time_limit")) ? (int?)null : reader.GetInt32("time_limit"),
                                    description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString("description"),
                                    isActive = reader.GetBoolean("is_active"),
                                    createdAt = reader.GetDateTime("created_at")
                                };

                                return Ok(new { success = true, exercise });
                            }
                            else
                            {
                                return NotFound(new { success = false, message = "Exercise not found" });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting writing exercise by ID");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Create a new writing exercise
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateWritingExercise([FromBody] CreateWritingExerciseRequest request)
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("LearningSystemDb");

                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    var query = @"
                        INSERT INTO exercises 
                        (title, content, questions_json, correct_answers_json, level, type, 
                         category, estimated_minutes, time_limit, description, created_by, is_active)
                        VALUES 
                        (@title, @content, @questionsJson, @correctAnswersJson, @level, @type, 
                         @category, @estimatedMinutes, @timeLimit, @description, @createdBy, 1)";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@title", request.Title);
                        command.Parameters.AddWithValue("@content", request.Content ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@questionsJson", request.QuestionsJson);
                        command.Parameters.AddWithValue("@correctAnswersJson", request.CorrectAnswersJson);
                        command.Parameters.AddWithValue("@level", request.Level ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@type", request.Type);
                        command.Parameters.AddWithValue("@category", request.Category ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@estimatedMinutes", request.EstimatedMinutes ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@timeLimit", request.TimeLimit ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@description", request.Description ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@createdBy", request.CreatedBy ?? (object)DBNull.Value);

                        await command.ExecuteNonQueryAsync();
                        var newId = command.LastInsertedId;

                        return Ok(new { success = true, message = "Exercise created successfully", id = newId });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating writing exercise");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing writing exercise
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateWritingExercise(int id, [FromBody] CreateWritingExerciseRequest request)
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("LearningSystemDb");

                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    var query = @"
                        UPDATE exercises 
                        SET title = @title, 
                            content = @content, 
                            questions_json = @questionsJson, 
                            correct_answers_json = @correctAnswersJson, 
                            level = @level, 
                            type = @type, 
                            category = @category, 
                            estimated_minutes = @estimatedMinutes, 
                            time_limit = @timeLimit, 
                            description = @description,
                            updated_at = NOW()
                        WHERE id = @id AND (type = 'writing_essay' OR type = 'writing_sentence')";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        command.Parameters.AddWithValue("@title", request.Title);
                        command.Parameters.AddWithValue("@content", request.Content ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@questionsJson", request.QuestionsJson);
                        command.Parameters.AddWithValue("@correctAnswersJson", request.CorrectAnswersJson);
                        command.Parameters.AddWithValue("@level", request.Level ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@type", request.Type);
                        command.Parameters.AddWithValue("@category", request.Category ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@estimatedMinutes", request.EstimatedMinutes ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@timeLimit", request.TimeLimit ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@description", request.Description ?? (object)DBNull.Value);

                        var rowsAffected = await command.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            return Ok(new { success = true, message = "Exercise updated successfully" });
                        }
                        else
                        {
                            return NotFound(new { success = false, message = "Exercise not found" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating writing exercise");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Delete a writing exercise
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWritingExercise(int id)
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("LearningSystemDb");

                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    var query = @"
                        DELETE FROM exercises 
                        WHERE id = @id AND (type = 'writing_essay' OR type = 'writing_sentence')";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);

                        var rowsAffected = await command.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            return Ok(new { success = true, message = "Exercise deleted successfully" });
                        }
                        else
                        {
                            return NotFound(new { success = false, message = "Exercise not found" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting writing exercise");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }

    public class CreateWritingExerciseRequest
    {
        public string Title { get; set; } = string.Empty;
        public string? Content { get; set; }
        public string QuestionsJson { get; set; } = "[]";
        public string CorrectAnswersJson { get; set; } = "[]";
        public string? Level { get; set; }
        public string Type { get; set; } = "writing_essay"; // writing_essay or writing_sentence
        public string? Category { get; set; }
        public int? EstimatedMinutes { get; set; }
        public int? TimeLimit { get; set; }
        public string? Description { get; set; }
        public int? CreatedBy { get; set; }
    }
}
