using EngAce.Api.Services.AI;

namespace EngAce.Api.Services.Interfaces;

/// <summary>
/// 🤖 Interface cho Gemini AI Service - Tạo bài tập TOEIC tự động
/// </summary>
public interface IGeminiService
{
    /// <summary>
    /// 🤖 Tạo câu hỏi từ Gemini AI (Part 5)
    /// </summary>
    Task<List<GeneratedQuestion>> GenerateQuestionsAsync(string content, string exerciseType, string level, int questionCount = 5);

    /// <summary>
    /// 🤖 Tạo giải thích cho câu hỏi
    /// </summary>
    Task<string> GenerateExplanationAsync(string questionText, string correctAnswer);

    /// <summary>
    /// 🤖 Test kết nối Gemini API
    /// </summary>
    Task<bool> TestConnectionAsync();

    /// <summary>
    /// 🤖 Lấy raw response từ Gemini (debug)
    /// </summary>
    Task<string?> GetRawGeminiResponseAsync(string content, string exerciseType, string level, int questionCount);

    /// <summary>
    /// 🤖 Tạo câu hỏi + passage từ Gemini AI (Part 6/7)
    /// </summary>
    Task<(List<GeneratedQuestion> questions, string passage)> GenerateQuestionsWithPassageAsync(string content, string exerciseType, string level, int questionCount = 5);
}