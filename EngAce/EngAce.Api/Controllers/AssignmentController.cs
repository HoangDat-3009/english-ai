using EngAce.Api.DTO;
using Entities;
using Entities.Enums;
using Events;
using Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using EngAce.Api.Services.Interfaces;
using System.Text;
using System.Text.Json;

namespace EngAce.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssignmentController(IMemoryCache cache, ILogger<AssignmentController> logger, IGeminiService geminiService) : ControllerBase
    {
        private readonly IMemoryCache _cache = cache;
        private readonly ILogger<AssignmentController> _logger = logger;
        private readonly IGeminiService _geminiService = geminiService;
        private readonly string _accessKey = HttpContextHelper.GetAccessKey();

        [HttpPost("Generate")]
        public async Task<ActionResult<List<Quiz>>> Generate([FromBody] GenerateQuizzes request, string provider = "gemini")
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

            if (GeneralHelper.GetTotalWords(request.Topic) > QuizScope.MaxTotalWordsOfTopic)
            {
                return BadRequest($"Chủ đề không được chứa nhiều hơn {QuizScope.MaxTotalWordsOfTopic} từ");
            }

            if (request.TotalQuestions < QuizScope.MinTotalQuestions || request.TotalQuestions > QuizScope.MaxTotalQuestions)
            {
                return BadRequest($"Số lượng câu hỏi phải nằm trong khoảng {QuizScope.MinTotalQuestions} đến {QuizScope.MaxTotalQuestions}");
            }

            if (request.TotalQuestions < request.AssignmentTypes.Count)
            {
                return BadRequest($"Số lượng câu hỏi không được nhỏ hơn số dạng câu hỏi mà bạn chọn");
            }

            var cacheKey = $"GenerateQuiz-{request.Topic.ToLower()}-{string.Join(string.Empty, request.AssignmentTypes)}-{request.EnglishLevel}-{request.TotalQuestions}-{provider}";
            if (_cache.TryGetValue(cacheKey, out var cachedQuizzes))
            {
                return Ok(cachedQuizzes);
            }

            try
            {
                var prompt = BuildQuizPrompt(request.Topic, request.AssignmentTypes, request.EnglishLevel, request.TotalQuestions);
                var response = await _geminiService.GenerateQuizResponseAsync(prompt, provider);
                
                // Extract JSON from markdown code block if present
                var jsonContent = ExtractJsonFromResponse(response);
                
                var quizzes = JsonSerializer.Deserialize<List<Quiz>>(jsonContent) ?? new List<Quiz>();
                _cache.Set(cacheKey, quizzes, TimeSpan.FromDays(1));

                _logger.LogInformation("{_accessKey} generated with {provider}: {Topic} - Quiz Types: {Types}", _accessKey[..10], provider, request.Topic, string.Join("-", request.AssignmentTypes.Select(t => t.ToString())));

                return Created("Success", quizzes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Topic: {Topic} - Quiz Types: {Types}", request.Topic, string.Join("-", request.AssignmentTypes.Select(t => t.ToString())));
                return BadRequest(ex.Message);
            }
        }

        private string BuildQuizPrompt(string topic, List<AssignmentType> quizTypes, EnglishLevel level, short questionsCount)
        {
            var prompt = new StringBuilder();
            
            prompt.AppendLine(@"You are an expert English teacher with over 20 years of teaching experience.
Generate multiple-choice questions based on the requirements below:

### Requirements:
1. **English Proficiency Level**: " + level.ToString() + @"
   - A1: Beginner (simple sentences, basic vocabulary)
   - A2: Elementary (basic understanding of short texts)
   - B1: Intermediate (main points of clear standard input)
   - B2: Upper-intermediate (clear, detailed text on familiar topics)
   - C1: Advanced (well-structured text on complex subjects)
   - C2: Proficient (near-native fluency)

2. **Question Guidelines**:
   - Clear, direct, and unambiguous
   - Grammatically correct
   - Practical, real-world scenarios
   - Types: Vocabulary, Grammar, Contextual Understanding, Practical Situations

3. **Choices**: Provide 4 unique choices per question
   - One correct answer
   - Three plausible but incorrect distractors

4. **Explanation**: Brief explanation in Vietnamese for why the correct answer is right

## Output Format:
Return a valid JSON array with these fields:
- Question: The question text in English
- Options: Array of 4 unique choices
- RightOptionIndex: Index of the correct answer (0-based)
- ExplanationInVietnamese: Brief explanation in Vietnamese

Example:
```json
[
    {
        ""Question"": ""What is the capital of Japan?"",
        ""Options"": [""Seoul"", ""Beijing"", ""Tokyo"", ""Bangkok""],
        ""RightOptionIndex"": 2,
        ""ExplanationInVietnamese"": ""Tokyo là thủ đô của Nhật Bản.""
    }
]
```");

            prompt.AppendLine($"\n### Task:");
            prompt.AppendLine($"- Topic: {topic}");
            prompt.AppendLine($"- Quiz Types: {string.Join(", ", quizTypes.Select(t => t.ToString()))}");
            prompt.AppendLine($"- Level: {level}");
            prompt.AppendLine($"- Total Questions: {questionsCount}");
            prompt.AppendLine("\nGenerate the questions now in valid JSON format:");

            return prompt.ToString();
        }

        [HttpGet("SuggestTopics")]
        public async Task<ActionResult<List<string>>> SuggestTopics(EnglishLevel englishLevel = EnglishLevel.Intermediate)
        {
            if (string.IsNullOrEmpty(_accessKey))
            {
                return Unauthorized("Invalid Access Key");
            }

            const int totalTopics = 5;
            var cacheKey = $"SuggestTopics-{englishLevel}";

            if (_cache.TryGetValue(cacheKey, out List<string> cachedTopics))
            {
                return Ok(cachedTopics
                    .OrderBy(x => Guid.NewGuid())
                    .Take(totalTopics)
                    .ToList());
            }

            try
            {
                var topics = await QuizScope.SuggestTopcis(_accessKey, englishLevel);

                var selectedTopics = topics
                    .OrderBy(x => Guid.NewGuid())
                    .Take(totalTopics)
                    .ToList();

                _cache.Set(cacheKey, topics, TimeSpan.FromDays(1));

                return Created("Success", selectedTopics);
            }
            catch
            {
                return BadRequest("Không thể gợi ý chủ đề. Vui lòng thử lại.");
            }
        }

        /// <summary>
        /// Retrieves a dictionary of English levels with their descriptions
        /// </summary>
        /// <returns>
        /// An <see cref="ActionResult{T}"/> containing a dictionary of English levels and their descriptions if the operation is successful.
        /// </returns>
        /// <response code="200">Returns a dictionary of English levels and their descriptions.</response>
        [HttpGet("GetEnglishLevels")]
        [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any, NoStore = false)]
        public ActionResult<Dictionary<int, string>> GetEnglishLevels()
        {
            var descriptions = Enum
                .GetValues(typeof(EnglishLevel))
                .Cast<EnglishLevel>()
                .ToDictionary(level => (int)level, level => GeneralHelper.GetEnumDescription(level)
            );

            return Ok(descriptions);
        }

        /// <summary>
        /// Retrieves a dictionary of quiz types with their descriptions
        /// </summary>
        /// <returns>
        /// An <see cref="ActionResult{T}"/> containing a dictionary of quiz types and their descriptions if the operation is successful.
        /// </returns>
        /// <response code="200">Returns a dictionary of quiz types and their descriptions.</response>
        [HttpGet("GetAssignmentTypes")]
        [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any, NoStore = false)]
        public ActionResult<Dictionary<int, string>> GetAssignmentTypes()
        {
            var descriptions = Enum
                .GetValues(typeof(AssignmentType))
                .Cast<AssignmentType>()
                .OrderBy(t => GeneralHelper.GetEnumDescription(t))
                .ToDictionary(type => (int)type, type => GeneralHelper.GetEnumDescription(type)
            );

            return Ok(descriptions);
        }

        private static string ExtractJsonFromResponse(string response)
        {
            // Remove markdown code block markers if present
            var content = response.Trim();
            
            // Check for ```json ... ``` pattern
            if (content.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
            {
                // Find the first newline after ```json
                var startIndex = content.IndexOf('\n') + 1;
                // Find the closing ```
                var endIndex = content.LastIndexOf("```");
                
                if (startIndex > 0 && endIndex > startIndex)
                {
                    content = content.Substring(startIndex, endIndex - startIndex).Trim();
                }
            }
            // Check for ``` ... ``` pattern (without json)
            else if (content.StartsWith("```"))
            {
                var startIndex = content.IndexOf('\n') + 1;
                var endIndex = content.LastIndexOf("```");
                
                if (startIndex > 0 && endIndex > startIndex)
                {
                    content = content.Substring(startIndex, endIndex - startIndex).Trim();
                }
            }
            
            return content;
        }
    }
}