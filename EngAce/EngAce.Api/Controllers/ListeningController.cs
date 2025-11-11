using EngAce.Api.DTO;
using Entities;
using Entities.Enums;
using Events;
using Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Linq;

namespace EngAce.Api.Controllers
{
    /// <summary>
    /// Provides endpoints for generating and grading AI-powered listening comprehension exercises.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ListeningController(IMemoryCache cache, ILogger<ListeningController> logger) : ControllerBase
    {
        private const int MaxTopicWordCount = 12;
        private static readonly TimeSpan CacheLifetime = TimeSpan.FromMinutes(45);

        private readonly IMemoryCache _cache = cache;
        private readonly ILogger<ListeningController> _logger = logger;
        private readonly string? _accessKey = HttpContextHelper.GetAccessKey();

        /// <summary>
        /// Generates a listening exercise tailored to the learner's preferences.
        /// </summary>
        /// <param name="request">The generation parameters supplied by the client.</param>
        /// <returns>A listening exercise containing audio, transcript, and questions.</returns>
        [HttpPost("Generate")]
        public async Task<ActionResult<ListeningExerciseResponse>> Generate([FromBody] GenerateListeningExercise request)
        {
            if (string.IsNullOrEmpty(_accessKey))
            {
                return Unauthorized("Invalid Access Key");
            }

            if (!Enum.IsDefined(typeof(ListeningGenre), request.Genre))
            {
                return BadRequest("Thể loại bài nghe không hợp lệ.");
            }

            if (request.TotalQuestions < ListeningScope.MinTotalQuestions || request.TotalQuestions > ListeningScope.MaxTotalQuestions)
            {
                return BadRequest($"Số lượng câu hỏi phải nằm trong khoảng {ListeningScope.MinTotalQuestions} đến {ListeningScope.MaxTotalQuestions}.");
            }

            if (!string.IsNullOrWhiteSpace(request.CustomTopic) && GeneralHelper.GetTotalWords(request.CustomTopic) > MaxTopicWordCount)
            {
                return BadRequest($"Chủ đề tùy chỉnh không được vượt quá {MaxTopicWordCount} từ.");
            }

            try
            {
                _logger.LogInformation("Starting listening exercise generation for genre {Genre}, level {Level}, questions {Questions}", request.Genre, request.EnglishLevel, request.TotalQuestions);
                var exercise = await ListeningScope.GenerateExerciseAsync(_accessKey, request.Genre, request.EnglishLevel, request.TotalQuestions, request.CustomTopic);
                _logger.LogInformation("Exercise generated successfully, generating audio...");
                var rawAudioContent = await TextToSpeechHelper.SynthesizeAsync(_accessKey, exercise.Transcript);
                var audioContent = string.IsNullOrWhiteSpace(rawAudioContent) || rawAudioContent.StartsWith("data:", StringComparison.OrdinalIgnoreCase)
                    ? rawAudioContent
                    : $"data:audio/mp3;base64,{rawAudioContent}";

                var exerciseId = Guid.NewGuid();
                var cacheItem = new ListeningExerciseCacheItem(exercise, audioContent);
                _cache.Set(GetCacheKey(exerciseId), cacheItem, CacheLifetime);

                var response = new ListeningExerciseResponse
                {
                    ExerciseId = exerciseId,
                    Title = exercise.Title,
                    Genre = GeneralHelper.GetEnumDescription(request.Genre),
                    EnglishLevel = exercise.EnglishLevel,
                    Transcript = exercise.Transcript,
                    AudioContent = audioContent,
                    Questions = exercise.Questions
                        .Select(q => new ListeningQuestionResponse
                        {
                            Question = q.Question,
                            Options = q.Options
                        })
                        .ToList()
                };

                _logger.LogInformation("Listening exercise generated for genre {Genre} with id {ExerciseId}", request.Genre, exerciseId);

                return Created("Success", response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate listening exercise for genre {Genre}. Error: {ErrorMessage}", request.Genre, ex.Message);
                
                // Check for quota exceeded error
                if (ex.Message.Contains("429") || ex.Message.Contains("Quota exceeded") || ex.Message.Contains("RATE_LIMIT_EXCEEDED"))
                {
                    return StatusCode(429, "API đã vượt quá giới hạn yêu cầu. Vui lòng thử lại sau 1 phút hoặc liên hệ quản trị viên để nâng cấp quota.");
                }
                
                return BadRequest($"Không thể tạo bài nghe: {ex.Message}");
            }
        }

        /// <summary>
        /// Grades the learner's answers for a previously generated listening exercise.
        /// </summary>
        /// <param name="request">The submitted answers together with the exercise identifier.</param>
        /// <returns>Detailed grading feedback for each question.</returns>
        [HttpPost("Grade")]
        public ActionResult<GradeListeningExerciseResponse> Grade([FromBody] GradeListeningExercise request)
        {
            if (!_cache.TryGetValue(GetCacheKey(request.ExerciseId), out ListeningExerciseCacheItem? cacheItem) || cacheItem == null)
            {
                return NotFound("Không tìm thấy bài nghe hoặc bài đã hết hạn.");
            }

            var answers = request.Answers ?? [];
            var answerLookup = answers.ToDictionary(a => a.QuestionIndex, a => a.SelectedOptionIndex);
            var questionsCount = cacheItem.Exercise.Questions.Count;
            var feedback = new List<ListeningQuestionGradeResponse>(questionsCount);

            for (var index = 0; index < questionsCount; index++)
            {
                var question = cacheItem.Exercise.Questions[index];
                answerLookup.TryGetValue(index, out var learnerAnswer);
                var hasAnswer = answerLookup.ContainsKey(index);
                var isCorrect = hasAnswer && learnerAnswer == question.RightOptionIndex;

                feedback.Add(new ListeningQuestionGradeResponse
                {
                    QuestionIndex = index,
                    Question = question.Question,
                    Options = question.Options,
                    SelectedOptionIndex = hasAnswer ? learnerAnswer : null,
                    RightOptionIndex = question.RightOptionIndex,
                    ExplanationInVietnamese = question.ExplanationInVietnamese,
                    IsCorrect = isCorrect
                });
            }

            var correctAnswers = feedback.Count(item => item.IsCorrect);
            var score = questionsCount == 0
                ? 0
                : Math.Round(correctAnswers / (double)questionsCount * 100, 2);

            var response = new GradeListeningExerciseResponse
            {
                ExerciseId = request.ExerciseId,
                Title = cacheItem.Exercise.Title,
                Transcript = cacheItem.Exercise.Transcript,
                AudioContent = cacheItem.AudioContent,
                TotalQuestions = questionsCount,
                CorrectAnswers = correctAnswers,
                Score = score,
                Questions = feedback
            };

            return Ok(response);
        }

        /// <summary>
        /// Retrieves the list of available listening genres.
        /// </summary>
        /// <returns>A dictionary mapping genre identifiers to their localized descriptions.</returns>
        [HttpGet("Genres")]
        [ResponseCache(Duration = 43200, Location = ResponseCacheLocation.Any, NoStore = false)]
        public ActionResult<Dictionary<int, string>> GetGenres()
        {
            var descriptions = Enum
                .GetValues(typeof(ListeningGenre))
                .Cast<ListeningGenre>()
                .ToDictionary(genre => (int)genre, genre => GeneralHelper.GetEnumDescription(genre));

            return Ok(descriptions);
        }

        private static string GetCacheKey(Guid exerciseId) => $"ListeningExercise-{exerciseId}";

        private sealed record ListeningExerciseCacheItem(ListeningExercise Exercise, string? AudioContent);
    }
}
