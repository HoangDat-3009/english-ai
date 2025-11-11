using EngAce.Api.DTO;
using Entities;
using Entities.Enums;
using Events;
using Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace EngAce.Api.Controllers;

/// <summary>
/// API endpoints cho bài tập nói
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SpeakingController : ControllerBase
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<SpeakingController> _logger;

    public SpeakingController(IMemoryCache cache, ILogger<SpeakingController> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Lấy danh sách các chủ đề bài nói
    /// </summary>
    [HttpGet("Topics")]
    public IActionResult GetTopics()
    {
        var topics = new Dictionary<int, string>
        {
            { (int)SpeakingTopic.SelfIntroduction, "Giới thiệu bản thân" },
            { (int)SpeakingTopic.DailyLife, "Cuộc sống hàng ngày" },
            { (int)SpeakingTopic.HobbiesAndInterests, "Sở thích và thú vui" },
            { (int)SpeakingTopic.TravelAndExploration, "Du lịch và khám phá" },
            { (int)SpeakingTopic.WorkAndCareer, "Công việc và nghề nghiệp" },
            { (int)SpeakingTopic.EducationAndLearning, "Giáo dục và học tập" },
            { (int)SpeakingTopic.TechnologyAndFuture, "Công nghệ và tương lai" }
        };

        return Ok(topics);
    }

    /// <summary>
    /// Tạo bài tập nói mới
    /// </summary>
    [HttpPost("Generate")]
    public async Task<IActionResult> Generate([FromBody] GenerateSpeakingExercise request)
    {
        try
        {
            _logger.LogInformation("Generating speaking exercise - Topic: {Topic}, Level: {Level}", 
                request.Topic, request.EnglishLevel);

            var scope = new SpeakingScope();
            var exercise = await scope.GenerateExerciseAsync(
                request.Topic, 
                request.EnglishLevel, 
                request.CustomTopic
            );

            // Cache exercise for 45 minutes
            var cacheKey = $"speaking_exercise_{exercise.ExerciseId}";
            _cache.Set(cacheKey, exercise, TimeSpan.FromMinutes(45));

            // Generate sample audio for the prompt
            string? audioContent = null;
            try
            {
                var apiKey = HttpContextHelper.GetGeminiApiKey();
                var audioBytes = await TextToSpeechHelper.SynthesizeAsync(apiKey, exercise.Prompt);
                audioContent = $"data:audio/mp3;base64,{audioBytes}";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to generate sample audio for speaking exercise");
            }

            var response = SpeakingExerciseResponse.FromEntity(exercise, audioContent);
            _logger.LogInformation("Successfully generated speaking exercise: {ExerciseId}", exercise.ExerciseId);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating speaking exercise");
            
            // Check for quota exceeded errors
            if (ex.Message.Contains("quota", StringComparison.OrdinalIgnoreCase) || 
                ex.Message.Contains("429"))
            {
                return StatusCode(429, new { message = "API đã vượt quá giới hạn yêu cầu. Vui lòng thử lại sau 1 phút." });
            }

            return StatusCode(500, new { message = "Không thể tạo bài tập nói. Vui lòng thử lại sau." });
        }
    }

    /// <summary>
    /// Phân tích giọng nói và ngữ pháp
    /// </summary>
    [HttpPost("Analyze")]
    public async Task<IActionResult> Analyze([FromBody] AnalyzeSpeechRequest request)
    {
        try
        {
            _logger.LogInformation("Analyzing speech for exercise: {ExerciseId}", request.ExerciseId);

            // Get cached exercise
            var cacheKey = $"speaking_exercise_{request.ExerciseId}";
            if (!_cache.TryGetValue<SpeakingExercise>(cacheKey, out var exercise) || exercise == null)
            {
                return NotFound(new { message = "Bài tập không tồn tại hoặc đã hết hạn" });
            }

            // Remove data URI prefix if present
            var audioBase64 = request.AudioData;
            if (audioBase64.Contains(","))
            {
                audioBase64 = audioBase64.Split(',')[1];
            }

            // Transcribe audio to text
            string transcribedText;
            try
            {
                transcribedText = await SpeechToTextHelper.TranscribeAsync(audioBase64);
                _logger.LogInformation("Transcribed text: {Text}", transcribedText);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to transcribe audio");
                return BadRequest(new { message = "Không thể nhận dạng giọng nói. Vui lòng đảm bảo âm thanh rõ ràng và thử lại." });
            }

            if (string.IsNullOrWhiteSpace(transcribedText))
            {
                return BadRequest(new { message = "Không phát hiện giọng nói trong audio. Vui lòng thu âm lại." });
            }

            // Analyze speech with AI
            var scope = new SpeakingScope();
            var analysis = await scope.AnalyzeSpeechAsync(transcribedText, exercise.Prompt);

            var response = SpeakingAnalysisResponse.FromEntity(analysis);
            _logger.LogInformation("Successfully analyzed speech - Overall score: {Score}", analysis.OverallScore);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing speech");

            // Check for quota exceeded errors
            if (ex.Message.Contains("quota", StringComparison.OrdinalIgnoreCase) || 
                ex.Message.Contains("429"))
            {
                return StatusCode(429, new { message = "API đã vượt quá giới hạn yêu cầu. Vui lòng thử lại sau 1 phút." });
            }

            return StatusCode(500, new { message = "Không thể phân tích giọng nói. Vui lòng thử lại sau." });
        }
    }

    /// <summary>
    /// Lấy danh sách cấp độ tiếng Anh
    /// </summary>
    [HttpGet("EnglishLevels")]
    public IActionResult GetEnglishLevels()
    {
        var levels = new Dictionary<int, string>
        {
            { (int)EnglishLevel.Beginner, "A1 - Beginner" },
            { (int)EnglishLevel.Elementary, "A2 - Elementary" },
            { (int)EnglishLevel.Intermediate, "B1 - Intermediate" },
            { (int)EnglishLevel.UpperIntermediate, "B2 - Upper Intermediate" },
            { (int)EnglishLevel.Advanced, "C1 - Advanced" },
            { (int)EnglishLevel.Proficient, "C2 - Proficient" }
        };

        return Ok(levels);
    }
}
