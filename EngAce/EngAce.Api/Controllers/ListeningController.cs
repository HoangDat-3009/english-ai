using EngAce.Api.DTO;
using Entities;
using Entities.Enums;
using Events;
using Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

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
        private static readonly ConcurrentDictionary<Guid, ListeningExerciseSummary> _recentExercises = new();
        private static readonly string ArchiveDirectoryPath = Path.Combine(AppContext.BaseDirectory, "Data", "ListeningExercises");
        private static readonly JsonSerializerOptions ArchiveSerializerOptions = new() { WriteIndented = true };

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
                var exercise = await ListeningScope.GenerateExerciseAsync(
                    _accessKey,
                    request.Genre,
                    request.EnglishLevel,
                    request.TotalQuestions,
                    request.CustomTopic,
                    request.AiModel);
                _logger.LogInformation("Exercise generated successfully, generating audio...");
                var rawAudioContent = await TextToSpeechHelper.SynthesizeAsync(_accessKey, exercise.Transcript);
                var audioContent = string.IsNullOrWhiteSpace(rawAudioContent) || rawAudioContent.StartsWith("data:", StringComparison.OrdinalIgnoreCase)
                    ? rawAudioContent
                    : $"data:audio/mp3;base64,{rawAudioContent}";

                var exerciseId = Guid.NewGuid();
                var createdAt = DateTime.UtcNow;
                var cacheItem = new ListeningExerciseCacheItem(exercise, audioContent, createdAt);

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(CacheLifetime)
                    .RegisterPostEvictionCallback(static (_, _, _, state) =>
                    {
                        if (state is Guid expiredId)
                        {
                            _recentExercises.TryRemove(expiredId, out _);
                        }
                    }, exerciseId);

                _cache.Set(GetCacheKey(exerciseId), cacheItem, cacheEntryOptions);

                var expiresAt = createdAt.Add(CacheLifetime);
                var summary = new ListeningExerciseSummary(
                    exerciseId,
                    exercise.Title,
                    GeneralHelper.GetEnumDescription(request.Genre),
                    exercise.EnglishLevel,
                    Convert.ToSByte(exercise.Questions.Count),
                    createdAt,
                    expiresAt);
                _recentExercises[exerciseId] = summary;

                await PersistExerciseArchiveAsync(exerciseId, exercise, request, audioContent, createdAt);

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
        /// Returns the list of recently generated listening exercises that are still cached.
        /// </summary>
        /// <param name="take">Optional number of items to return (1-50). Defaults to 20.</param>
        [HttpGet("Recent")]
        public ActionResult<IEnumerable<ListeningExerciseSummary>> GetRecent([FromQuery] int? take)
        {
            var limit = take.HasValue ? Math.Clamp(take.Value, 1, 50) : 20;
            var items = _recentExercises
                .Values
                .OrderByDescending(item => item.CreatedAt)
                .Take(limit)
                .ToList();

            return Ok(items);
        }

        /// <summary>
        /// Lists archived listening exercises saved as JSON files for teachers to review later.
        /// </summary>
        /// <param name="take">Optional page size (1-200). Defaults to 50.</param>
        [HttpGet("Archive")]
        public async Task<ActionResult<IEnumerable<ListeningExerciseArchiveSummary>>> GetArchivedExercises([FromQuery] int? take)
        {
            var limit = take.HasValue ? Math.Clamp(take.Value, 1, 200) : 50;

            if (!Directory.Exists(ArchiveDirectoryPath))
            {
                return Ok(Array.Empty<ListeningExerciseArchiveSummary>());
            }

            var summaries = new List<ListeningExerciseArchiveSummary>(limit);
            var archiveFiles = Directory
                .EnumerateFiles(ArchiveDirectoryPath, "*.json", SearchOption.TopDirectoryOnly)
                .OrderByDescending(path => System.IO.File.GetCreationTimeUtc(path));

            foreach (var filePath in archiveFiles)
            {
                if (summaries.Count >= limit)
                {
                    break;
                }

                try
                {
                    await using var stream = System.IO.File.OpenRead(filePath);
                    var archiveRecord = await JsonSerializer.DeserializeAsync<ListeningExerciseArchive>(stream);

                    if (archiveRecord is null)
                    {
                        continue;
                    }

                    var fileInfo = new FileInfo(filePath);
                    summaries.Add(new ListeningExerciseArchiveSummary(
                        archiveRecord.ExerciseId,
                        archiveRecord.Title,
                        archiveRecord.GenreLabel,
                        archiveRecord.Genre,
                        archiveRecord.EnglishLevel,
                        archiveRecord.TotalQuestions,
                        archiveRecord.CreatedAt,
                        archiveRecord.ExpiresAt,
                        fileInfo.Name,
                        fileInfo.Length,
                        archiveRecord.Request));
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to read listening archive file {FilePath}", filePath);
                }
            }

            return Ok(summaries);
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

        private async Task PersistExerciseArchiveAsync(Guid exerciseId, ListeningExercise exercise, GenerateListeningExercise request, string? audioContent, DateTime createdAt)
        {
            try
            {
                Directory.CreateDirectory(ArchiveDirectoryPath);
                var archiveRecord = new ListeningExerciseArchive(
                    exerciseId,
                    exercise.Title,
                    exercise.Genre,
                    GeneralHelper.GetEnumDescription(exercise.Genre),
                    exercise.EnglishLevel,
                    exercise.Questions.Count,
                    exercise.Transcript,
                    audioContent,
                    exercise.Questions,
                    createdAt,
                    createdAt.Add(CacheLifetime),
                    new ListeningExerciseArchiveRequest(
                        request.Genre,
                        request.EnglishLevel,
                        request.TotalQuestions,
                        request.CustomTopic,
                        request.AiModel));

                var filePath = Path.Combine(ArchiveDirectoryPath, $"{exerciseId}.json");
                await using var stream = System.IO.File.Create(filePath);
                await JsonSerializer.SerializeAsync(stream, archiveRecord, ArchiveSerializerOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to archive listening exercise {ExerciseId}", exerciseId);
            }
        }

        private static string GetCacheKey(Guid exerciseId) => $"ListeningExercise-{exerciseId}";

        private sealed record ListeningExerciseCacheItem(ListeningExercise Exercise, string? AudioContent, DateTime CreatedAt);

        public sealed record ListeningExerciseSummary(
            Guid ExerciseId,
            string Title,
            string Genre,
            EnglishLevel EnglishLevel,
            sbyte TotalQuestions,
            DateTime CreatedAt,
            DateTime ExpiresAt);

        public sealed record ListeningExerciseArchiveSummary(
            Guid ExerciseId,
            string Title,
            string GenreLabel,
            ListeningGenre Genre,
            EnglishLevel EnglishLevel,
            int TotalQuestions,
            DateTime CreatedAt,
            DateTime ExpiresAt,
            string FileName,
            long FileSizeBytes,
            ListeningExerciseArchiveRequest Request);

        private sealed record ListeningExerciseArchive(
            Guid ExerciseId,
            string Title,
            ListeningGenre Genre,
            string GenreLabel,
            EnglishLevel EnglishLevel,
            int TotalQuestions,
            string Transcript,
            string? AudioContent,
            IReadOnlyList<Quiz> Questions,
            DateTime CreatedAt,
            DateTime ExpiresAt,
            ListeningExerciseArchiveRequest Request);

        public sealed record ListeningExerciseArchiveRequest(
            ListeningGenre Genre,
            EnglishLevel EnglishLevel,
            sbyte TotalQuestions,
            string? CustomTopic,
            AiModel AiModel);
    }
}
