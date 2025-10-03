using EngAce.Api.DTO;
using Entities;
using Entities.Enums;
using Events;
using Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace EngAce.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ListeningController(IMemoryCache cache, ILogger<ListeningController> logger) : ControllerBase
    {
        private readonly IMemoryCache _cache = cache;
        private readonly ILogger<ListeningController> _logger = logger;
        private readonly string _accessKey = HttpContextHelper.GetAccessKey();

        /// <summary>
        /// Generate a listening exercise based on topic and parameters
        /// </summary>
        [HttpPost("Generate")]
        public async Task<ActionResult<ListeningExercise>> Generate([FromBody] GenerateListeningExercise request)
        {
            if (string.IsNullOrEmpty(_accessKey))
            {
                return Unauthorized("Invalid Access Key");
            }

            request.Topic = string.IsNullOrEmpty(request.Topic) ? "" : request.Topic.Trim();

            if (string.IsNullOrWhiteSpace(request.Topic))
            {
                return BadRequest("Tên chủ đề không được để trống");
            }

            if (GeneralHelper.GetTotalWords(request.Topic) > ListeningScope.MaxTotalWordsOfTopic)
            {
                return BadRequest($"Chủ đề không được chứa nhiều hơn {ListeningScope.MaxTotalWordsOfTopic} từ");
            }

            if (request.TotalQuestions < ListeningScope.MinTotalQuestions || request.TotalQuestions > ListeningScope.MaxTotalQuestions)
            {
                return BadRequest($"Số lượng câu hỏi phải nằm trong khoảng {ListeningScope.MinTotalQuestions} đến {ListeningScope.MaxTotalQuestions}");
            }

            if (request.TotalQuestions < request.QuestionTypes.Count)
            {
                return BadRequest($"Số lượng câu hỏi không được nhỏ hơn số dạng câu hỏi mà bạn chọn");
            }

            var cacheKey = $"GenerateListening-{request.Topic.ToLower()}-{string.Join(string.Empty, request.QuestionTypes)}-{request.Level}-{request.TotalQuestions}";
            if (_cache.TryGetValue(cacheKey, out var cachedExercise))
            {
                return Ok(cachedExercise);
            }

            try
            {
                var listeningExercise = await ListeningScope.GenerateExerciseAsync(_accessKey, request.Topic, request.QuestionTypes, request.Level, request.TotalQuestions, request.DurationInMinutes ?? 5, request.PreferredAccent ?? "American");
                
                _cache.Set(cacheKey, listeningExercise, TimeSpan.FromMinutes(30));
                
                return Ok(listeningExercise);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating listening exercise for topic: {Topic}", request.Topic);
                return StatusCode(500, "Đã có lỗi xảy ra khi tạo bài tập nghe. Vui lòng thử lại sau.");
            }
        }

        /// <summary>
        /// Get a specific listening exercise by ID
        /// </summary>
        [HttpGet("{exerciseId}")]
        public ActionResult<ListeningExercise> GetExercise(string exerciseId)
        {
            if (string.IsNullOrEmpty(_accessKey))
            {
                return Unauthorized("Invalid Access Key");
            }

            if (string.IsNullOrEmpty(exerciseId))
            {
                return BadRequest("Exercise ID không được để trống");
            }

            // Try to get from cache
            if (_cache.TryGetValue($"ListeningExercise-{exerciseId}", out var exercise))
            {
                return Ok(exercise);
            }

            return NotFound("Không tìm thấy bài tập nghe");
        }

        /// <summary>
        /// Submit listening exercise answers and get results
        /// </summary>
        [HttpPost("Submit")]
        public ActionResult<ListeningExerciseResult> Submit([FromBody] SubmitListeningExercise submission)
        {
            if (string.IsNullOrEmpty(_accessKey))
            {
                return Unauthorized("Invalid Access Key");
            }

            if (submission == null || string.IsNullOrEmpty(submission.ExerciseId))
            {
                return BadRequest("Dữ liệu bài nộp không hợp lệ");
            }

            // Get the original exercise to check answers
            if (!_cache.TryGetValue($"ListeningExercise-{submission.ExerciseId}", out ListeningExercise? exercise))
            {
                return NotFound("Không tìm thấy bài tập nghe");
            }

            try
            {
                // Convert DTO to Entity
                var answers = submission.Answers.Select(dto => new ListeningAnswer
                {
                    QuestionId = dto.QuestionId,
                    SelectedOptionIndex = dto.SelectedOptionIndex,
                    IsCorrect = false, // Will be set in EvaluateAnswers
                    TimeSpentOnQuestion = dto.TimeSpentOnQuestion
                }).ToList();

                var result = ListeningScope.EvaluateAnswers(exercise, answers, submission.TimeSpent);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating listening exercise answers for exercise: {ExerciseId}", submission.ExerciseId);
                return StatusCode(500, "Đã có lỗi xảy ra khi chấm bài. Vui lòng thử lại sau.");
            }
        }

        /// <summary>
        /// Get suggested topics for listening exercises based on English level
        /// </summary>
        [HttpGet("SuggestTopics")]
        public ActionResult<List<string>> SuggestTopics([FromQuery] EnglishLevel level)
        {
            if (string.IsNullOrEmpty(_accessKey))
            {
                return Unauthorized("Invalid Access Key");
            }

            try
            {
                var topics = ListeningScope.GetSuggestedTopics(level);
                return Ok(topics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting suggested topics for level: {Level}", level);
                return StatusCode(500, "Đã có lỗi xảy ra khi lấy danh sách chủ đề. Vui lòng thử lại sau.");
            }
        }
    }
}