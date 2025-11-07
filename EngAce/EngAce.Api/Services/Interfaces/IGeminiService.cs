using EngAce.Api.Services.AI;

namespace EngAce.Api.Services.Interfaces;

public interface IGeminiService
{
    Task<List<GeneratedQuestion>> GenerateQuestionsAsync(string content, string exerciseType, string level, int questionCount = 5);
    Task<string> GenerateExplanationAsync(string questionText, string correctAnswer);
    Task<bool> TestConnectionAsync();
    Task<string?> GetRawGeminiResponseAsync(string content, string exerciseType, string level, int questionCount);
}