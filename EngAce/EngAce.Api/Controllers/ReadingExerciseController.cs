using EngAce.Api.DTO;
using EngAce.Api.DTO.Exercises;
using EngAce.Api.DTO.Shared;
using EngAce.Api.DTO.Core;
using EngAce.Api.Services.Interfaces;
using Entities.Models;
using Microsoft.AspNetCore.Mvc;

namespace EngAce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReadingExerciseController : ControllerBase
{
    private readonly IReadingExerciseService _readingExerciseService;
    private readonly ILogger<ReadingExerciseController> _logger;

    public ReadingExerciseController(IReadingExerciseService readingExerciseService, ILogger<ReadingExerciseController> logger)
    {
        _readingExerciseService = readingExerciseService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReadingExerciseDto>>> GetAllExercises([FromQuery] string? level = null)
    {
        try
        {
            var exercises = string.IsNullOrEmpty(level) 
                ? await _readingExerciseService.GetAllExercisesAsync()
                : await _readingExerciseService.GetExercisesByLevelAsync(level);

            return Ok(exercises);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reading exercises");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ReadingExerciseDto>> GetExerciseById(int id)
    {
        try
        {
            var exercise = await _readingExerciseService.GetExerciseByIdAsync(id);
            if (exercise == null)
                return NotFound(new { message = $"Exercise with ID {id} not found" });

            return Ok(exercise);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting exercise with ID {ExerciseId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpPost]
    public async Task<ActionResult<ReadingExerciseDto>> CreateExercise([FromBody] CreateExerciseDto createDto)
    {
        _logger.LogInformation("=== CREATE EXERCISE ENDPOINT CALLED ===");
        _logger.LogInformation("Request data: Name={Name}, Level={Level}, Type={Type}", 
            createDto?.Name, createDto?.Level, createDto?.Type);
            
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model validation failed: {Errors}", 
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return BadRequest(ModelState);
            }
            var exercise = new ReadingExercise
            {
                Name = createDto.Name,
                Content = createDto.Content,
                Level = createDto.Level,
                Type = createDto.Type,
                Description = createDto.Description,
                EstimatedMinutes = createDto.EstimatedMinutes,
                CreatedBy = createDto.CreatedBy ?? "System",
                SourceType = "manual"
            };

            var result = await _readingExerciseService.CreateExerciseAsync(exercise);
            return CreatedAtAction(nameof(GetExerciseById), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating reading exercise");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ReadingExerciseDto>> UpdateExercise(int id, [FromBody] UpdateExerciseDto updateDto)
    {
        try
        {
            var exercise = new ReadingExercise
            {
                Name = updateDto.Name,
                Content = updateDto.Content,
                Level = updateDto.Level,
                Type = updateDto.Type,
                Description = updateDto.Description,
                EstimatedMinutes = updateDto.EstimatedMinutes
            };

            var result = await _readingExerciseService.UpdateExerciseAsync(id, exercise);
            if (result == null)
                return NotFound(new { message = $"Exercise with ID {id} not found" });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating exercise with ID {ExerciseId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteExercise(int id)
    {
        try
        {
            var result = await _readingExerciseService.DeleteExerciseAsync(id);
            if (!result)
                return NotFound(new { message = $"Exercise with ID {id} not found" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting exercise with ID {ExerciseId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    // Submit functionality consolidated to POST /api/ReadingExercise/submit-result

    /// <summary>
    /// Create new passage/reading material (Step 1: Submit passage only)
    /// </summary>
    [HttpPost("create-passage")]
    public async Task<ActionResult<ReadingExerciseDto>> CreatePassage([FromBody] CreatePassageRequest request)
    {
        _logger.LogInformation("=== CREATE PASSAGE === Title: {Title}, Type: {Type}, Level: {Level}", 
            request.Title, request.PartType, request.Level);

        try
        {
            // Validate
            if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Content))
            {
                return BadRequest(new { message = "Title and Content are required" });
            }

            // Create exercise entity (not active yet, waiting for questions)
            var exercise = new ReadingExercise
            {
                Name = request.Title,
                Content = request.Content,
                Type = request.PartType ?? "Part 6",
                Level = request.Level ?? "Intermediate",
                SourceType = "manual",
                CreatedBy = request.CreatedBy ?? "Admin",
                Description = $"{request.PartType} exercise - {request.Level}",
                EstimatedMinutes = request.PartType == "Part 6" ? 20 : 30,
                IsActive = false, // Will be activated when questions are added
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await _readingExerciseService.CreateExerciseAsync(exercise);

            _logger.LogInformation("=== PASSAGE CREATED === Id: {Id}, Title: {Title}, Waiting for questions", 
                result.Id, result.Title);

            return Ok(new 
            {
                exerciseId = result.Id,
                message = "Passage created successfully. Now add questions to activate.",
                exercise = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating passage");
            return StatusCode(500, new { message = $"Error: {ex.Message}" });
        }
    }

    /// <summary>
    /// Add questions to existing passage (Step 2: Submit questions)
    /// </summary>
    [HttpPost("{exerciseId}/add-questions")]
    public async Task<ActionResult<ReadingExerciseDto>> AddQuestions(int exerciseId, [FromBody] AddQuestionsRequest request)
    {
        _logger.LogInformation("=== ADD QUESTIONS === ExerciseId: {ExerciseId}, Count: {Count}", exerciseId, request.Questions?.Count ?? 0);

        try
        {
            // Validate
            if (request.Questions == null || !request.Questions.Any())
            {
                return BadRequest(new { message = "At least one question is required" });
            }

            // Get existing exercise (will need to access DB directly through service)
            var existingExercise = await _readingExerciseService.GetExerciseByIdAsync(exerciseId);
            if (existingExercise == null)
            {
                return NotFound(new { message = "Exercise not found" });
            }

            // Create questions and add them to exercise
            // We'll need a new service method for this
            var questions = request.Questions.Select(q => new ReadingQuestion
            {
                ReadingExerciseId = exerciseId,
                QuestionText = q.QuestionText,
                OptionA = q.OptionA,
                OptionB = q.OptionB,
                OptionC = q.OptionC,
                OptionD = q.OptionD,
                CorrectAnswer = q.CorrectAnswer,
                Explanation = q.Explanation,
                OrderNumber = q.OrderNumber,
                Difficulty = 2 // Medium default
            }).ToList();

            // Add questions via service (will need new method)
            var result = await _readingExerciseService.AddQuestionsToExerciseAsync(exerciseId, questions);

            _logger.LogInformation("=== QUESTIONS ADDED === ExerciseId: {Id}, Total Questions: {Count}, Now Active", exerciseId, questions.Count);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding questions to exercise {ExerciseId}", exerciseId);
            return StatusCode(500, new { message = $"Error: {ex.Message}" });
        }
    }

    /// <summary>
    /// Submit exercise results (alternative endpoint for frontend compatibility)
    /// </summary>
    /// <param name="submitRequest">Exercise submission data</param>
    /// <returns>Exercise with results</returns>
    [HttpPost("submit-result")]
    public async Task<ActionResult<ReadingExerciseDto>> SubmitResult([FromBody] SubmitExerciseResultRequest submitRequest)
    {
        try
        {
            var result = await _readingExerciseService.SubmitExerciseResultAsync(
                submitRequest.ExerciseId, 
                submitRequest.UserId, 
                submitRequest.Answers
            );
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting exercise result for exercise {ExerciseId} and user {UserId}", 
                submitRequest.ExerciseId, submitRequest.UserId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Create a new reading exercise with AI-generated questions
    /// </summary>
    /// <param name="request">Exercise creation request with AI question generation parameters</param>
    /// <returns>Created exercise with AI-generated questions</returns>
    [HttpPost("create-with-ai")]
    public async Task<ActionResult<ReadingExerciseDto>> CreateExerciseWithAI([FromBody] CreateExerciseWithAIRequest request)
    {
        try
        {
            var result = await _readingExerciseService.CreateExerciseWithAIQuestionsAsync(request);
            return CreatedAtAction(nameof(GetExerciseById), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating exercise with AI questions");
            return StatusCode(500, new { message = "Error creating exercise with AI questions", details = ex.Message });
        }
    }

    /// <summary>
    /// Generate additional questions for an existing exercise using AI
    /// </summary>
    /// <param name="id">Exercise ID</param>
    /// <param name="questionCount">Number of questions to generate (default: 3)</param>
    /// <returns>Success status</returns>
    [HttpPost("{id}/generate-questions")]
    public async Task<ActionResult> GenerateAdditionalQuestions(int id, [FromQuery] int questionCount = 3)
    {
        try
        {
            var success = await _readingExerciseService.GenerateAdditionalQuestionsAsync(id, questionCount);
            
            if (success)
            {
                return Ok(new { message = $"Successfully generated {questionCount} additional questions for exercise {id}" });
            }
            
            return BadRequest(new { message = "Failed to generate additional questions" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating additional questions for exercise {Id}", id);
            return StatusCode(500, new { message = "Error generating additional questions", details = ex.Message });
        }
    }

    /// <summary>
    /// Generate new reading exercise with AI-powered questions
    /// Compatible with frontend ReadingExercises page AI generation feature
    /// </summary>
    /// <param name="request">AI generation parameters from frontend</param>
    /// <returns>Generated reading exercise with questions</returns>
    [HttpPost("generate-ai")]
    public async Task<ActionResult<EngAce.Api.DTO.Exercises.GenerateAIResponse>> GenerateWithAI([FromBody] EngAce.Api.DTO.Exercises.GenerateAIRequest request)
    {
        try
        {
            _logger.LogInformation("Generating AI exercise for topic: {Topic}, level: {Level}, type: {Type}", 
                request.Topic, request.Level, request.Type);

            // Call AI service to generate exercise content and questions
            // TODO: Implement AI exercise generation
            return BadRequest(new EngAce.Api.DTO.Exercises.GenerateAIResponse 
            { 
                Success = false, 
                Message = "AI exercise generation not implemented yet",
                ErrorDetails = "This feature is under development"
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid AI generation request for topic: {Topic}", request.Topic);
            return BadRequest(new EngAce.Api.DTO.Exercises.GenerateAIResponse 
            { 
                Success = false, 
                Message = "Invalid request parameters",
                ErrorDetails = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "AI service unavailable for topic: {Topic}", request.Topic);
            return ServiceUnavailable(new EngAce.Api.DTO.Exercises.GenerateAIResponse 
            { 
                Success = false, 
                Message = "AI generation service is currently unavailable",
                ErrorDetails = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error generating AI exercise for topic: {Topic}", request.Topic);
            return StatusCode(500, new EngAce.Api.DTO.Exercises.GenerateAIResponse 
            { 
                Success = false, 
                Message = "Internal server error during AI generation",
                ErrorDetails = ex.Message
            });
        }
    }

    /// <summary>
    /// Get exercises created by admin uploads
    /// Compatible with adminUploadService.getAdminExercises() frontend call
    /// </summary>
    [HttpGet("admin-uploads")]
    public async Task<ActionResult<IEnumerable<ReadingExerciseDto>>> GetAdminUploads()
    {
        try
        {
            // TODO: Implement GetExercisesBySourceTypeAsync
            var adminExercises = await _readingExerciseService.GetAllExercisesAsync();
            return Ok(adminExercises.Where(e => e.SourceType == "uploaded"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting admin uploaded exercises");
            return StatusCode(500, new { message = "Error getting admin uploaded exercises" });
        }
    }

    /// <summary>
    /// Get upload statistics for admin dashboard
    /// Compatible with adminUploadService.getUploadStats() frontend call
    /// </summary>
    [HttpGet("upload-stats")]
    public async Task<ActionResult<UploadStatsDto>> GetUploadStats()
    {
        try
        {
            // TODO: Implement GetUploadStatsAsync
            var stats = new { TotalUploaded = 0, TotalAI = 0, TotalManual = 0 };
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting upload statistics");
            return StatusCode(500, new { message = "Error getting upload statistics" });
        }
    }

    /// <summary>
    /// Clear all reading exercises and questions (for development/testing)
    /// </summary>
    [HttpPost("clear-all")]
    public async Task<ActionResult> ClearAllExercises()
    {
        try
        {
            var result = await _readingExerciseService.ClearAllExercisesAsync();
            return Ok(new { message = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing all exercises");
            return StatusCode(500, new { message = "Error clearing exercises", error = ex.Message });
        }
    }

    /// <summary>
    /// Fix SourceType for old AI-generated exercises (one-time migration)
    /// </summary>
    [HttpPost("fix-ai-sourcetype")]
    public async Task<ActionResult> FixAISourceType()
    {
        try
        {
            var result = await _readingExerciseService.FixAISourceTypeAsync();
            return Ok(new { message = "SourceType fixed successfully", details = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fixing AI sourceType");
            return StatusCode(500, new { message = "Error fixing sourceType", error = ex.Message });
        }
    }

    /// <summary>
    /// Helper method to return ServiceUnavailable status
    /// </summary>
    private ActionResult<EngAce.Api.DTO.Exercises.GenerateAIResponse> ServiceUnavailable(EngAce.Api.DTO.Exercises.GenerateAIResponse response)
    {
        return StatusCode(503, response);
    }
}