using Entities.Data;
using Entities.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using EngAce.Api.Services.Interfaces;
using EngAce.Api.Services.AI;

namespace EngAce.Api.Controllers;

/// <summary>
/// Controller for managing reading exercises with full admin functionality
/// Supports AI generation, manual creation, and file upload
/// Uses new Exercise entity with simplified schema
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ReadingExerciseController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ReadingExerciseController> _logger;
    private readonly IGeminiService _geminiService;

    public ReadingExerciseController(ApplicationDbContext context, ILogger<ReadingExerciseController> logger, IGeminiService geminiService)
    {
        _context = context;
        _logger = logger;
        _geminiService = geminiService;
    }

    /// <summary>
    /// Get all exercises with optional level filtering
    /// Only returns Part 5, Part 6, and Part 7 exercises
    /// </summary>
        [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetAllExercises([FromQuery] string? level = null)
    {
        try
        {
            // Filter for active exercises and only Part 5, 6, 7
            IQueryable<Exercise> query = _context.Exercises
                .Where(e => e.IsActive && 
                       (e.Type == "Part 5" || e.Type == "Part 6" || e.Type == "Part 7"));

            if (!string.IsNullOrEmpty(level))
                query = query.Where(e => e.Level == level);

            query = query.OrderByDescending(e => e.CreatedAt);

            // Get raw data from database
            var rawExercises = await query.ToListAsync();

            // Then, parse JSON in memory
            var exercises = rawExercises.Select(e => new
            {
                ExerciseId = e.ExerciseId,
                Id = e.ExerciseId,
                Title = e.Title,
                Name = e.Title,
                Content = e.Content,
                Level = e.Level,
                Type = e.Type,
                Category = e.Category,
                EstimatedMinutes = e.EstimatedMinutes,
                Duration = e.EstimatedMinutes,
                SourceType = e.SourceType,
                CreatedByUserId = e.CreatedByUserId,
                CreatedBy = e.CreatedByDisplay,
                Description = e.Description,
                CreatedAt = e.CreatedAt,
                DateCreated = e.CreatedAt,
                Questions = ParseQuestionsJson(e.Questions)
            }).ToList();

            return Ok(exercises);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reading exercises");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get exercise by ID with full details
    /// </summary>
        [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetExerciseById(int id)
    {
        try
        {
            // First get raw exercise from database
            var rawExercise = await _context.Exercises
                .FirstOrDefaultAsync(e => e.ExerciseId == id && e.IsActive);

            if (rawExercise == null)
                return NotFound(new { message = $"Exercise with ID {id} not found" });

            // Parse JSON questions in memory
            var exercise = new
            {
                ExerciseId = rawExercise.ExerciseId,
                Id = rawExercise.ExerciseId, // Alias for frontend compatibility
                Title = rawExercise.Title,
                Name = rawExercise.Title, // Alias for frontend compatibility
                Content = rawExercise.Content,
                Level = rawExercise.Level,
                Type = rawExercise.Type,
                Category = rawExercise.Category,
                EstimatedMinutes = rawExercise.EstimatedMinutes,
                Duration = rawExercise.EstimatedMinutes, // Alias for frontend compatibility
                SourceType = rawExercise.SourceType,
                CreatedByUserId = rawExercise.CreatedByUserId,
                CreatedBy = rawExercise.CreatedByDisplay,
                Description = rawExercise.Description,
                CreatedAt = rawExercise.CreatedAt,
                DateCreated = rawExercise.CreatedAt, // Alias for frontend compatibility
                Questions = ParseQuestionsJson(rawExercise.Questions) // Parse JSON questions in memory (static method)
            };

            return Ok(exercise);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting exercise with ID {ExerciseId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Create passage for exercise (Step 1 of 2-step process)
    /// Frontend calls this first, then adds questions
    /// </summary>
    [HttpPost("create-passage")]
    public async Task<ActionResult<object>> CreatePassage([FromBody] CreatePassageRequest request)
    {
        _logger.LogInformation("=== CREATE PASSAGE ENDPOINT CALLED ===");
        _logger.LogInformation("Request data: Title={Title}, PartType={PartType}, Level={Level}", 
            request?.Title, request?.PartType, request?.Level);
            
        try
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return BadRequest(new { message = "Title is required" });
            }

            if (string.IsNullOrWhiteSpace(request.Content))
            {
                return BadRequest(new { message = "Content is required" });
            }

            var exercise = new Exercise
            {
                Title = request.Title,
                Content = request.Content,
                Level = request.Level ?? "Intermediate",
                Type = request.PartType ?? "Part 6",
                Category = "General",
                Description = $"Reading exercise: {request.Title}",
                EstimatedMinutes = request.PartType == "Part 6" ? 20 : 30,
                SourceType = "manual",
                Questions = "[]", // Empty questions initially - will be added in step 2
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = false // Not active until questions are added
            };
            exercise.CreatedByUserId = await ResolveCreatedByUserIdAsync(request.CreatedBy);

            _context.Exercises.Add(exercise);
            await _context.SaveChangesAsync();

            var result = new
            {
                exerciseId = exercise.ExerciseId,
                id = exercise.ExerciseId,
                title = exercise.Title,
                name = exercise.Title,
                content = exercise.Content,
                level = exercise.Level,
                type = exercise.Type,
                createdByUserId = exercise.CreatedByUserId,
                createdBy = exercise.CreatedByDisplay,
                createdAt = exercise.CreatedAt,
                isActive = exercise.IsActive,
                message = "Passage created successfully. Please add questions to activate the exercise."
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating passage");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>
    /// Create new exercise manually (Admin function)
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<object>> CreateExercise([FromBody] CreateExerciseRequest request)
    {
        _logger.LogInformation("=== CREATE EXERCISE ENDPOINT CALLED ===");
        _logger.LogInformation("Request data: Title={Title}, Level={Level}, Type={Type}", 
            request?.Title, request?.Level, request?.Type);
            
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model validation failed: {Errors}", 
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return BadRequest(ModelState);
            }

            var exercise = new Exercise
            {
                Title = request.Title,
                Content = request.Content,
                Level = request.Level,
                Type = request.Type,
                Category = request.Category ?? "General",
                Description = request.Description,
                EstimatedMinutes = request.EstimatedMinutes ?? 30,
                SourceType = "manual",
                Questions = "[]", // Empty questions initially
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };
            exercise.CreatedByUserId = await ResolveCreatedByUserIdAsync(request.CreatedBy);

            _context.Exercises.Add(exercise);
            await _context.SaveChangesAsync();

            var result = new
            {
                exercise.ExerciseId,
                Id = exercise.ExerciseId,
                exercise.Title,
                Name = exercise.Title,
                exercise.Content,
                exercise.Level,
                exercise.Type,
                exercise.Category,
                exercise.EstimatedMinutes,
                exercise.SourceType,
                CreatedByUserId = exercise.CreatedByUserId,
                CreatedBy = exercise.CreatedByDisplay,
                exercise.Description,
                exercise.CreatedAt,
                Questions = new List<object>()
            };

            return CreatedAtAction(nameof(GetExerciseById), new { id = exercise.ExerciseId }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating reading exercise");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Add questions to existing exercise (Admin function)
    /// </summary>
    [HttpPost("{exerciseId}/add-questions")]
    public async Task<ActionResult<object>> AddQuestions(int exerciseId, [FromBody] AddQuestionsRequest request)
    {
        _logger.LogInformation("=== ADD QUESTIONS === ExerciseId: {ExerciseId}, Count: {Count}", 
            exerciseId, request.Questions?.Count ?? 0);

        try
        {
            if (request.Questions == null || !request.Questions.Any())
            {
                return BadRequest(new { message = "At least one question is required" });
            }

            var exercise = await _context.Exercises.FindAsync(exerciseId);
            if (exercise == null)
            {
                return NotFound(new { message = "Exercise not found" });
            }

            // Convert questions to JSON format
            var questionsJson = JsonSerializer.Serialize(request.Questions.Select((q, index) => new
            {
                questionText = q.QuestionText,
                options = new[] { q.OptionA, q.OptionB, q.OptionC, q.OptionD },
                correctAnswer = q.CorrectAnswer,
                explanation = q.Explanation,
                orderNumber = index + 1
            }));

            exercise.Questions = questionsJson;
            exercise.CorrectAnswers = SerializeCorrectAnswersFromRequest(request.Questions);
            exercise.UpdatedAt = DateTime.UtcNow;
            exercise.IsActive = true; // Activate now that it has questions

            await _context.SaveChangesAsync();

            var result = new
            {
                exercise.ExerciseId,
                Id = exercise.ExerciseId,
                exercise.Title,
                Name = exercise.Title,
                exercise.Content,
                exercise.Level,
                exercise.Type,
                exercise.Category,
                exercise.EstimatedMinutes,
                exercise.SourceType,
                CreatedByUserId = exercise.CreatedByUserId,
                CreatedBy = exercise.CreatedByDisplay,
                exercise.Description,
                exercise.CreatedAt,
                Questions = ParseQuestionsJson(exercise.Questions)
            };

            _logger.LogInformation("=== QUESTIONS ADDED === ExerciseId: {Id}, Total Questions: {Count}, Now Active", 
                exerciseId, request.Questions.Count);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding questions to exercise {ExerciseId}", exerciseId);
            return StatusCode(500, new { message = $"Error: {ex.Message}" });
        }
    }

    /// <summary>
    /// Submit exercise results
    /// </summary>
    [HttpPost("submit-result")]
    public async Task<ActionResult<object>> SubmitResult([FromBody] SubmitResultRequest request)
    {
        try
        {
            var exercise = await _context.Exercises
                .FirstOrDefaultAsync(e => e.ExerciseId == request.ExerciseId);

            if (exercise == null)
                throw new ArgumentException($"Exercise with ID {request.ExerciseId} not found");

            var questions = ParseQuestionsJson(exercise.Questions);
            int correctAnswers = 0;

            // Calculate score
            for (int i = 0; i < Math.Min(request.Answers.Count, questions.Count); i++)
            {
                var questionCorrectAnswer = questions[i].correctAnswer;
                if (request.Answers[i] == questionCorrectAnswer)
                    correctAnswers++;
            }

            var score = questions.Count > 0 ? (decimal)Math.Round((double)correctAnswers / questions.Count * 100, 2) : 0;

            // Get max attempt number for this user and exercise
            // Load to client first because DefaultIfEmpty with MaxAsync cannot be translated to SQL
            var existingAttempts = await _context.Completions
                .Where(c => c.UserId == request.UserId && c.ExerciseId == request.ExerciseId)
                .Select(c => c.Attempts)
                .ToListAsync();
            
            var maxAttempt = existingAttempts.Any() ? existingAttempts.Max() : 0;
            var attemptNumber = maxAttempt + 1;

            // Save completion result
            var completion = new Completion
            {
                UserId = request.UserId,
                ExerciseId = request.ExerciseId,
                Score = score,
                TotalQuestions = questions.Count,
                UserAnswers = JsonSerializer.Serialize(request.Answers),
                StartedAt = DateTime.UtcNow.AddMinutes(-(exercise.EstimatedMinutes ?? 30)),
                CompletedAt = DateTime.UtcNow,
                IsCompleted = true,
                Attempts = attemptNumber,
                TimeSpentMinutes = exercise.EstimatedMinutes ?? 30
            };

            _context.Completions.Add(completion);
            
            // Update user stats
            var user = await _context.Users.FindAsync(request.UserId);
            if (user != null)
            {
                user.TotalXp += (int)Math.Round(score);
                user.LastActiveAt = DateTime.UtcNow;
            }
            
            await _context.SaveChangesAsync();

            var result = new
            {
                exercise.ExerciseId,
                Id = exercise.ExerciseId,
                exercise.Title,
                Name = exercise.Title,
                exercise.Content,
                exercise.Level,
                exercise.Type,
                Questions = questions,
                UserResult = new
                {
                    Score = score,
                    CorrectAnswers = correctAnswers,
                    TotalQuestions = questions.Count,
                    CompletedAt = DateTime.UtcNow
                }
            };

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting exercise result for exercise {ExerciseId} and user {UserId}", 
                request.ExerciseId, request.UserId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// 🤖 Generate exercise with AI (Admin function)
    /// Flow: Nhận request từ frontend -> Gọi GeminiService -> Parse response -> Lưu DB -> Return exercise
    /// Input: {topic, level, type} -> AI tạo questions + passage (Part 6/7) -> JSON lưu DB
    /// </summary>
    [HttpPost("generate-ai")]
    public async Task<ActionResult<object>> GenerateWithAI([FromBody] GenerateAIRequest request)
    {
        try
        {
            _logger.LogInformation("Generating AI exercise for topic: {Topic}, level: {Level}, type: {Type}", 
                request.Topic, request.Level, request.Type);

            // Generate content using Gemini AI
            var baseContent = GenerateBaseContent(request.Topic, request.Type, request.Level);
            
            // Get question count based on type
            var questionCount = GetQuestionCountByType(request.Type);
            
            // Generate questions using Gemini AI (with passage for Part 6/7)
            List<GeneratedQuestion> aiQuestions;
            string finalContent;
            
            // Determine provider (default to gemini if not specified)
            var provider = string.IsNullOrWhiteSpace(request.Provider) ? "gemini" : request.Provider.ToLower();
            if (provider != "gemini" && provider != "openai")
            {
                provider = "gemini"; // Fallback to gemini if invalid provider
            }
            
            _logger.LogInformation("Using AI provider: {Provider}", provider);
            
            if (request.Type == "Part 6" || request.Type == "Part 7")
            {
                // Use the combined method for Part 6/7 to get both questions and passage
                var (questions, passage) = await _geminiService.GenerateQuestionsWithPassageAsync(
                    baseContent, 
                    request.Type, 
                    request.Level, 
                    questionCount,
                    provider
                );
                
                aiQuestions = questions;
                finalContent = passage;
                
                _logger.LogInformation("AI Questions and passage generated using {Provider}: {Count} questions, passage length: {PassageLength} for type {Type}", 
                    provider, aiQuestions?.Count ?? 0, finalContent?.Length ?? 0, request.Type);
            }
            else
            {
                // For Part 5, just generate questions
                aiQuestions = await _geminiService.GenerateQuestionsAsync(
                    baseContent, 
                    request.Type, 
                    request.Level, 
                    questionCount,
                    provider
                );
                finalContent = baseContent;
                
                _logger.LogInformation("AI Questions generated using {Provider}: {Count} questions for type {Type}", 
                    provider, aiQuestions?.Count ?? 0, request.Type);
            }

            // If AI generation fails, fall back to sample questions
            string questionsJson;
            
            if (aiQuestions.Any())
            {
                // Use AI-generated questions - ensure consistent format
                var formattedQuestions = aiQuestions.Select((q, index) => new
                {
                    id = index + 1,
                    questionText = q.QuestionText ?? $"Question {index + 1}",
                    question = q.QuestionText ?? $"Question {index + 1}",
                    options = q.Options ?? new List<string> { "Option A", "Option B", "Option C", "Option D" },
                    correctAnswer = q.CorrectAnswer >= 0 && q.CorrectAnswer < (q.Options?.Count ?? 4) 
                        ? q.CorrectAnswer 
                        : 0,
                    explanation = q.Explanation ?? "No explanation available",
                    difficulty = q.Difficulty > 0 && q.Difficulty <= 5 ? q.Difficulty : 3,
                    orderNumber = index + 1
                }).ToList();
                
                questionsJson = JsonSerializer.Serialize(formattedQuestions, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                });

                _logger.LogInformation("Successfully generated {Count} questions using Gemini AI", aiQuestions.Count);
            }
            else
            {
                // Fallback to sample questions
                questionsJson = GenerateSampleQuestions(request.Topic, request.Type, request.Level);
                finalContent = baseContent;
                
                _logger.LogWarning("AI generation failed, using fallback sample questions");
            }

            var exercise = new Exercise
            {
                Title = $"AI Generated: {request.Topic}",
                Content = finalContent,
                Level = request.Level,
                Type = request.Type,
                Category = "AI Generated",
                Description = $"AI generated exercise on topic: {request.Topic}",
                EstimatedMinutes = request.Type == "Part 6" ? 20 : 30,
                SourceType = "ai",
                Questions = questionsJson,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };
            exercise.CreatedByUserId = await ResolveCreatedByUserIdAsync(request.CreatedBy);

            _context.Exercises.Add(exercise);
            await _context.SaveChangesAsync();

            var result = new
            {
                Success = true,
                Message = "AI exercise generated successfully",
                Exercise = new
                {
                    exercise.ExerciseId,
                    Id = exercise.ExerciseId,
                    exercise.Title,
                    Name = exercise.Title,
                    exercise.Content,
                    exercise.Level,
                    exercise.Type,
                    exercise.Category,
                    exercise.EstimatedMinutes,
                    exercise.SourceType,
                    CreatedByUserId = exercise.CreatedByUserId,
                    CreatedBy = exercise.CreatedByDisplay,
                    exercise.Description,
                    exercise.CreatedAt,
                    Questions = ParseQuestionsJson(exercise.Questions)
                }
            };

            return CreatedAtAction(nameof(GetExerciseById), new { id = exercise.ExerciseId }, result);
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("403") || ex.Message.Contains("401") || ex.Message.Contains("authentication failed"))
        {
            _logger.LogError(ex, "Gemini API authentication failed. Please check API key.");
            return StatusCode(500, new 
            { 
                success = false,
                message = "AI service authentication failed. Please check the API key configuration in appsettings.json.",
                error = "Authentication failed. Please verify your Gemini API key is valid and has the necessary permissions."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating exercise with AI questions");
            return StatusCode(500, new { 
                success = false,
                message = "Failed to generate exercise with AI. Using fallback questions instead.",
                error = ex.Message 
            });
        }
    }

    /// <summary>
    /// Test Gemini AI connection (Debug endpoint)
    /// </summary>
    [HttpPost("test-gemini")]
    public async Task<ActionResult<object>> TestGeminiConnection()
    {
        try
        {
            _logger.LogInformation("Testing Gemini AI connection");
            
            // Test connection first
            var connectionTest = await _geminiService.TestConnectionAsync();
            
            if (!connectionTest)
            {
                return Ok(new { 
                    Success = false, 
                    Message = "Gemini connection failed",
                    Details = "Check API key and internet connection"
                });
            }

            // Test simple question generation
            var questions = await _geminiService.GenerateQuestionsAsync(
                "Business Meeting", 
                "Part 5", 
                "Intermediate", 
                2
            );

            return Ok(new {
                Success = true,
                Message = "Gemini connection successful",
                ConnectionTest = connectionTest,
                QuestionsGenerated = questions.Count,
                SampleQuestions = questions.Take(2)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing Gemini connection");
            return StatusCode(500, new { 
                Success = false, 
                Message = "Error testing Gemini connection", 
                ErrorDetails = ex.Message 
            });
        }
    }

    /// <summary>
    /// Get exercises by source type (Admin function)
    /// </summary>
    [HttpGet("admin-uploads")]
    public async Task<ActionResult<IEnumerable<object>>> GetAdminUploads()
    {
        try
        {
            var exercises = await _context.Exercises
                .Include(e => e.CreatedByUser)
                .Where(e => e.SourceType == "uploaded" && e.IsActive)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            var response = exercises.Select(e => new
            {
                e.ExerciseId,
                Id = e.ExerciseId,
                e.Title,
                Name = e.Title,
                e.SourceType,
                CreatedByUserId = e.CreatedByUserId,
                CreatedBy = e.CreatedByDisplay,
                e.CreatedAt,
                e.OriginalFileName
            });

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting admin uploaded exercises");
            return StatusCode(500, new { message = "Error getting admin uploaded exercises" });
        }
    }

    /// <summary>
    /// Get upload statistics (Admin function)
    /// </summary>
    [HttpGet("upload-stats")]
    public async Task<ActionResult<object>> GetUploadStats()
    {
        try
        {
            var stats = await _context.Exercises
                .Where(e => e.IsActive)
                .GroupBy(e => e.SourceType)
                .Select(g => new { SourceType = g.Key, Count = g.Count() })
                .ToListAsync();

            var result = new
            {
                TotalUploaded = stats.FirstOrDefault(s => s.SourceType == "uploaded")?.Count ?? 0,
                TotalAI = stats.FirstOrDefault(s => s.SourceType == "ai")?.Count ?? 0,
                TotalManual = stats.FirstOrDefault(s => s.SourceType == "manual")?.Count ?? 0,
                TotalExercises = stats.Sum(s => s.Count)
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting upload statistics");
            return StatusCode(500, new { message = "Error getting upload statistics" });
        }
    }

    /// <summary>
    /// Delete exercise (Admin function)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteExercise(int id)
    {
        try
        {
            var exercise = await _context.Exercises.FindAsync(id);
            if (exercise == null)
                return NotFound(new { message = $"Exercise with ID {id} not found" });

            // Soft delete
            exercise.IsActive = false;
            exercise.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting exercise with ID {ExerciseId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    #region Helper Methods

    private static List<dynamic> ParseQuestionsJson(string questionsJson)
    {
        var results = new List<dynamic>();
        if (string.IsNullOrWhiteSpace(questionsJson))
            return results;

        try
        {
            using var document = JsonDocument.Parse(questionsJson);
            if (document.RootElement.ValueKind != JsonValueKind.Array)
                return results;

            var index = 0;
            foreach (var element in document.RootElement.EnumerateArray())
            {
                var questionText = ExtractQuestionText(element, ++index);
                var options = ExtractOptions(element);
                var correctAnswer = ExtractCorrectAnswer(element, options.Length);
                var explanation = ExtractExplanation(element);

                results.Add(new
                {
                    questionText,
                    question = questionText,
                    options,
                    correctAnswer,
                    explanation = explanation ?? "No explanation available"
                });
            }
        }
        catch
        {
            // Ignore malformed JSON to keep API resilient
        }

        return results.Cast<dynamic>().ToList();
    }

    private static string ExtractQuestionText(JsonElement element, int fallbackIndex)
    {
        if (TryGetString(element, out var text, "questionText", "question", "text", "prompt", "title"))
            return text;

        return $"Question {fallbackIndex}";
    }

    private static string[] ExtractOptions(JsonElement element)
    {
        if (TryGetArray(element, out var options, "options", "choices", "answers", "answerChoices"))
        {
            var parsed = options
                .Select((opt, idx) => opt.ValueKind == JsonValueKind.String
                    ? opt.GetString() ?? $"Option {idx + 1}"
                    : opt.GetRawText())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .ToArray();

            if (parsed.Length > 0)
                return parsed;
        }

        return new[] { "Option A", "Option B", "Option C", "Option D" };
    }

    private static int ExtractCorrectAnswer(JsonElement element, int optionCount)
    {
        if (TryGetInt(element, out var numeric, "correctAnswer", "correct_answer", "answer", "correctOption", "correct_option"))
            return NormalizeAnswerIndex(numeric, optionCount);

        if (TryGetString(element, out var strValue, "correctAnswer", "correct_answer", "answer"))
        {
            var letterIndex = LetterToIndex(strValue);
            if (letterIndex >= 0)
                return letterIndex;

            if (int.TryParse(strValue, out var parsed))
                return NormalizeAnswerIndex(parsed, optionCount);
        }

        return 0;
    }

    private static string? ExtractExplanation(JsonElement element)
    {
        if (TryGetString(element, out var explanation, "explanation", "reason", "rationale"))
            return explanation;

        return null;
    }

    private static bool TryGetString(JsonElement element, out string value, params string[] propertyNames)
    {
        foreach (var property in propertyNames)
        {
            if (TryGetPropertyCaseInsensitive(element, property, out var jsonElement) && jsonElement.ValueKind == JsonValueKind.String)
            {
                value = jsonElement.GetString() ?? string.Empty;
                return true;
            }
        }

        value = string.Empty;
        return false;
    }

    private static bool TryGetInt(JsonElement element, out int value, params string[] propertyNames)
    {
        foreach (var property in propertyNames)
        {
            if (TryGetPropertyCaseInsensitive(element, property, out var jsonElement))
            {
                if (jsonElement.ValueKind == JsonValueKind.Number && jsonElement.TryGetInt32(out value))
                    return true;

                if (jsonElement.ValueKind == JsonValueKind.String && int.TryParse(jsonElement.GetString(), out value))
                    return true;
            }
        }

        value = 0;
        return false;
    }

    private static bool TryGetArray(JsonElement element, out IEnumerable<JsonElement> items, params string[] propertyNames)
    {
        foreach (var property in propertyNames)
        {
            if (TryGetPropertyCaseInsensitive(element, property, out var jsonElement) && jsonElement.ValueKind == JsonValueKind.Array)
            {
                items = jsonElement.EnumerateArray();
                return true;
            }
        }

        items = Enumerable.Empty<JsonElement>();
        return false;
    }

    private static bool TryGetPropertyCaseInsensitive(JsonElement element, string propertyName, out JsonElement value)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            value = default;
            return false;
        }

        if (element.TryGetProperty(propertyName, out value))
            return true;

        foreach (var property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    private static int NormalizeAnswerIndex(int rawValue, int optionCount)
    {
        if (optionCount <= 0)
            return Math.Max(0, rawValue);

        if (rawValue >= 0 && rawValue < optionCount)
            return rawValue;

        if (rawValue >= 1 && rawValue <= optionCount)
            return rawValue - 1;

        return 0;
    }

    private static int LetterToIndex(string? letter)
    {
        if (string.IsNullOrWhiteSpace(letter))
            return -1;

        return letter.Trim().ToUpperInvariant() switch
        {
            "A" => 0,
            "B" => 1,
            "C" => 2,
            "D" => 3,
            _ => -1
        };
    }

    private string GenerateTemplateContent(string topic, string type, string level)
    {
        // Normalize type for matching
        var normalizedType = type?.Replace(" ", "").ToLower() ?? "part7";
        
        return normalizedType switch
        {
            "part5" => GeneratePart5Content(topic, level),
            "part6" => GeneratePart6Content(topic, level),
            "part7" => GeneratePart7Content(topic, level),
            _ => $"Practice passage about {topic} ({level})"
        };
    }

    private string GeneratePart5Content(string topic, string level)
    {
        // Part 5: Grammar exercises with increasing difficulty by level
        switch (level.ToLower())
        {
            case "beginner":
                return $"Basic grammar exercises about {topic}. Focus on simple tenses, basic sentence structures, and common vocabulary. Choose the best answer to complete each sentence.";
            case "intermediate": 
                return $"Intermediate grammar exercises about {topic}. Focus on complex tenses, conditional sentences, passive voice, and business vocabulary. Choose the best answer to complete each sentence.";
            case "advanced":
                return $"Advanced grammar exercises about {topic}. Focus on subjunctive mood, complex conditionals, advanced syntax, and sophisticated vocabulary. Choose the best answer to complete each sentence.";
            default:
                return $"Grammar exercises focusing on {topic} - {level} level. Complete the sentences by choosing the correct answer.";
        }
    }

    private string GeneratePart6Content(string topic, string level)
    {
        // Part 6: Text completion with difficulty based on level
        if (level.ToLower() == "beginner")
        {
            // Beginner: Simple vocabulary and basic sentence structures
            var beginnerTemplates = new Dictionary<string, string>
            {
                ["business"] = @"Job Application Email

Dear Sir/Madam,

I __(1)__ writing to apply for the sales assistant position at your company. I __(2)__ this job advertisement in the local newspaper last week.

I have experience __(3)__ in retail for two years. I am good __(4)__ working with customers and helping them find what they need.

I hope to hear from you soon.

Best regards,
John Smith",
                ["sport"] = @"Sports Club Notice

Dear Members,

The tennis courts __(1)__ closed for repair next week. We __(2)__ to finish the work by Friday so you can play again on Saturday.

During this time, you __(3)__ use the basketball court instead. Please __(4)__ us if you have any questions.

Thank you,
Sports Club Manager",
                ["family"] = @"Family Picnic Invitation

Hi Everyone,

We __(1)__ having a family picnic this Sunday at the park. Please __(2)__ some food to share with everyone.

The picnic __(3)__ start at 12:00 PM. We __(4)__ games for the children and music for dancing.

See you there!
Love,
Mom",
                ["meeting"] = @"Office Meeting Notice

To: All Staff
From: Manager

There __(1)__ be a meeting next Monday at 10 AM in the main office. We __(2)__ discuss the new work schedule.

Please __(3)__ on time. If you cannot come, please __(4)__ me know before Monday.

Thank you,
Office Manager"
            };

            var key = beginnerTemplates.Keys.FirstOrDefault(k => topic.ToLower().Contains(k)) ?? "business";
            return beginnerTemplates[key];
        }
        else if (level.ToLower() == "intermediate")
        {
            // Intermediate: More complex vocabulary and structures
            var intermediateTemplates = new Dictionary<string, string>
            {
                ["business"] = @"From: Marketing Director <marketing@company.com>
To: All Employees <all@company.com>
Subject: Annual Company Conference

Dear Staff,

I'm excited to announce our annual company conference __(1)__ on March 15-16 at the Grand Hotel. This year's theme focuses __(2)__ innovation and growth strategies.

The conference will feature keynote speakers, workshops, and networking sessions. Please __(3)__ your attendance by March 1st as seating is limited. 

We look forward __(4)__ seeing everyone there.

Best regards,
Sarah Chen
Marketing Director",
                ["sport"] = @"Sports Club Newsletter

Welcome to our monthly sports update! This month has been __(1)__ exciting for our club members. Our basketball team __(2)__ won three consecutive matches in the regional tournament.

The swimming pool renovation is __(3)__ completed and will reopen next Monday. All members are __(4)__ to try our new facilities and updated equipment.

Don't forget to register for the annual sports day event happening next month!",
                ["family"] = @"Family Reunion Planning Committee

Dear Family Members,

We are __(1)__ to announce our annual family reunion scheduled for July 15th at Grandma's house. Everyone is __(2)__ to bring a dish to share for our potluck lunch.

The children's activities __(3)__ organized by cousin Maria, and Uncle Tom will __(4)__ the barbecue as usual. Please RSVP by June 30th.

Looking forward to seeing everyone!",
                ["meeting"] = @"MEMO
TO: All Department Heads
FROM: Operations Manager
RE: Monthly Review Meeting

The monthly departmental review meeting __(1)__ scheduled for next Thursday at 2:00 PM in Conference Room B. Please __(2)__ your monthly reports and budget summaries.

This meeting is __(3)__ for all department heads. If you cannot attend, please __(4)__ a representative from your team.

Thank you for your cooperation."
            };

            var key = intermediateTemplates.Keys.FirstOrDefault(k => topic.ToLower().Contains(k)) ?? "business";
            return intermediateTemplates[key];
        }
        else // Advanced
        {
            // Advanced: Complex structures, sophisticated vocabulary, formal language
            var advancedTemplates = new Dictionary<string, string>
            {
                ["business"] = @"CONFIDENTIAL MEMORANDUM
From: Chief Executive Officer
To: Board of Directors
Re: Strategic Restructuring Proposal

Esteemed Board Members,

Following extensive deliberation and comprehensive market analysis, I am __(1)__ a strategic restructuring initiative that could potentially revolutionize our operational paradigm.

The proposed framework __(2)__ substantial modifications to our current hierarchical structure, necessitating careful consideration of stakeholder implications. Our preliminary assessment __(3)__ that implementation would yield significant competitive advantages while mitigating operational redundancies.

I respectfully request your thorough __(4)__ of the attached documentation prior to our upcoming board meeting.

Respectfully submitted,
Margaret Harrison, CEO",
                ["sport"] = @"International Sports Federation Bulletin

Distinguished Colleagues,

The recent amendments to competition regulations have __(1)__ considerable discourse among member federations regarding implementation protocols and compliance mechanisms.

Our technical committee has __(2)__ extensive consultations with international stakeholders to ensure seamless integration of these modifications. The new framework __(3)__ enhanced fairness standards while maintaining competitive integrity.

We anticipate your continued __(4)__ as we navigate these regulatory transitions and strengthen our global sporting community.

Yours in sport,
Dr. Elizabeth Chen
Secretary General",
                ["family"] = @"Estate Planning Consultation Notice

Dear Extended Family,

Following Grandfather's recent passing and pursuant to his expressed wishes, we are __(1)__ a comprehensive review of estate distribution and family trust arrangements.

The legal proceedings __(2)__ careful coordination among all beneficiaries and require unanimous consensus on several critical matters. Our estate attorney has __(3)__ that proper documentation and family agreement are essential for expeditious resolution.

Your prompt __(4)__ to this matter would be greatly appreciated as we honor Grandfather's legacy and ensure fair distribution according to his final wishes.

With sincere regards,
The Estate Committee",
                ["meeting"] = @"Executive Committee Resolution

WHEREAS, the organization has undergone significant operational transformations, and
WHEREAS, stakeholder expectations have __(1)__ substantially since our last strategic planning session,

BE IT RESOLVED that the Executive Committee hereby __(2)__ the formation of a comprehensive review panel to evaluate current policies and procedures.

This initiative __(3)__ immediate attention to regulatory compliance, operational efficiency, and stakeholder satisfaction metrics. All department heads are __(4)__ to provide detailed assessments within thirty days of this resolution.

Adopted this day by unanimous consent."
            };

            var key = advancedTemplates.Keys.FirstOrDefault(k => topic.ToLower().Contains(k)) ?? "business";
            return advancedTemplates[key];
        }
    }

    private string GeneratePart7Content(string topic, string level)
    {
        if (level.ToLower() == "beginner")
        {
            // Beginner: Single passage
            var beginnerTemplates = new Dictionary<string, string>
            {
                ["business"] = @"New Store Opening

Green Grocery Store will open its new location on Oak Street next Monday, November 20th. The store will be open from 8:00 AM to 9:00 PM, Monday through Saturday, and 10:00 AM to 6:00 PM on Sunday.

To celebrate the opening, all customers will receive a 10% discount on their first purchase. The store offers fresh fruits, vegetables, dairy products, and organic foods at competitive prices.

Free parking is available behind the store. For more information, call 555-0123.",

                ["meeting"] = @"Library Notice

The City Library will be closed for renovation from December 1st to December 15th. During this time, the library's books cannot be borrowed or returned.

The mobile library service will visit different neighborhoods during the closure. Check the library website for the mobile library schedule.

All books currently borrowed have their due dates extended to December 20th. No late fees will be charged during the renovation period.",

                ["travel"] = @"Bus Service Update

Starting January 1st, City Bus Route 42 will have a new schedule. The first bus will depart at 6:00 AM instead of 6:30 AM, and the last bus will leave at 11:00 PM instead of 10:30 PM.

Buses will run every 15 minutes during peak hours (7:00-9:00 AM and 5:00-7:00 PM) and every 30 minutes during other times.

Monthly passes can be purchased at any bus station or online. Students and seniors receive a 25% discount."
            };

            var key = beginnerTemplates.Keys.FirstOrDefault(k => topic.ToLower().Contains(k)) ?? "business";
            return beginnerTemplates[key];
        }
        else
        {
            // Intermediate/Advanced: Two passages
            var advancedTemplates = new Dictionary<string, string>
            {
                ["business"] = @"Document 1: Email
From: Jennifer Walsh <j.walsh@globaltech.com>
To: Project Team <projectteam@globaltech.com>
Subject: Q4 Project Status Update

Dear Team,

I'm writing to update you on our Q4 projects. The new software platform launch is progressing well and remains on schedule for a December 15th release. However, we've encountered some challenges with the mobile application integration that may require additional resources.

Please attend the emergency meeting tomorrow at 3:00 PM to discuss resource allocation and timeline adjustments.

Best,
Jennifer Walsh
Project Manager

Document 2: Meeting Agenda
GlobalTech Q4 Emergency Meeting
Date: November 22, 2025
Time: 3:00 PM - 4:30 PM
Location: Conference Room A

Agenda:
1. Mobile integration issues (15 minutes)
2. Resource reallocation discussion (30 minutes)
3. Timeline revision (20 minutes)
4. Client communication strategy (25 minutes)

Action items from previous meetings:
- Complete user testing by November 30th
- Finalize API documentation
- Schedule client demo for December 10th",

                ["meeting"] = @"Document 1: Company Announcement
NOTICE TO ALL EMPLOYEES

Effective January 1, 2026, FlexiCorp will implement a new flexible working policy. Employees may work from home up to three days per week, subject to manager approval and department requirements.

The policy aims to improve work-life balance and reduce commute-related stress. Training sessions on remote work best practices will be held in December.

For questions, contact HR at ext. 2400.

Document 2: HR Policy Guidelines
Remote Work Policy Implementation

Eligibility:
- Full-time employees with 6+ months tenure
- Roles suitable for remote work
- Satisfactory performance reviews

Requirements:
- Reliable internet connection
- Dedicated workspace at home
- Attendance at mandatory in-person meetings

Application Process:
1. Complete online application form
2. Manager approval required
3. IT equipment request if needed
4. Signed remote work agreement",

                ["travel"] = @"Document 1: Travel Blog Post
Hidden Gems of Southeast Asia

While most tourists flock to Bangkok and Singapore, experienced travelers know that the real treasures lie off the beaten path. Places like Luang Prabang in Laos offer stunning French colonial architecture and peaceful riverside settings.

The key to discovering these hidden gems is to travel slowly, stay in locally-owned accommodations, and engage with residents. Many of the most memorable experiences come from unexpected encounters and spontaneous detours.

Budget travelers should consider the shoulder seasons (April-May and September-October) when prices are lower and crowds thinner.

Document 2: Travel Agency Brochure
Discover Southeast Asia Package

15-Day Adventure Tour
Starting from $2,299 per person

Includes:
- Flights from major US cities
- Accommodation (mix of hotels and guesthouses)
- Local transportation
- English-speaking guides
- Some meals

Destinations:
- Thailand: Bangkok, Chiang Mai (4 days)
- Laos: Luang Prabang, Vang Vieng (3 days)
- Vietnam: Hanoi, Ha Long Bay (4 days)
- Cambodia: Siem Reap, Phnom Penh (4 days)

Best Travel Times: November-March
Group Size: Maximum 16 people
Booking Deadline: 60 days before departure"
            };

            var key = advancedTemplates.Keys.FirstOrDefault(k => topic.ToLower().Contains(k)) ?? "business";
            return advancedTemplates[key];
        }
    }

    private string GenerateDefaultContent(string topic)
    {
        return $"This is a sample reading exercise about {topic}. The content will be generated based on the selected topic and difficulty level.";
    }

    private string GenerateSampleQuestions(string topic, string type, string level)
    {
        var questions = new List<object>();
        
        // Normalize type for matching
        var normalizedType = type?.Replace(" ", "").ToLower();
        var normalizedLevel = level?.ToLower() ?? "intermediate";
        
        switch (normalizedType)
        {
            case "part5": // Grammar - 5 questions with increasing difficulty
                var grammarTopics = GetGrammarTopicsByLevel(normalizedLevel);
                for (int i = 1; i <= 5; i++)
                {
                    var grammarFocus = grammarTopics[Math.Min(i-1, grammarTopics.Length-1)];
                    questions.Add(new
                    {
                        questionText = $"({grammarFocus}) Choose the correct option to complete the sentence about {topic}:",
                        options = GetGrammarOptions(grammarFocus, normalizedLevel),
                        correctAnswer = 0,
                        explanation = $"{grammarFocus} explanation for {topic} - question {i}"
                    });
                }
                break;
                
            case "part6": // Text completion - 4 questions with context difficulty
                var completionTypes = GetCompletionTypesByLevel(normalizedLevel);
                for (int i = 1; i <= 4; i++)
                {
                    var completionFocus = completionTypes[Math.Min(i-1, completionTypes.Length-1)];
                    questions.Add(new
                    {
                        questionText = $"({completionFocus}) Choose the best word/phrase for blank ({i}) in the {topic} passage:",
                        options = GetCompletionOptions(completionFocus, normalizedLevel),
                        correctAnswer = 0,
                        explanation = $"{completionFocus} explanation for blank {i} about {topic}"
                    });
                }
                break;
                
            case "part7": // Reading comprehension - 4 questions (easy to hard)
                string[] difficultyTypes = GetReadingDifficultyTypes(normalizedLevel);
                for (int i = 1; i <= 4; i++)
                {
                    questions.Add(new
                    {
                        questionText = $"({difficultyTypes[i-1]}) What can be inferred about {topic} from question {i}?",
                        options = GetReadingOptions(difficultyTypes[i-1], normalizedLevel),
                        correctAnswer = 0,
                        explanation = $"Reading comprehension explanation for {difficultyTypes[i-1]} question about {topic}"
                    });
                }
                break;
                
            default:
                // Fallback - General reading questions
                questions.Add(new
                {
                    questionText = $"What is the main topic of this {type?.ToLower()} exercise?",
                    options = new[] { topic, "General information", "Technical details", "Personal opinions" },
                    correctAnswer = 0,
                    explanation = $"The exercise focuses on {topic}."
                });
                questions.Add(new
                {
                    questionText = "According to the passage, what is the key information?",
                    options = new[] { "Key point A", "Key point B", "Key point C", "Key point D" },
                    correctAnswer = 1,
                    explanation = "This information is mentioned in the text."
                });
                break;
        }

        return JsonSerializer.Serialize(questions);
    }

    private string GenerateBaseContent(string topic, string type, string level)
    {
        // This generates base content/context for AI to work with
        return type switch
        {
            "Part 5" => $"Create grammar exercises about {topic} for {level} level",
            "Part 6" => $"Create a business document about {topic} with blanks for {level} level",
            "Part 7" => $"Create reading comprehension passage(s) about {topic} for {level} level",
            _ => $"Create TOEIC exercises about {topic} for {level} level"
        };
    }

    private int GetQuestionCountByType(string type)
    {
        return type switch
        {
            "Part 5" => 5,
            "Part 6" => 4,
            "Part 7" => 4,
            _ => 4
        };
    }

    private string ExtractContentFromAiResponse(string baseContent, string type, List<EngAce.Api.Services.AI.GeneratedQuestion> aiQuestions)
    {
        // For Part 6 and Part 7, try to get the actual passage content from Gemini
        if ((type == "Part 6" || type == "Part 7") && aiQuestions.Any())
        {
            // Get raw Gemini response to extract passage content
            try
            {
                var passageContent = GetPassageContentFromGemini(baseContent, type, GetQuestionCountByType(type)).Result;
                if (!string.IsNullOrWhiteSpace(passageContent))
                {
                    return passageContent;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to extract passage content from Gemini for {Type}", type);
            }
        }
        
        // Fallback to template content if Gemini extraction fails
        return GetTemplateContentByType(type, "business", "intermediate");
    }

    private async Task<string> GetPassageContentFromGemini(string baseContent, string type, int questionCount)
    {
        try
        {
            var rawResponse = await _geminiService.GetRawGeminiResponseAsync(baseContent, type, "Intermediate", questionCount);
            
            if (string.IsNullOrWhiteSpace(rawResponse))
                return string.Empty;

            // Extract passage content before the questions
            // For Part 6: Look for email/document content
            // For Part 7: Look for reading passage content
            
            if (type == "Part 6")
            {
                // Extract email/document content (usually comes before questions in Gemini response)
                var lines = rawResponse.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                var passageLines = new List<string>();
                var foundJsonStart = false;
                
                foreach (var line in lines)
                {
                    if (line.Trim().StartsWith("[") || line.Trim().StartsWith("{"))
                    {
                        foundJsonStart = true;
                        break;
                    }
                    
                    if (!foundJsonStart && !string.IsNullOrWhiteSpace(line.Trim()) && 
                        !line.Contains("Create") && !line.Contains("JSON") && !line.Contains("Return"))
                    {
                        passageLines.Add(line.Trim());
                    }
                }
                
                if (passageLines.Any())
                {
                    return string.Join("\n\n", passageLines);
                }
            }
            else if (type == "Part 7")
            {
                // Similar extraction for Part 7 reading passages
                var lines = rawResponse.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                var passageLines = new List<string>();
                var foundJsonStart = false;
                
                foreach (var line in lines)
                {
                    if (line.Trim().StartsWith("[") || line.Trim().StartsWith("{"))
                    {
                        foundJsonStart = true;
                        break;
                    }
                    
                    if (!foundJsonStart && !string.IsNullOrWhiteSpace(line.Trim()) &&
                        !line.Contains("Create") && !line.Contains("JSON") && !line.Contains("Return"))
                    {
                        passageLines.Add(line.Trim());
                    }
                }
                
                if (passageLines.Any())
                {
                    return string.Join("\n\n", passageLines);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting passage content from Gemini for {Type}", type);
        }
        
        return string.Empty;
    }

    private string GetTemplateContentByType(string type, string topic, string level)
    {
        return type switch
        {
            "Part 6" => GeneratePart6Content(topic, level),
            "Part 7" => GeneratePart7Content(topic, level),
            _ => GenerateBaseContent(topic, type, level)
        };
    }

    private string[] GetGrammarTopicsByLevel(string level)
    {
        return level switch
        {
            "beginner" => new[] { "simple present", "present continuous", "simple past", "basic prepositions", "articles" },
            "intermediate" => new[] { "present perfect", "past continuous", "conditional sentences", "passive voice", "complex prepositions" },
            "advanced" => new[] { "subjunctive mood", "complex conditionals", "advanced passive", "reported speech", "sophisticated syntax" },
            _ => new[] { "present perfect", "past continuous", "conditional sentences", "passive voice", "complex prepositions" }
        };
    }

    private string[] GetGrammarOptions(string grammarFocus, string level)
    {
        return level switch
        {
            "beginner" => new[] { $"Basic {grammarFocus} option A", $"Basic {grammarFocus} option B", $"Basic {grammarFocus} option C", $"Basic {grammarFocus} option D" },
            "intermediate" => new[] { $"Intermediate {grammarFocus} option A", $"Intermediate {grammarFocus} option B", $"Intermediate {grammarFocus} option C", $"Intermediate {grammarFocus} option D" },
            "advanced" => new[] { $"Advanced {grammarFocus} option A", $"Advanced {grammarFocus} option B", $"Advanced {grammarFocus} option C", $"Advanced {grammarFocus} option D" },
            _ => new[] { $"{grammarFocus} option A", $"{grammarFocus} option B", $"{grammarFocus} option C", $"{grammarFocus} option D" }
        };
    }

    private string[] GetCompletionTypesByLevel(string level)
    {
        return level switch
        {
            "beginner" => new[] { "basic verb", "simple adjective", "common preposition", "basic noun" },
            "intermediate" => new[] { "complex verb form", "descriptive adjective", "phrasal verb", "business term" },
            "advanced" => new[] { "sophisticated vocabulary", "formal register", "complex syntax", "idiomatic expression" },
            _ => new[] { "complex verb form", "descriptive adjective", "phrasal verb", "business term" }
        };
    }

    private string[] GetCompletionOptions(string completionFocus, string level)
    {
        return level switch
        {
            "beginner" => new[] { $"Simple {completionFocus} A", $"Simple {completionFocus} B", $"Simple {completionFocus} C", $"Simple {completionFocus} D" },
            "intermediate" => new[] { $"Intermediate {completionFocus} A", $"Intermediate {completionFocus} B", $"Intermediate {completionFocus} C", $"Intermediate {completionFocus} D" },
            "advanced" => new[] { $"Advanced {completionFocus} A", $"Advanced {completionFocus} B", $"Advanced {completionFocus} C", $"Advanced {completionFocus} D" },
            _ => new[] { $"{completionFocus} A", $"{completionFocus} B", $"{completionFocus} C", $"{completionFocus} D" }
        };
    }

    private string[] GetReadingDifficultyTypes(string level)
    {
        return level switch
        {
            "beginner" => new[] { "main idea", "specific detail", "simple inference", "vocabulary" },
            "intermediate" => new[] { "main idea", "specific detail", "inference", "author's purpose" },
            "advanced" => new[] { "complex inference", "critical analysis", "synthesis", "evaluation" },
            _ => new[] { "main idea", "specific detail", "inference", "complex analysis" }
        };
    }

    private string[] GetReadingOptions(string difficultyType, string level)
    {
        return level switch
        {
            "beginner" => new[] { $"Basic {difficultyType} option A", $"Basic {difficultyType} option B", $"Basic {difficultyType} option C", $"Basic {difficultyType} option D" },
            "intermediate" => new[] { $"Intermediate {difficultyType} option A", $"Intermediate {difficultyType} option B", $"Intermediate {difficultyType} option C", $"Intermediate {difficultyType} option D" },
            "advanced" => new[] { $"Advanced {difficultyType} option A", $"Advanced {difficultyType} option B", $"Advanced {difficultyType} option C", $"Advanced {difficultyType} option D" },
            _ => new[] { $"{difficultyType} option A", $"{difficultyType} option B", $"{difficultyType} option C", $"{difficultyType} option D" }
        };
    }

    private string ExtractPassageFromRawGeminiResponse(string rawResponse)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(rawResponse))
                return string.Empty;

            // Try to find a ```json ... ``` code block first
            var jsonBlockStart = rawResponse.IndexOf("```json", StringComparison.OrdinalIgnoreCase);
            if (jsonBlockStart >= 0)
            {
                var start = rawResponse.IndexOf('\n', jsonBlockStart);
                if (start < 0) start = jsonBlockStart + "```json".Length;
                var end = rawResponse.IndexOf("```", start + 1, StringComparison.Ordinal);
                if (end > start)
                {
                    var jsonText = rawResponse.Substring(start + 1, end - start - 1).Trim();
                    try
                    {
                        // Try Part 6/7 format with passage + questions
                        using var doc = JsonDocument.Parse(jsonText);
                        if (doc.RootElement.TryGetProperty("passage", out var passageProp))
                        {
                            return passageProp.GetString() ?? string.Empty;
                        }
                        
                        // If no passage property, might be array format - return empty for now
                        // Part 5 uses array format and doesn't need passage
                        return string.Empty;
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogDebug(ex, "JSON parsing of extracted code block failed");
                    }
                }
            }

            // If no json block found, try to find first JSON object in text
            var firstBrace = rawResponse.IndexOf('{');
            if (firstBrace >= 0)
            {
                var candidate = rawResponse.Substring(firstBrace);
                try
                {
                    using var doc = JsonDocument.Parse(candidate);
                    if (doc.RootElement.TryGetProperty("passage", out var passageProp))
                        return passageProp.GetString() ?? string.Empty;
                }
                catch (JsonException)
                {
                    // ignore
                }
            }

            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error extracting passage from raw Gemini response");
            return string.Empty;
        }
    }

    private async Task<int?> ResolveCreatedByUserIdAsync(string? createdBy)
    {
        if (string.IsNullOrWhiteSpace(createdBy))
            return null;

        if (int.TryParse(createdBy, out var userId))
        {
            var exists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (exists)
                return userId;
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u =>
                u.Username == createdBy ||
                u.Email == createdBy ||
                u.FullName == createdBy);

        return user?.Id;
    }

    private static string SerializeCorrectAnswersFromRequest(IEnumerable<QuestionRequest> questions)
    {
        var letters = questions.Select(q => ConvertAnswerIndexToLetter(q.CorrectAnswer));
        return JsonSerializer.Serialize(letters);
    }

    private static string ConvertAnswerIndexToLetter(int answerIndex)
    {
        var normalized = Math.Max(0, answerIndex - 1);
        return normalized switch
        {
            0 => "A",
            1 => "B",
            2 => "C",
            3 => "D",
            _ => "A"
        };
    }

    #endregion

    #region Request Models

    public class CreateExerciseRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Level { get; set; } = "Intermediate";
        public string Type { get; set; } = "Reading";
        public string? Category { get; set; }
        public string? Description { get; set; }
        public int? EstimatedMinutes { get; set; }
        public string? CreatedBy { get; set; }
    }

    public class AddQuestionsRequest
    {
        public List<QuestionRequest> Questions { get; set; } = new();
    }

    public class QuestionRequest
    {
        public string QuestionText { get; set; } = string.Empty;
        public string OptionA { get; set; } = string.Empty;
        public string OptionB { get; set; } = string.Empty;
        public string OptionC { get; set; } = string.Empty;
        public string OptionD { get; set; } = string.Empty;
        public int CorrectAnswer { get; set; }
        public string? Explanation { get; set; }
    }

    public class SubmitResultRequest
    {
        public int ExerciseId { get; set; }
        public int UserId { get; set; }
        public List<int> Answers { get; set; } = new();
    }

    public class GenerateAIRequest
    {
        public string Topic { get; set; } = string.Empty;
        public string Level { get; set; } = "Intermediate";
        public string Type { get; set; } = "Reading";
        public string? CreatedBy { get; set; }
        public string Provider { get; set; } = "gemini"; // "gemini" or "openai"
    }

    public class CreatePassageRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? PartType { get; set; } = "Part 6";
        public string? Level { get; set; } = "Intermediate";
        public string? CreatedBy { get; set; } = "Admin";
    }

    #endregion
}