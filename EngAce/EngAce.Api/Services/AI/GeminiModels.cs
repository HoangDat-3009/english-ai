using System.Text.Json.Serialization;

namespace EngAce.Api.Services.AI;

public class GeminiRequest
{
    [JsonPropertyName("contents")]
    public List<Content> Contents { get; set; } = new();
    
    [JsonPropertyName("generationConfig")]
    public GenerationConfig GenerationConfig { get; set; } = new();
}

public class Content
{
    [JsonPropertyName("parts")]
    public List<Part> Parts { get; set; } = new();
    
    [JsonPropertyName("role")]
    public string Role { get; set; } = "user";
}

public class Part
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}

public class GenerationConfig
{
    [JsonPropertyName("temperature")]
    public double Temperature { get; set; } = 0.7;
    
    [JsonPropertyName("maxOutputTokens")]
    public int MaxOutputTokens { get; set; } = 4096;
    
    [JsonPropertyName("topK")]
    public int TopK { get; set; } = 40;
    
    [JsonPropertyName("topP")]
    public double TopP { get; set; } = 0.95;
}

public class GeminiResponse
{
    [JsonPropertyName("candidates")]
    public List<Candidate> Candidates { get; set; } = new();
}

public class Candidate
{
    [JsonPropertyName("content")]
    public Content Content { get; set; } = new();
    
    [JsonPropertyName("finishReason")]
    public string FinishReason { get; set; } = string.Empty;
    
    [JsonPropertyName("index")]
    public int Index { get; set; }
}

public class GeneratedQuestion
{
    [JsonPropertyName("questionText")]
    public string QuestionText { get; set; } = string.Empty;
    
    [JsonPropertyName("options")]
    public List<string> Options { get; set; } = new();
    
    [JsonPropertyName("correctAnswer")]
    public int CorrectAnswer { get; set; }
    
    [JsonPropertyName("explanation")]
    public string Explanation { get; set; } = string.Empty;
    
    [JsonPropertyName("difficulty")]
    public int Difficulty { get; set; } = 3;
}

public class Part7Response
{
    [JsonPropertyName("passage")]
    public string Passage { get; set; } = string.Empty;
    
    [JsonPropertyName("questions")]
    public List<GeneratedQuestion> Questions { get; set; } = new();
}