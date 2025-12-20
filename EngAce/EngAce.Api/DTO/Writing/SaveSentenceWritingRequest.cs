using System.Text.Json.Serialization;

namespace EngAce.Api.DTO.Writing;

public class SaveSentenceWritingRequest
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("topic")]
    public string Topic { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("sentences")]
    public List<SentenceWritingItem> Sentences { get; set; } = new();

    [JsonPropertyName("level")]
    public string? Level { get; set; }

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

public class SentenceWritingItem
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("vietnamese")]
    public string Vietnamese { get; set; } = string.Empty;

    [JsonPropertyName("correctAnswer")]
    public string CorrectAnswer { get; set; } = string.Empty;

    [JsonPropertyName("suggestion")]
    public SentenceWritingSuggestion? Suggestion { get; set; }
}

public class SentenceWritingSuggestion
{
    [JsonPropertyName("vocabulary")]
    public List<SentenceVocabItem> Vocabulary { get; set; } = new();

    [JsonPropertyName("structure")]
    public string Structure { get; set; } = string.Empty;
}

public class SentenceVocabItem
{
    [JsonPropertyName("word")]
    public string Word { get; set; } = string.Empty;

    [JsonPropertyName("meaning")]
    public string Meaning { get; set; } = string.Empty;
}


