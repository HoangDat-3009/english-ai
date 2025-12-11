using System.Text.Json.Serialization;

namespace EngAce.Api.DTO
{
    public class SaveExerciseRequest
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("topic")]
        public string Topic { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string? Content { get; set; }

        [JsonPropertyName("questions")]
        public List<ExerciseQuestion> Questions { get; set; } = new();

        [JsonPropertyName("level")]
        public string? Level { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("category")]
        public string? Category { get; set; }

        [JsonPropertyName("estimatedMinutes")]
        public int? EstimatedMinutes { get; set; }

        [JsonPropertyName("timeLimit")]
        public int? TimeLimit { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("createdBy")]
        public int? CreatedBy { get; set; }
    }

    public class ExerciseQuestion
    {
        [JsonPropertyName("question")]
        public string Question { get; set; } = string.Empty;

        [JsonPropertyName("options")]
        public List<string> Options { get; set; } = new();

        [JsonPropertyName("rightOptionIndex")]
        public int RightOptionIndex { get; set; }

        [JsonPropertyName("explanationInVietnamese")]
        public string? ExplanationInVietnamese { get; set; }
    }
}
