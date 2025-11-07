namespace EngAce.Api.Services.AI;

public class GeminiRequest
{
    public List<Content> Contents { get; set; } = new();
    public GenerationConfig GenerationConfig { get; set; } = new();
}

public class Content
{
    public List<Part> Parts { get; set; } = new();
}

public class Part
{
    public string Text { get; set; } = string.Empty;
}

public class GenerationConfig
{
    public double Temperature { get; set; } = 0.7;
    public int MaxOutputTokens { get; set; } = 1024;
    public int TopK { get; set; } = 40;
    public double TopP { get; set; } = 0.95;
}

public class GeminiResponse
{
    public List<Candidate> Candidates { get; set; } = new();
}

public class Candidate
{
    public Content Content { get; set; } = new();
    public string FinishReason { get; set; } = string.Empty;
    public int Index { get; set; }
}

public class GeneratedQuestion
{
    public string QuestionText { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public int CorrectAnswer { get; set; }
    public string Explanation { get; set; } = string.Empty;
    public int Difficulty { get; set; } = 3;
}